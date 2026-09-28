# The Shared Session and Request Packing

libplctag has no session or connection object you open and hand around. You create per-tag handles
(`Tag`), and the native core transparently shares one CIP session (one EtherNet/IP registration plus
one Forward Open) across every handle that targets the same gateway, path, and PLC type. Sharing is
on by default (`share_session = 1`).

That shared session is where packing happens. Several CIP requests are combined into one Multiple
Service Packet (CIP service `0x0A`, `AB_EIP_CMD_CIP_MULTI`, addressed to the Connection Manager at
class `0x02` / instance `0x01`) so a single TCP round-trip services many tags. Both reads and writes
can be packed.

This document is the shared session end to end: what it shares, why the reusable unit is the warm
handle, whether requests pack, when and how large a pack gets, and what you can and cannot steer
about which tags share a packet. These are library facts, not our decisions. They are the inputs the
client ADRs consume. The design that responds to them lives in the Logix client ADRs, above all
[maximizing throughput with one shared connection](../../AllenBradley.Logix.Documentation/ADR/2026-07-16-maximizing-throughput-with-one-shared-connection.md)
and [reading and writing a group of tags](../../AllenBradley.Logix.Documentation/ADR/2026-07-16-reading-and-writing-a-group-of-tags.md).

## The one fact to carry

libplctag does not batch on a timer. It batches on the in-flight round-trip. There is no delay you
can tune to make packing happen. Requests co-pack only because they arrive while the session thread
is busy with the previous packet. Every throughput question about this library reduces to how many
requests are in flight at once. Hold that while reading the rest.

## One session, shared by gateway/path/PLC

Two independent `Tag` handles for two different tags on the same controller do not each get their own
connection. They enqueue onto one shared request queue and are serviced by one session thread. This
is the mechanism that makes packing possible. Without shared sessions there would be nothing for the
packer to bundle.

| Attribute | Effect |
|-----------|--------|
| `share_session` (default `1`) | Handles to the same gateway/path/PLC share one session + Forward Open |
| Gateway + path + PLC type | The identity the core matches on to decide sharing |

Because sharing is keyed on those three attributes, handles that differ in gateway, path, or PLC type
get separate sessions and cannot co-pack. That detail matters if a single client ever spans more than
one controller.

## The reusable unit is the warm handle

There is no connection handle to cache, so the expensive, reusable thing is the initialized `Tag`.
First use of a handle lazily performs the session registration, Forward Open, and symbol resolution.
Every read/write after that reuses them. A handle that has been read once is "warm", and throwing it
away discards that setup cost.

The client design leans on two consequences. Handles are keyed by the tag they address, so two data
points naming the same tag with the same shape can share one handle and one warm connection. This is
why the handle-reuse ADR caches tags by the data point itself and scopes the cache by device (gateway plus path
plus PLC type), the same three attributes the core uses to decide session sharing. And the
connection is a shared resource with a finite in-flight ceiling. Because every handle to a controller
funnels through one session thread, the useful degree of parallelism is bounded by that connection,
not by the number of handles. Sizing it is one of the ADR's open questions for the device probe.

## Whether requests pack: `allow_packing`

### The per-tag default is set by PLC type

`ab_common.c:254-304`. Only ControlLogix/CompactLogix actually honors the attribute:

```c
case AB_PLC_LGX:
    tag->allow_packing = attr_get_int(attribs, "allow_packing", 1);  // default ON
```

Every other type hard-forces `allow_packing = 0`: PLC5, SLC, MLGX, LGX_PCCC, Micro800, and the
Generic CIP type (`ab_common.c:257,262,267,272,287,296`). Passing `allow_packing=1` to a Micro800
or generic device therefore has no effect. Each read/write setup site in `eip_cip.c` then copies the
tag's flag onto its request (`req->allow_packing = tag->allow_packing`, e.g. lines 458, 606, 759).

### Combining happens in the session thread, not at the tag

The decision lives in `process_requests()` (`session.c:1760`), the loop that drains the shared queue
each `SESSION_IDLE` cycle. Packing only bundles requests sitting in the queue at that same instant.
The bundling logic (`session.c:1821-1868`):

1. Always pull request #0. If it does not even fit the available payload, nothing is sent this cycle.
2. Start packing only if request #0's `allow_packing` is true and more requests are queued.
3. Subtract multi-request overhead (the `cip_multi_req_header` plus a 2-byte offset entry per request).
4. Loop over the remaining queued requests, breaking on the first one that is not packable, does
   not fit remaining space, or once `MAX_REQUESTS` (400) is hit.

Available space is the Forward-Open-negotiated payload minus CPF/path overhead
(`session_get_available_cip_payload_space`, `session.c:306`). Then `pack_requests()` (`session.c:2283`)
writes the multi-header and per-request offsets, and `unpack_response()` (`session.c:2085`) splits the
single response back out by offset, so per-request status survives packing.

## When and how large a pack is

### The session thread is self-clocking, not timed

There is no linger window and no batch timer. You cannot configure libplctag to "wait a few
milliseconds and collect more requests." The thread's cycle is to grab the front request, greedily
pack every following packable request that still fits, send one packet, await the round-trip, and
repeat. It does not sleep between a request arriving and sending it. A newly enqueued request wakes
the thread immediately (`session.c:1260`). The only idle wait, `SESSION_IDLE_WAIT_TIME` (100 ms),
applies when the queue is empty, and it is a housekeeping poll, not a batching delay.

### Why requests batch at all

