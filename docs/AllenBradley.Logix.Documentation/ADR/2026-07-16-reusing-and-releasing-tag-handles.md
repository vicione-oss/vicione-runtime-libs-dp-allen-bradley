# Reusing and Releasing Tag Handles

## Context and Problem Statement

This decision covers the client's tag-lifetime folder,
`src/AllenBradley.Logix/Client/Tags/Lifetime/`, which holds the handle cache, the tag it hands out and the
interface over it, together with the factory that builds the handles next door under the libplctag
adapter. It was made under
[issue #5: Client base design](https://gitlab.com/vicione-oss/addons/allen-bradley/cip/-/work_items/5).

libplctag gives us one **handle** per PLC tag, the wrapper's `Tag` object, holding that tag's connection
state (see [Operations, not accessors](2026-07-16-operations-not-accessors-over-libplctag.md) for the
interface we put over it). There is no separate session object we could cache. The expensive, reusable
thing is the connected handle itself. The first read on a handle does three slow steps. It registers a
session with the controller, performs a Forward Open (the CIP handshake that opens a connection and claims
one of the controller's small pool of connection slots), and resolves the tag name. Every later read on
that handle skips all three (see
[the-shared-session.md](../../AllenBradley.Documentation/libPlcTag/the-shared-session.md)).

Handles must also be **disposed explicitly**. A handle left to its finalizer is freed after CLR shutdown
has already begun, and the native library then kills the whole process with the fail-fast code
`0xC0000602` (see
[tag-disposal-and-shutdown.md](../../AllenBradley.Documentation/libPlcTag/tag-disposal-and-shutdown.md)).
The connectivity spike hit exactly this crash.

The DataPort lifecycle acquires a client per device and releases it, and a released client is disconnected
before it is disposed. So the handle cache needs a clear owner, a point at which everything it holds is
freed, and a way back into service after that point, because a disconnect is reversible where a dispose is
not.

## Decision Drivers

- Pay the handle's setup cost once per tag, not once per poll.
- Dispose every handle explicitly, so the `0xC0000602` crash cannot happen.
- One clear owner for creating and disposing, matching the acquire/release lifecycle.

## Considered Options

- **Option 1: Cache one tag per data point**, keyed by the data-point record, with the manager
  scoped per controller and freeing every tag when the connection ends
- **Option 2: Leave handles to their finalizers**
- **Option 3: Cache by an extracted key**, rather than the data point itself

## Decision Outcome

Chosen: **Option 1, cache one tag per data point**, keyed by the record, scoped per controller, all freed
when the connection ends. It reuses the connected handle without inventing a second definition of tag
identity, and it keeps disposal explicit.

Data points are C# records, so the data point itself is the cache key. Two data points that name the same
tag with the same shape compare equal, and therefore share one tag and one connected handle. The key is
exactly the record's fields. That is the tag name, the poll frequency, the channels the value is routed
to, and whatever configuration the type carries, such as a `STRING`'s declared capacity. There is no
second, hand-written definition of "same tag" to keep in sync with the record.

The manager is scoped to **one controller**. Its scope is the factory's connection information, meaning
gateway, CIP routing path, controller family and per-operation timeout, of which the first three are the
identity the C core uses to decide which handles share a session. That makes the manager the per-device
handle cache.

What the manager hands out is a tag, not the bare handle. The tag joins the configured data point, the
controller's metadata for that tag, and the read/write access into one object. It has the shape of the
sibling S7 addon's symbolic data-point access, and it is what the batches and configuration verification
both project off. The minimal access interface survives underneath as the operation and mock seam (see
[Operations, not accessors](2026-07-16-operations-not-accessors-over-libplctag.md)). The tag composes it,
adds the two immutable getters, and disposes it. The concurrency contract is untouched, because metadata and
the data point are data rather than exchange state and need no gating.

The manager also owns the controller's symbol table, which is what it has to join that metadata from. This
mirrors the S7 addon's access manager owning its root-node handle. The browse runs once through an
injected loader and the definitions are retained whole. It is idempotent, which is the same guard S7 puts
on its root node. Asking for a tag joins the lookup by name onto the handle at creation, and it **throws**
if the definitions have not been loaded. The browse is a hard connect precondition rather than a lazy
first-access side effect.

Creation and freeing run under one lock. A tag can therefore never be created and then leak to a
finalizer, neither by two callers racing on the same key nor by a caller arriving after the cache has
already been emptied. A disposal that throws is **logged, not re-raised**, so one failing handle cannot
block the freeing of the handles behind it.

The manager ends two ways, because everything it holds is connect-scoped. The disconnect step disposes
every tag, drops the definitions, and leaves the manager ready to browse and rebuild. Disposal is terminal
and does the same freeing on the way out. Both are idempotent, and both free **every** handle, which is
the guarantee this decision exists for. A client release runs one and then the other.

### Consequences

Two obligations follow from this decision, and both are still open now that the lifecycle around them
exists. The client pool disconnects and then disposes a client when its last holder releases, and neither
step waits for work in flight. Because disposal is deliberately unguarded, a release under load can free a
handle mid-operation. Closing that needs a stop-and-drain, meaning stop polling, let in-flight operations
finish, and only then free. Nothing performs one today. The second obligation is a recreate policy. A
cached handle can hit a terminal error mid-life, and nothing says when or how that one tag is rebuilt. The
only recovery available is a disconnect, which drops the whole cache.

### Enforcement

The manager's unit suite pins the behaviour in place. Equal data points share one tag. The disconnect step
and disposal each free every tag. A drained manager serves again once the definitions are reloaded. Asking
for a tag before they are loaded throws. A throwing disposal does not stop the freeing of the rest. Code
review holds the rule that tags are created and freed only through the manager.

## Pros and Cons of the Options

### Option 1: Cache one tag per data point (chosen)

#### Pros

Reusing the connected handle across polls is the whole point of the cache. The handle's setup cost, which
is session registration, the Forward Open, and name resolution, is paid once per tag rather than on every
read. Keying the cache on the data-point record itself means there is no second definition of "same tag"
that could drift out of sync with the record's fields. And because every tag is created and disposed under
the manager's one lock, none can leak to a finalizer and trigger the `0xC0000602` crash.

#### Cons

The cost falls on release. Disposal is deliberately left unguarded against in-flight operations (see
[Operations, not accessors](2026-07-16-operations-not-accessors-over-libplctag.md)), so a release under
load can free a handle mid-operation. Closing that race is an obligation this decision hands to the lifecycle
work (see Consequences above).

Keying on the whole record has a smaller cost, and that one is no longer hypothetical. A data point
carries its poll frequency and the channels it feeds, and a handle ignores both. One controller tag
configured at two frequencies is therefore two data points and gets two handles, which means two
connection slots and two name resolutions for one tag. That is what not maintaining a second definition of
tag identity costs, and it is paid until an explicit key is worth extracting.

### Option 2: Leave handles to their finalizers (rejected)

#### Cons

A finalizer runs after CLR shutdown has already begun, so the native teardown kills the process with the
fail-fast code `0xC0000602`. The connectivity spike's tag lister did exactly this and crashed an otherwise
green test run.

### Option 3: Cache by an extracted key rather than the data point (rejected)

#### Cons

An extracted key is a second definition of tag identity that has to be kept in sync with the record's
fields. Data points do carry fields a handle ignores, poll frequency and channels, so the duplicate
handles Option 1's key produces are real rather than theoretical. What keeps this rejected is that a
handful of extra handles costs less than a hand-written identity that can silently drift from the record.

## More Information

- [the-shared-session.md](../../AllenBradley.Documentation/libPlcTag/the-shared-session.md)
  (the connected handle as the reusable unit, and the sharing identity the cache scope mirrors)
- [tag-disposal-and-shutdown.md](../../AllenBradley.Documentation/libPlcTag/tag-disposal-and-shutdown.md)
  (why disposal must be explicit)
- Related: [Operations, not accessors](2026-07-16-operations-not-accessors-over-libplctag.md) ·
  [Maximizing throughput with one shared connection](2026-07-16-maximizing-throughput-with-one-shared-connection.md) ·
  [Reading and writing a group of tags](2026-07-16-reading-and-writing-a-group-of-tags.md)
- [Issue #5: Client base design](https://gitlab.com/vicione-oss/addons/allen-bradley/cip/-/work_items/5)
