# Maximizing Throughput with One Shared Connection

## Context and Problem Statement

This decision covers `src/AllenBradley.Logix/Client/`. It covers how the concurrent group read (see
[Reading and writing a group of tags](2026-07-16-reading-and-writing-a-group-of-tags.md)) maps onto
libplctag connections, and what the access factory does, and deliberately does not, set on each handle. It
settles the *connection layout*. The interface it sits on is [A testable interface over
libplctag](2026-07-16-testable-libplctag-interface.md), and the handle cache it would reshape is [Reusing
and releasing tag handles](2026-07-16-reusing-and-releasing-tag-handles.md). It was made under
[issue #5: Client base design](https://gitlab.com/vicione-oss/addons/allen-bradley/cip/-/work_items/5).

Our read groups are **poll-frequency classes**. Data points are grouped by how often they are polled, into
a fast loop, a slower loop, or a bulk loop. Each group is read as a set of concurrent per-tag operations,
and libplctag's C core (the native library under the .NET wrapper) packs requests that share a session
into a Multiple Service Packet (MSP), entirely on its own. An MSP is one CIP request carrying many tag
reads in a single network round-trip. A **session** here is one TCP connection to the controller plus the
CIP connection opened over it by a Forward Open, the handshake that claims one of the controller's
connection slots.

So how many sessions should the handles sit on? One shared session for the whole controller, or one
session per poll-frequency class?

Three library facts frame the choice, all documented under
[`libPlcTag/`](../../AllenBradley.Documentation/libPlcTag/README.md).

The session is shared by default, and it sends eagerly. Every handle with the same gateway, routing path
and PLC type joins **one** first-in-first-out queue, serviced by one session thread. That thread packs
whatever is queued the moment it can send
([the-shared-session.md](../../AllenBradley.Documentation/libPlcTag/the-shared-session.md)). The core
optimizes total throughput on one pipe. It knows nothing about our groups.

We cannot dictate what gets packed together, only exclude a tag or move it to another session. There is no
"bundle exactly these" API. There are two levers. `allow_packing` drops one tag out of packing, and
`connection_group_id` puts a tag on a separate session, guaranteeing separate packets even to the same
controller. `connection_group_id` is not exposed on the .NET `Tag` class, so using it means assembling the
raw attribute string (the key=value configuration string a handle is created from) by hand.

Sessions are a limited controller resource. Each distinct `connection_group_id` costs a socket, a Forward
Open, and one CIP connection slot. A ControlLogix or CompactLogix has a small, fixed pool of those slots,
shared with every other client talking to the controller.

The tension is real. The library optimizes total throughput on a shared connection, while the group model
wants each poll-frequency class's read to be predictable, unaffected by what the other classes are doing.
That is the classic trade-off between one shared queue (throughput) and separate connections
(predictability). Because our groups are keyed by poll frequency, the group boundary genuinely *is* a
scheduling boundary. Splitting by group would therefore not be arbitrary here. It is the one axis on which
a separate connection could be justified.

Two facts pull the other way, and are why we do **not** split the connection yet:

1. A separate connection buys scheduling independence, not resource independence. Separate sessions have
   separate queues, but they still share one wire, one controller CPU, and one connection-slot pool. The
   isolation is only partial.
2. Poll-frequency classes mostly do not fire at the same time. They run on different timers, so a shared
   session already separates them *in time*. For most of every cycle, the fast class has the queue to
   itself. The classes compete only in the moments their timers coincide. Even then, packing them together
   leaves partial failure untouched, because a bad tag still reports per request, and it helps timestamp
   coherence, because tags read in one packet are *more* time-coherent rather than less. The only property
   at risk is a class's worst-case latency when timers coincide, and that can be defended more cheaply
   than by spending a session.

## Considered Options

- **Option 1: One shared session per controller.** No `connection_group_id`, so every poll-frequency
  class shares the controller's single session. Classes are separated by time (their timers
  rarely collide), and worst-case latency is defended by capping the number of in-flight requests
  and sizing the timeout to match, not by a separate connection. Maximum packing, fewest
  connections, one lifecycle to connect and dispose.
