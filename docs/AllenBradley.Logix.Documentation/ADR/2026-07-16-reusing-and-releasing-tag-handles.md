# Reusing and Releasing Tag Handles

## Context and Problem Statement

This decision covers `src/AllenBradley.Logix/Client/Tags/`, which holds `CachingLogixTagManager`,
`LogixTagAccessFactory`, and the `ILogixTagManager` interface. It was made under
[issue #5: Client base design](https://gitlab.com/vicione-oss/addons/allen-bradley/cip/-/work_items/5).

libplctag gives us one **handle** per PLC tag, the wrapper's `Tag` object, holding that tag's
connection state (see
[A testable interface over libplctag](2026-07-16-testable-libplctag-interface.md) for the
interface we put over it). There is no separate session object we could cache. The expensive,
reusable thing is the connected handle itself. The first read on a handle does three slow steps.
It registers a session with the controller, performs a Forward Open (the CIP handshake that opens
a connection and claims one of the controller's small pool of connection slots), and resolves the
tag name. Every later read on that handle skips all three (see
[the-shared-session.md](../../AllenBradley.Documentation/libPlcTag/the-shared-session.md)).

Handles must also be **disposed explicitly**. A handle left to its finalizer is freed after CLR
shutdown has already begun, and the native library then kills the whole process with the fail-fast
code `0xC0000602` (see
[tag-disposal-and-shutdown.md](../../AllenBradley.Documentation/libPlcTag/tag-disposal-and-shutdown.md)).
The connectivity spike hit exactly this crash.

The DataPort lifecycle acquires a client per device (`AcquireConnectedAsync`) and releases it
(`ReleaseAsync`). So the handle cache needs a clear owner, and a clear point at which everything
it holds is disposed.

## Decision Drivers

- Pay the handle's setup cost once per tag, not once per poll.
- Dispose every handle explicitly, so the `0xC0000602` crash cannot happen.
- One clear owner for creating and disposing, matching the acquire/release lifecycle.

## Considered Options

- **Option 1: Cache one tag per data point**, keyed by the data-point record, with the manager
  scoped per controller and disposing every tag on release
- **Option 2: Leave handles to their finalizers**
- **Option 3: Cache by an extracted key**, rather than the data point itself

## Decision Outcome

Chosen: **Option 1, cache one tag per data point**, keyed by the record, scoped per
controller, all disposed on release. It reuses the connected handle without inventing a second
definition of tag identity, and it keeps disposal explicit.

Data points are C# records, so the **data point itself is the cache key**. Two data points that
name the same tag with the same shape compare equal, and therefore share one tag, and with it
one connected handle. The key is exactly the record's fields. Today that is the tag name, with
element count and string options once those fields exist. There is no second, hand-written
definition of "same tag" to keep in sync with the record.

The manager is scoped to **one controller**. Its scope is the factory's connection information
(gateway, CIP routing path, and PLC type), which is the same identity the C core uses to decide
which handles share a session. That makes `CachingLogixTagManager` the per-device handle
cache.

Creation and disposal run under one lock. A tag can therefore never be created and then leak
to a finalizer, neither by two callers racing on the same key nor by a caller arriving after
`Dispose` has already emptied the cache. `ReleaseAsync` disposes **every** tag. A `Dispose`
that throws is **logged, not re-raised**, so one failing handle cannot block the disposal of the
handles behind it.

### Amendment: a metadata-bearing tag, and the manager owns the schema

`TagFor` now returns **`ILogixTag`**, not the bare `ILogixTagAccess`. It joins the configured
data point, the controller's metadata for its tag, and the read/write handle into **one object**,
the shape S7's `ISymbolicDataPointAccess` has. The minimal `ILogixTagAccess` survives underneath
as the access and mock seam (ADR-001). `LogixTag` composes it, adds the two immutable getters,
and disposes it. The concurrency contract is untouched. Metadata and the data point are data,
not exchange state, so they need no gating.

To join that metadata on, the manager now also **owns the controller's symbol table**, mirroring
S7's `SymbolicAccessManager` owning `RootNodeHandle`. `LoadSchemaAsync` browses once at connect
through an injected `ILogixSchemaBrowser` and retains the schema whole (idempotent, the S7
root-node guard). `TagFor` joins `_schema.Lookup(name)` onto the handle at creation, and
**throws** if it is called before the schema loads. The browse is a hard connect precondition,
not a lazy first-access side effect. Disposal drops the schema alongside the handles.

None of this touches the handle lifecycle this ADR is about. The cache key is still the data-point
record, reuse is unchanged, and creation, disposal and the throwing-dispose drain still run under
the one lock. The object handed out is richer. The `0xC0000602` guarantee is identical. See
[A unified data point access](../unified-data-point-access-plan.md) for the reshape in full.

### Consequences

Two obligations follow from this decision, and both fall to the client-lifecycle work that builds
acquire and release. Because `Dispose` is left unguarded, `ReleaseAsync` needs a stop-and-drain step.
It must stop polling, let in-flight operations finish, and only then release. Without that step, a
release under load can dispose a handle mid-operation. The lifecycle also needs a recreate policy,
since a cached handle can hit a terminal error mid-life and the design does not yet say when or how
such a tag is rebuilt. Neither is settled here.

### Enforcement

`CachingLogixTagManagerTests` pins the behaviour in place: equal data points share one
tag, release disposes every tag, and a throwing `Dispose` does not stop the disposal of the
rest. Code review holds the rule that tags are created and disposed only through the manager.

## Pros and Cons of the Options

### Option 1: Cache one tag per data point (chosen)

#### Pros

Reusing the connected handle across polls is the whole point of the cache. The handle's setup cost,
which is session registration, the Forward Open, and name resolution, is paid once per tag rather
than on every read. Keying the cache on the data-point record itself means there is no second
definition of "same tag" that could drift out of sync with the record's fields. And because every
tag is created and disposed under the manager's one lock, none can leak to a finalizer and trigger
the `0xC0000602` crash.

#### Cons

The cost falls on release. `Dispose` is deliberately left unguarded against in-flight operations
(see [A testable interface over libplctag](2026-07-16-testable-libplctag-interface.md)), so a
`ReleaseAsync` under load can dispose a handle mid-operation. Closing that race is an obligation this
decision hands to the lifecycle work (see Consequences above). Keying on the whole record has a
smaller cost too. A data point that later grows a field the tag ignores would split the cache into
two entries for one real tag, until an explicit key is extracted.

### Option 2: Leave handles to their finalizers (rejected)

#### Cons

A finalizer runs after CLR shutdown has already begun, so the native teardown kills the process with
the fail-fast code `0xC0000602`. The connectivity spike's `PlcTagLister` did exactly this and crashed
an otherwise green test run.

### Option 3: Cache by an extracted key rather than the data point (rejected)

#### Cons

An extracted key is a second definition of tag identity that has to be kept in sync with the
record's fields, and it buys nothing until a data point actually carries a field the tag should
ignore.

## More Information

- [the-shared-session.md](../../AllenBradley.Documentation/libPlcTag/the-shared-session.md)
  (the connected handle as the reusable unit; the sharing identity the cache scope mirrors)
- [tag-disposal-and-shutdown.md](../../AllenBradley.Documentation/libPlcTag/tag-disposal-and-shutdown.md)
  (why disposal must be explicit)
- Related: [A testable interface over libplctag](2026-07-16-testable-libplctag-interface.md) ·
  [Maximizing throughput with one shared connection](2026-07-16-maximizing-throughput-with-one-shared-connection.md) ·
  [Reading and writing a group of tags](2026-07-16-reading-and-writing-a-group-of-tags.md)
- [Issue #5: Client base design](https://gitlab.com/vicione-oss/addons/allen-bradley/cip/-/work_items/5)
