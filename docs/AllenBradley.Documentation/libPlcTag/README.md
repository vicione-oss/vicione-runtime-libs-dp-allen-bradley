# libplctag Behaviour

How the [libplctag](https://github.com/libplctag/libplctag) native library and its
[.NET wrapper](https://github.com/libplctag/libplctag.NET) actually behave. These are the facts
about our dependency that shape the client design. Where [`cip-protocol/`](../cip-protocol/README.md)
documents CIP and EtherNet/IP as ODVA defines them on the wire, this folder documents the *library
we drive that wire through*. It records what the library does that the CIP spec does not dictate, and
the quirks that have cost us debugging time.

These are **library facts, not our decisions.** They are the inputs the client ADRs consume, and the
design that responds to them lives in the Logix client ADRs
([a testable interface over libplctag](../../AllenBradley.Logix.Documentation/ADR/2026-07-16-testable-libplctag-interface.md)
onward).
Each document says where the behaviour comes from (a source line in the C core or the wrapper, or a
test that pins it against the real device), so a claim here can be re-verified rather than trusted.

The docs split on the two libplctag object concepts: the implicit **shared session** that every handle
to a controller funnels through, and the individual **per-tag handle** you drive.

| Document | What it covers |
|----------|----------------|
| **The shared session** | |
| [the-shared-session.md](the-shared-session.md) | The one CIP session shared across every handle to a gateway/path/PLC, and request packing end to end: **whether** requests pack (`allow_packing` by PLC type), **when and how large** a pack gets (the self-clocking thread, no linger timer, throughput as a function of requests-in-flight), and **what you can steer** (exclude a tag or segregate a session, but never compose an exact bundle). It also covers why the reusable unit is the warm handle |
| **The per-tag handle** | |
| [concurrent-operations-on-a-handle.md](concurrent-operations-on-a-handle.md) | What happens when two operations overlap on one handle: the native `PLCTAG_ERR_BUSY` guard, the wrapper's LIFO completion mispairing, and the raw-buffer race below it |
| [tag-disposal-and-shutdown.md](tag-disposal-and-shutdown.md) | Why every `Tag` must be disposed deterministically. Native handles finalized after CLR teardown fail-fast the process with `0xC0000602` |

## The one fact to carry into every other document

**libplctag does not batch on a timer. It batches on the in-flight round-trip.** There is no delay
you can tune to make packing happen. Requests co-pack only because they arrive while the session
thread is busy with the previous packet. Every throughput question about this library reduces to
*how many requests are in flight at once*, and every document here is a facet of that. See
[the-shared-session.md](the-shared-session.md) for the mechanism.