- **Option 2: One session per poll-frequency class.** Each class gets its own `connection_group_id`, so
  another class firing at the same moment can never queue ahead of it. That is real, class-aligned
  isolation, paid for with hand-assembled attribute strings (off the typed `Tag` API), one
  Forward Open and one controller slot per class, and one connect/reconnect/dispose lifecycle per
  class.
- **Option 3: One session per group, whatever the reason for the group.** Splits the shared queue into
  many thin ones and spends a controller slot per group, including for groups that exist for
  delivery reasons (which data goes to which consumer) rather than scheduling reasons.

## Decision Outcome

Chosen: **Option 1 now, Option 2 deferred, Option 3 rejected.**

The client runs **one shared session per controller**. The access factory sets the typed connection
attributes, meaning `Gateway`, `Path`, `PlcType`, `Protocol`, `Name`, `Timeout` and `AllowPacking`, and it
does not set `connection_group_id`. All handles to a controller therefore share its session and pack
together. `AllowPacking` is the library's default and is set explicitly because it is the lever the whole
decision rests on. Packing is what turns a group of concurrent reads into one round-trip instead of *N*.

Independence between the poll-frequency classes comes from their separation in time, defended by two
things rather than by separate sessions.

The per-operation timeout is sized for the worst case of draining the shared queue, not for a single
exchange. A tag at the back of a queue several packets deep waits several round-trips before its own
answer arrives. An under-sized timeout brings back the timeout-then-abort cascade the testable-interface
ADR exists to avoid. It is configured per device on the connection record, 5 s by default with a 100 ms
floor, because what has to drain is *that* controller's queue.

The second defence is a cap on in-flight requests, so the shared queue can never grow deeper than a
class's timeout can absorb. **That cap does not exist yet.** A group still starts one operation per tag
with no ceiling (see [Reading and writing a group of
tags](2026-07-16-reading-and-writing-a-group-of-tags.md)), so the timeout currently stands alone. Both the
ceiling and the timeout sizing that matches it are outputs of the throughput measurement against the real
controller, and that measurement has not been run.

Option 2 is a **deferred optimization**, not a rejected idea. It waits on a measurement showing the
classes actually compete, and it would then be applied narrowly, to one class, rather than spread across
every group. It is not one to reach for by default.

Promote Option 2 for a class only when a measurement on the shared session shows that, in the moments the
timers coincide, a fast or critical class's read latency exceeds a hard budget. The realistic culprit
would be a slow bulk class firing at the same moment with reads so large they must be split across
packets. A transfer split that way occupies whole packets by itself and blocks other requests from packing
alongside it. The trigger is a hard latency requirement that separation in time cannot guarantee. A soft,
best-effort target is served by timeout sizing on the shared session. When adopting Option 2, separate by
class and cap it at two or three sessions, with the shared session as the fallback for the tail of rarer
frequencies. Never one session per group. The lifecycle-keying change described below comes with it.

### Consequences

One thing is left for later work, the throughput measurement that has to produce the in-flight cap and the
per-operation timeout to match it. Until it runs, the configured timeout is a plausible default rather
than a sized number, and there is no cap at all.

Growing a per-class connection later stays cheap, which is the constraint this decision puts on the client
lifecycle. The client pool reference-counts one client per connection record and knows nothing else about
scoping. Option 2 is therefore a change to that record, which would gain the poll class, and to the mapper
that builds it. Nothing in the pool, the factory or the tag manager would have to move. The per-operation
timeout rides on the same record, which is where the sizing this ADR asks for is configured.

### Enforcement

Compliance is a code-review check on the factory. It sets only the typed connection attributes, and
`connection_group_id` stays unused until a measurement shows it is needed.

## Pros and Cons of the Options

### Option 1: One shared session per controller (chosen)

