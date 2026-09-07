# Reading and Writing a Group of Tags

## Context and Problem Statement

This decision covers the client and its read and write batches, under `src/AllenBradley.Logix/Client/`. It
was made under
[issue #5: Client base design](https://gitlab.com/vicione-oss/addons/allen-bradley/cip/-/work_items/5).

The DataPort read hands the client a **group** of data points and expects a list of typed values back. The
write hands it a list of typed values and returns a bare `ValueTask`. libplctag has no call that reads
several tags at once (see [A testable interface over
libplctag](2026-07-16-testable-libplctag-interface.md)), so a group cannot be one wire call. It is *N*
separate handle operations.

What makes *N* operations acceptable is packing. libplctag's C core (the native library under the .NET
wrapper) services all handles to one controller from a single queue, and bundles whatever is waiting in
that queue into a Multiple Service Packet (MSP), one CIP request that carries many tag reads in a single
network round-trip. Whether packing happens depends on how many requests sit in the queue at the same
moment. Start the whole group at once and the core can pack it. Read the group in a blocking loop and the
queue never holds more than one request, so nothing ever packs (see
[the-shared-session.md](../../AllenBradley.Documentation/libPlcTag/the-shared-session.md)).

## Considered Options

- **Option 1: Resolve every converter and tag up front**, then start one operation per tag
  at once and await them all, handling failure per tag
- **Option 2: Hand-roll the Multiple Service Packet** ourselves over `libplctag.NativeImport`
- **Option 3: Mirror the S7 addon's batch**, where the whole group is one client-library call

## Decision Outcome

Chosen: **Option 1, resolve everything before any I/O**. Resolve every converter (see [Decoding tag bytes
into typed values](2026-07-16-decoding-tag-bytes-into-typed-values.md)) and every tag (see [Reusing and
releasing tag handles](2026-07-16-reusing-and-releasing-tag-handles.md)), then start one operation per tag
at once and await them all. Starting them concurrently is what fills the core's queue and makes its
packing engage. And libplctag offers no batch call we could mirror instead.

On the write side, resolving goes one step further. Constructing the batch also has the converter encode
each value into the bytes it occupies, sized from the type or the configured capacity and from nothing
the handle knows. A value that will not encode, such as a string longer than its tag was declared to
hold, therefore fails before any tag is touched, instead of leaving half the batch written. Where the
conversion sits is the one place the two directions differ: a decode needs the reply and so happens per
entry, an encode needs nothing from the device and so happens up front. Whether the bytes fit the tag on
the controller is libplctag's check: its handle is the controller's width and refuses a longer payload
before sending, which the adapter reports as a failed outcome for that tag.

Failure is **detected** per tag and **reported** per batch. Every tag is attempted — none of them stops
its siblings — and the failures are combined afterwards into one exception that **names every failed tag
and its reason**. We collect them as results rather than letting them throw, because `Task.WhenAll`
rethrows only the *first* exception of a set, and a caller told about one failed tag out of five would go
looking in the wrong place. Both directions do this, and the exception is `LogixTagException` in both.
The encode loop follows the same rule from before any I/O: a batch holding three values that will not
fit their tags names all three, and says that nothing was sent.

The group is therefore the **unit of delivery**, the same contract the sibling Siemens S7 addon's batch
has (`Siemens.S7.Absolute/Client/S7NetPlusClient.cs`). A read returns a typed value for every point in
the group, or it throws and returns none. What rules out returning the group short, or full-length with
a placeholder standing in for the tag that failed, is where those values end up:
`IncomingDataPortBase.ToExternalValue` hard-codes `Validity = 1` and passes `IDataPointValue.Value`
through as it is. A missing reading would reach the engine as a **valid null**, indistinguishable from a
tag that genuinely holds nothing, and the engine would act on it. A group with an invisible hole in it is
worse than no group at all. Marking those readings invalid instead is possible — override
`ToExternalValue` and map onto `Validity` — but that is a decision about what the engine should see, and
it belongs to the port rather than to the client.

Failing costs one poll and nothing more. The failure is logged with the batch size and the controller,
and the polling job above catches it, counts it against the circuit breaker and tries again on the next
tick, so a transiently unreadable tag delays a group rather than corrupting it.

