# Maximizing Throughput with One Shared Connection

## Context and Problem Statement

The client reads a group of tags as concurrent operations, one for each tag
([Reading and writing a group of tags](2026-07-16-reading-and-writing-a-group-of-tags.md)). The data points are grouped
by poll frequency, for example a fast class, a slow class and a bulk class. This decision sets how many libplctag
sessions these operations use. The code is the access factory in `src/AllenBradley.Logix/Client/Tags/Access/`. This
decision was made under
issue #5: Client base design.

In this ADR, "session" has the libplctag meaning. It is one EtherNet/IP session plus the CIP connection that a Forward
Open opens over it.

Three facts about libplctag frame the decision
([the shared session](../../AllenBradley.Documentation/libplctag/the-shared-session.md)):

1. All handles with the same connection endpoint, route path and PLC type share one session. The session has one
   queue. Its thread packs all waiting requests into one Multiple Service Packet (MSP), which is one network round trip.
2. We cannot select which requests go into one packet. `allow_packing=false` removes one tag from packing.
   setting a different `connection_group_id` puts a tag on a separate session. The .NET `Tag` class does not have `connection_group_id`, so
   it needs a raw attribute string.
3. Each session uses one CIP connection slot on the controller. A controller has only a small number of slots, and
   other clients use them too.

One shared session gives the highest throughput. Separate sessions give each poll class a more predictable latency.
Two facts speak against separate sessions. First, separate sessions still share one network, one controller and one
slot pool. Second, the poll classes run on different timers, so they seldom send at the same time.

## Considered Options

1. **One shared session per controller.** All poll classes use one session. A timeout and a limit on the requests in
   flight protect the latency.
2. **One session per poll-frequency class.** Each class gets its own `connection_group_id`. No other class can queue in
   front of it.
3. **One session per group,** also for groups that only route data to a consumer.

## Decision Outcome

Chosen option: **Option 1, one shared session per controller**. Option 2 is deferred. Option 3 is rejected.

Option 1 packs the most requests into each packet, and it uses the fewest connection slots. The poll classes are
already separate in time, so they compete only when their timers coincide.

- The access factory sets only the typed attributes of `Tag`: `Gateway`, `Path`, `PlcType`, `Protocol`, `Name`,
  `Timeout` and `AllowPacking`. It does not set `connection_group_id`.
- `AllowPacking` is `true`. This is the libplctag default. We set it explicitly because the decision depends on it.
- The client pool gives the incoming port and the outgoing port of one controller the same client. Thus, reads and
  writes use the same session and can go into the same packet.
- The operation timeout is set per device, with a default of 5 seconds. It must be long enough for a tag at the end of
  a full queue.
- A limit on the requests in flight must keep the queue shorter than the timeout allows.

Change to Option 2 only if a measurement shows this: when the timers coincide, a fast class misses a hard latency
limit. Then give only that class its own session. Use a maximum of two or three sessions. All other classes stay on the
shared session.

### Consequences

- Good: One full queue gives the most packing.
- Good: Each controller uses one connection slot and has one lifecycle to connect and dispose.
- Good: The factory uses only the typed `Tag` API, not a raw attribute string.
- Bad: A fast class has no hard latency guarantee when the timers coincide.
- Bad: The correct timeout depends on the load. It increases with the size of the groups.
- Open: The limit on the requests in flight does not exist yet. The 5-second timeout is a default, not a measured
  value. Both need a throughput measurement against a real controller.
- Open: Option 2 must stay easy to add. The client pool keys each client on `LogixClientInformation`. For Option 2,
  this key gets the poll class. The pool, the factory and the tag manager do not change.

## Why Not the Other Options

### Option 2: One session per poll-frequency class (deferred)

This option gives real isolation between the classes. It costs a raw attribute string, and one connection slot and
one lifecycle for each class. We add it only when a measurement shows the need.

### Option 3: One session per group (rejected)

This option splits the queue into many small queues and uses one connection slot for each group. A group that routes
data to a consumer says nothing about when its tags are polled. Thus, a separate session for it gives no scheduling
benefit.

## More Information

Open questions for the throughput measurement:

- Do the classes compete? When the bulk class and the fast class poll at the same time, how much slower is the fast
  class?
- Which limit on the requests in flight, and which timeout, keep each class in its latency budget?
- Must the client request the extended Forward Open? It negotiates packets of about 4000 bytes, not about 504 bytes.
  Larger packets empty the queue faster.

Links:

- Explanation: [Connecting](../explanation/client/connecting.md)
- Background: [The shared session](../../AllenBradley.Documentation/libplctag/the-shared-session.md)
- Related decisions: [Reading and writing a group of tags](2026-07-16-reading-and-writing-a-group-of-tags.md) ·
  [Reusing and releasing tag handles](2026-07-16-reusing-and-releasing-tag-handles.md)
