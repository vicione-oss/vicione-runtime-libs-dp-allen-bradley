# Client architecture — how the classes collaborate

The Logix client is the layer between the two dataports and the native `libplctag` handle. It is one
stack, entered from two seams, that narrows a group of configured data points down to individual
reads and writes on the wire and back to typed values.

![How the Logix client classes collaborate](../../diagrams/client-architecture-collaboration.svg)

The source scene is
[`diagrams/client-architecture-collaboration.excalidraw`](../../diagrams/client-architecture-collaboration.excalidraw);
re-export the SVG and run the font-fix after editing it.

## The read/write path

- **`ILogixReadClient` / `ILogixWriteClient`** are the two seams the dataports depend on — the
  incoming port reads, the outgoing port writes — kept split so each direction depends only on what it
  uses (ADR-002).
- **`LogixClient`** implements both. It is the *one shared instance per device*: giving both
  directions the same client is what makes them share the connection and the access cache underneath.
  It delegates each call to a batch.
- **`LogixReadBatch` / `LogixWriteBatch`** resolve every data point up front — a converter and an
  access apiece — then fan the individual operations out. A read decodes each buffer; a write encodes
  into a right-sized one first. A single tag failing degrades only its own value (read) or is named in
  the thrown exception (write); it never sinks the batch.
- **`DataPointConverterRegistry` → `IDataPointConverter`** map each data-point type to its
  encode/decode/type-check logic (`DIntConverter`, `RealConverter`). The registry is the run-time
  type-safety gate: a converter whose expected CIP type conflicts with the controller's metadata
  produces a Bad value or fails the write rather than misreading bytes (ADR-003).

## Access, cache, and the native handle

- **`CachingLogixTagManager`** (`ILogixTagManager`) owns the controller
  schema and hands out one `LogixTag` per data point, cached for its lifetime. It is the
  per-device tag cache and schema owner; disposing it frees every handle.
- **`LogixTag`** joins the three immutable facts about a point — the configured
  `DataPoint`, the controller's `Metadata`, and the read/write access — into the single object every
  consumer projects off.
- **`SynchronizedLogixTagAccess` → `LogixTagAccess` → `libplctag Tag`** is the access chain. The
  synchronized wrapper gates one whole operation at a time on a shared handle (ADR-001); the inner
  adapter runs the read/write against the native `Tag` and maps its exceptions onto results.
- **`LogixTagAccessFactory`** (`ILogixTagAccessFactory`) builds those accesses — for data-point
  tags on behalf of the manager, and for `@tags` system tags on behalf of the browser.

## Schema and verification

- **`LogixSchemaBrowser`** (`ILogixSchemaBrowser`) reads the controller's symbol table once at connect
  through the same exchange seam and **builds** a `LogixControllerSchema` — the tag-name → type
  declaration map the manager joins onto every access.
- **`LogixConfigurationVerifier`** is a *projection* of the manager's accesses, not a second browser:
  it loads the schema, then reports the tags whose declared type, shape, or existence disagrees with
  the configuration.