A reply too short for the type the point was configured as fails the group like any other tag failure. It
is caught narrowly on the read path, only so the tag it happened to can be named rather than a bare
`ArgumentException` travelling with no address in it. On a verified tag it should not happen at all.

Cancellation flows into every operation and still throws, because cancelling is the caller's decision and
not the device's answer.

### Consequences

The group starts one operation per tag with no upper bound, so a concurrency cap must be set before this
runs in production. That cap, and the in-flight load one shared connection can absorb, come from the
planned throughput measurement (see [Maximizing throughput with one shared
connection](2026-07-16-maximizing-throughput-with-one-shared-connection.md)). The same measurement must
confirm the core actually packs concurrent reads into one Multiple Service Packet, by counting round-trips
for *N* concurrent reads on one shared session, since the whole approach rests on it. If the core does not
pack them, this decision still stands, but the throughput story changes. The fallback is to accept *N*
round-trips or move to a newer libplctag batch facility, never a hand-rolled packet.

### Enforcement

The client suite drives the client against a fake tag manager whose tags are in-process fakes over the
access seam, and it holds the contract in place. A data point with no converter aborts before anything is
read. A failed read, and a reply too short to decode, each raise one exception naming the tag; so do
failed writes, and several failed tags in one batch are all named in it — including several values that
will not encode, which are named without a byte being sent. A group where every tag answers returns a
value per point in the group's own order. A cancelled batch raises the cancellation itself and touches
no tag. The device tier round-trips a value through the same batches against the real controller.

The model carries no valueless value shape and no quality flag, so there is nothing for a read to return
in place of a value it does not have.

## Pros and Cons of the Options

### Option 1: Resolve every converter and tag up front, then start all at once (chosen)

#### Pros

Every tag is attempted independently, so a failing one never hides another and the exception can name
the whole set — better diagnostics than a single-call batch, which reports whichever failure the library
noticed first. We also inherit the core's packing instead of owning any CIP encoding ourselves.

#### Cons

The fan-out is currently unbounded. A group of 200 data points starts 200 concurrent operations. Bounding
that fan-out is an obligation handed to the throughput work (see Consequences above). Duplicate data
points in one group also serialize. Two equal data points share one synchronized tag, so their
"concurrent" reads block each other into two round-trips instead of one. That is a missed chance to
de-duplicate before I/O rather than a correctness problem.

### Option 2: Hand-roll the Multiple Service Packet (rejected)

Drop to `libplctag.NativeImport` (the raw bindings to the C core) and encode the CIP packet
ourselves.

#### Cons

It is only worth doing if we owned the whole wrapper layer, an option already rejected in [A testable
interface over libplctag](2026-07-16-testable-libplctag-interface.md). And it buys nothing, because
the core already packs.

### Option 3: Mirror the S7 addon's one-call batch (rejected)

In the sibling Siemens S7 addon, the client library (S7.Net) exposes a read-multiple call that takes the
whole batch at once and updates each item's value in place. The S7 batch classes are built around it.
What was rejected is the *call shape*, not the failure contract — we share the latter, and the group is
the unit of delivery in both addons.

#### Cons

libplctag has no such call, so the model does not port. Starting *N* operations concurrently is what
stands in for it.

## More Information

- [the-shared-session.md](../../AllenBradley.Documentation/libPlcTag/the-shared-session.md)
  (why starting reads concurrently is what makes packing engage)
- Related: [A testable interface over libplctag](2026-07-16-testable-libplctag-interface.md) ·
  [Maximizing throughput with one shared connection](2026-07-16-maximizing-throughput-with-one-shared-connection.md) ·
  [Reusing and releasing tag handles](2026-07-16-reusing-and-releasing-tag-handles.md) ·
  [Decoding tag bytes into typed values](2026-07-16-decoding-tag-bytes-into-typed-values.md)
- S7 precedent: its client's data-item batch classes, one per direction
- [Issue #5: Client base design](https://gitlab.com/vicione-oss/addons/allen-bradley/cip/-/work_items/5)
