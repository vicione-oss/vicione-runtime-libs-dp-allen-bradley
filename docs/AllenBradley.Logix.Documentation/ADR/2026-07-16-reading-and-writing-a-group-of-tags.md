# Reading and Writing a Group of Tags

## Context and Problem Statement

This decision covers `src/AllenBradley.Logix/Client/`, namely `LogixClient`, `LogixReadBatch`,
and `LogixWriteBatch`. It was made under
[issue #5: Client base design](https://gitlab.com/vicione-oss/addons/allen-bradley/cip/-/work_items/5).

The DataPort read hands the client a **group** of data points and expects a list of typed values
back. The write hands it a list of typed values and returns a bare `ValueTask`. libplctag has no
call that reads several tags at once (see
[A testable interface over libplctag](2026-07-16-testable-libplctag-interface.md)), so a group
cannot be one wire call. It is *N* separate handle operations.

What makes *N* operations acceptable is packing. libplctag's C core (the native library under
the .NET wrapper) services all handles to one controller from a single queue, and bundles
whatever is waiting in that queue into a Multiple Service Packet (MSP), one CIP request that
carries many tag reads in a single network round-trip. Whether packing happens depends on how many
requests sit in the queue at the same moment. Start the whole group at once and the core can pack
it. Read the group in a blocking loop and the queue never holds more than one request, so nothing
ever packs (see
[the-shared-session.md](../../AllenBradley.Documentation/libPlcTag/the-shared-session.md)).

## Considered Options

- **Option 1: Resolve every converter and tag up front**, then start one operation per tag
  at once and await them all, handling failure per tag
- **Option 2: Hand-roll the Multiple Service Packet** ourselves over `libplctag.NativeImport`
- **Option 3: Mirror the S7 addon's batch**, where the whole group is one client-library call

## Decision Outcome

Chosen: **Option 1, resolve everything before any I/O**. Resolve every converter (see
[Decoding tag bytes into typed values](2026-07-16-decoding-tag-bytes-into-typed-values.md)) and
every tag (see
[Reusing and releasing tag handles](2026-07-16-reusing-and-releasing-tag-handles.md)), then start
one operation per tag at once and await them all. Starting them concurrently is precisely
what fills the core's queue and makes its packing engage. And libplctag offers no batch call we
could mirror instead.

```csharp
var reads = entries.Select(e => ReadEntryAsync(e, ct));
return await Task.WhenAll(reads);
```

Failure is handled **per tag**. The two directions surface it differently, because their
contracts differ:

- **Read** returns a value list (`IReadOnlyList`). A failed read marks its own data point as Bad
  quality (the DataPort framework's per-value flag for "this reading is unusable"), and the group
  still returns a full list. One bad tag never fails the whole group.
- **Write** returns a bare `ValueTask`, which has no per-tag channel. A dropped write therefore
  surfaces as a `LogixTagException` that **names every failed tag**. All writes are started
  together, and none stops its siblings. We collect the failures as results and combine them into
  one exception ourselves, because `Task.WhenAll` rethrows only the *first* exception of a set.
  A caller told about only the first failed tag would re-drive the wrong set.

Cancellation flows into every operation and still throws, because cancelling is the caller's
decision, not the device's answer.

### Consequences

The group starts one operation per tag with no upper bound, so a concurrency cap must be set before
this runs in production. That cap, and the in-flight load one shared connection can absorb, come from
the planned throughput measurement (see [Maximizing throughput with one shared
connection](2026-07-16-maximizing-throughput-with-one-shared-connection.md)). The same measurement
must confirm the core actually packs concurrent reads into one Multiple Service Packet, by counting
round-trips for *N* concurrent reads on one shared session, since the whole approach rests on it. If
the core does not pack them, this decision still stands, but the throughput story changes. The
fallback is to accept *N* round-trips or move to a newer libplctag batch facility, never a
hand-rolled packet.

### Enforcement

Unit tests against a fake `ILogixTagAccess` hold the contract in place: a group runs as *N*
concurrent operations; a failed read marks only its own data point Bad quality; failed writes
raise a single `LogixTagException` naming every failed tag.

## Pros and Cons of the Options

### Option 1: Resolve every converter and tag up front, then start all at once (chosen)

#### Pros

Per-tag failure isolation comes for free, because each tag operates independently. That is a clear
improvement over an all-or-nothing batch call, and it maps directly onto the value list the read
side returns. We also inherit the core's packing instead of owning any CIP encoding ourselves.

#### Cons

The fan-out is currently unbounded. A group of 200 data points starts 200 concurrent operations.
Bounding that fan-out is an obligation handed to the throughput work (see Consequences above).
Duplicate data points in one group also serialize. Two equal data points share one
synchronized tag, so their "concurrent" reads block each other into two round-trips instead of one.
That is a missed chance to de-duplicate before I/O, not a correctness problem.

### Option 2: Hand-roll the Multiple Service Packet (rejected)

Drop to `libplctag.NativeImport` (the raw bindings to the C core) and encode the CIP packet
ourselves.

#### Cons

It is only worth doing if we owned the whole wrapper layer, an option already rejected in [A testable
interface over libplctag](2026-07-16-testable-libplctag-interface.md). And it buys nothing, because
the core already packs.

### Option 3: Mirror the S7 addon's one-call batch (rejected)

In the sibling Siemens S7 addon, the client library (S7.Net) exposes
`ReadMultipleVarsAsync(List<DataItem>)`, where the whole batch is one call and the library updates
each `DataItem.Value` in place. The S7 batch classes are built around that call.

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
- S7 precedent: `Siemens.S7.Absolute/Client/DataItems/`: `S7ReadBatch.cs`, `S7WriteBatch.cs`
- [Issue #5: Client base design](https://gitlab.com/vicione-oss/addons/allen-bradley/cip/-/work_items/5)