Because awaiting the round-trip blocks the thread. The effective "wait" before a request is sent is
the current in-flight round-trip, nothing more. With an idle, empty queue, a request arrives, the
thread is free, and it goes out immediately as a batch of one. No packing, because there was nothing
to pack it with. Under load, while the thread sends and awaits packet *N*, further requests pile up.
When *N*'s round-trip completes, the next cycle finds several waiting and packs them into packet
*N+1*.

This is the counter-intuitive part. More concurrency produces larger natural batches and higher
throughput. Reading tags one at a time (issue read, block until it returns, issue the next) keeps
exactly one request in the queue, so every packet carries a single tag and packing never engages, even
with `allow_packing` on.

### What bounds a single pack

Once the thread decides to pack, the batch grows until the first of these stops it:

| Bound | Value / rule |
|-------|--------------|
| Packing enabled | `allow_packing` on (default for ControlLogix/CompactLogix), forced off for PLC5, SLC, MicroLogix, Micro800, and Generic CIP |
| Request count | up to `MAX_REQUESTS` = 400 (`session.c:53`) |
| Payload size | must all fit one negotiated CIP payload (~504 B connected, or ~4000 B with the extended Forward Open) |
| First request | always sent even if it is unpackable or fills the packet alone (a batch of one) |
| Contiguity | packing stops at the first non-packable request from the front of the queue, even if packable requests sit behind it |
| Fragmentation | a large tag whose transfer must fragment consumes whole packets, so it effectively does not co-pack |

## What you can steer: exclude and segregate, never compose

The blunt fact is that there is no exact-bundle API. You cannot tell libplctag "pack A, B and C into
one packet." Composition is decided entirely by the session thread from queue order (FIFO) plus
available space. Every lever is subtractive. You can exclude a tag from packing or segregate a set
onto its own session, but you can never compose a specific bundle. Design around influence, not
control.

| Lever | Effect | Exposed in .NET `Tag`? |
|-------|--------|------------------------|
| `allow_packing` (`AllowPacking`) | Per-tag on/off. `false` → the tag is never bundled; it is always sent alone. `true` (default for ControlLogix/CompactLogix) → eligible, nothing more. The only direct switch. | Yes |
| `connection_group_id` | Per-tag session selector. Different id ⇒ different session/socket ⇒ guaranteed separate packets, even to the same PLC. The only hard segregation. | No, needs the raw attribute string |
| Submission timing | Soft and indirect. Firing tags concurrently lets them pack; awaiting each in turn keeps them apart. No guarantee, it only changes what sits in the queue in one thread cycle. | n/a |
| Size bounds | Implicit ceilings, not knobs: `MAX_REQUESTS` = 400 and one CIP payload (~504 B / ~4000 B extended). | n/a |

Setting `allow_packing = false` excludes one tag, which is then sent in its own packet every time.
Use it to stop a known-pathological tag (one that always fragments, or whose latency you want
isolated) from sitting at the front of the FIFO queue and, by the contiguity rule, blocking the tags
queued behind it.

A distinct `connection_group_id` segregates a set. Different id, different session, separate packets,
guaranteed. It is the tool for head-of-line isolation between classes of tags, such as a slow or
large polling set on its own id so its round-trips never delay a fast set. The cost is a second
Forward Open and a second in-flight ceiling. The .NET wrapper's `Tag` does not surface it, so using
it means building the raw libplctag attribute string rather than setting the typed properties.

Rule of thumb: use `allow_packing = false` to exclude a tag, and a distinct `connection_group_id` to
segregate a set. To maximise packing, set `allow_packing = true` and keep many concurrent tags on one
session. Exact bundling is not controllable.

## What this means for the client

The fan-out rides the core's packing. The group read/write ADR executes a group as *N* concurrent
per-access reads awaited together, because firing the whole group at once is the only way the core
sees enough queued requests to pack them. A loop of blocking reads would keep one request in the
queue and never pack.

The cache key mirrors the sharing key. The handle-reuse ADR caches tags by the data point and scopes
them by device (gateway + path + PLC type), the same identity the core shares sessions on.

The group is not, and cannot be made into, a packing unit. There is no call mapping a group onto a
packet, so the group is a delivery/partial-failure unit. Throughput is a property of how saturated
the shared queue is. The shared-connection ADR settles the session topology (one shared session per
controller, with `connection_group_id` segregation deferred) on exactly these facts.

## Source references

| Location | Role |
|----------|------|
| `ab_common.c:254-304` | Sets per-tag `allow_packing` default by PLC type |
| `eip_cip.c` (458, 606, 759, 948, 1096, 1281) | Copies the tag flag onto each request |
| `session.c:1236` | `session_add_request` enqueues onto the shared queue |
| `session.c:1260` | A new request wakes the session thread immediately |
| `session.c:1760` | `process_requests`, the self-clocking send/pack loop |
| `session.c:1821-1868` | The bundling decision (allow_packing, count, space, contiguity) |
| `session.c:306` | `session_get_available_cip_payload_space`, the space budget |
| `session.c:2283` / `:2085` | `pack_requests` builds the MSP; `unpack_response` splits it back out |
| `session.c:53` | `MAX_REQUESTS` = 400 cap |
| `SESSION_IDLE_WAIT_TIME` (100 ms) | Empty-queue housekeeping poll, not a batching delay |
| `share_session` (default `1`) | Session sharing across handles to one gateway/path/PLC |
| `connection_group_id` attribute | Distinct ids ⇒ distinct sessions ⇒ separate packets; not exposed on the .NET `Tag` |
| libplctag.NET `Tag.cs` | Exposes `AllowPacking`; does not expose `connection_group_id` |