#### Pros

It is the simplest layout with the most packing. One fully fed queue is exactly what the core optimizes,
so we inherit its throughput instead of splitting it up. It also spends the fewest controller resources,
one socket, one Forward Open, and one connection slot per controller, which leaves the controller's small
slot pool for other clients. And it is one lifecycle to get right, a single session to connect on acquire
and dispose explicitly on release (see [Reusing and releasing tag
handles](2026-07-16-reusing-and-releasing-tag-handles.md)). One session per class would multiply both the
connection setup and the explicit-disposal work that prevents the `0xC0000602` shutdown crash. Staying on
the typed `Tag` API also means no hand-assembled attribute strings, so the factory stays small and typed.

#### Cons

The fast class gets no *hard* latency guarantee when timers coincide. Its bound is best-effort, held by
the timeout sizing and, once it exists, the in-flight cap. A class that genuinely cannot slip is exactly
the case that reopens Option 2. Timeout sizing also becomes load-dependent. The per-operation timeout must
cover the worst-case queue drain, which grows with group size and shrinks with the negotiated packet size.

### Option 2: One session per poll-frequency class (deferred)

#### Pros

It gives real, class-aligned isolation. Another class firing at the same moment can never queue ahead
of a reserved class.

#### Cons

It costs hand-assembled attribute strings (off the typed `Tag` API), plus a Forward Open, a controller
slot, and a connect/reconnect/dispose lifecycle per class. Held as a future optimization, applied narrowly
and never uniformly.

There is also a keying change a later Option 2 carries. Today the choice is invisible in the code, because
everything is keyed by controller. The connection record is gateway plus path plus per-operation timeout,
and the handle cache is scoped per device. Under Option 2 the session identity
would also carry the poll class. The clean mapping is then one manager and one session per class per
device, where each poll-frequency DataPort has its own manager, its own `connection_group_id`, and its own
handle cache, instead of one manager per device. Keeping that move cheap is a standing constraint on the
lifecycle. The per-device scoping the handles ADR describes must not be treated as the *only* possible
scoping axis.

### Option 3: One session per group, whatever the reason for the group (rejected)

#### Cons

It splits the shared queue into many thin ones and spends a hard controller slot per group, in exchange
for isolation that is only partial anyway. The sessions still share one wire, one controller CPU, and one
slot pool. It also separates on the wrong boundary. A group that exists to route data to a consumer says
nothing about *when* its tags are polled, so a session per such group buys no scheduling benefit at all.
Option 2, not Option 3, is the future path, precisely because Option 2's boundary is the scheduling
boundary.

## More Information

These open questions are inputs to the planned throughput measurement against the real controller
(issue #5), and gate any move to Option 2:

- Do the classes actually compete? On the shared session, when the bulk class fires at the same moment as
  the fast class, does the fast class's read latency exceed its budget? By how much? And is the cause the
  number of requests, or a single large transfer split across packets?
- What in-flight limit and per-operation timeout bound a class's latency across realistic group sizes and
  packet sizes? The negotiated packet size is roughly 504 bytes on a standard connection, and around 4000
  bytes when the controller accepts the extended Forward Open, a variant of the connection handshake that
  negotiates larger packets.
- Whether to request the extended Forward Open at all. Larger packets mean denser packing and a faster
  queue drain, which directly changes the answers above.

- Related: [Reading and writing a group of tags](2026-07-16-reading-and-writing-a-group-of-tags.md)
  (the concurrent group read this connection layout carries) ·
  [Reusing and releasing tag handles](2026-07-16-reusing-and-releasing-tag-handles.md)
  (the per-device cache this layout would reshape)
- libplctag behaviour:
  [the-shared-session.md](../../AllenBradley.Documentation/libPlcTag/the-shared-session.md)
  (sharing, packing, and the levers for controlling it)
- [Issue #5: Client base design](https://gitlab.com/vicione-oss/addons/allen-bradley/cip/-/work_items/5)
