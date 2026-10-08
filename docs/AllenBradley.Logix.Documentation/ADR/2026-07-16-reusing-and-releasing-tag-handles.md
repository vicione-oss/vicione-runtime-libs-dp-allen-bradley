# Reusing and Releasing Tag Handles

## Context and Problem Statement

libplctag gives one handle for each PLC tag. There is no session object that we could keep. The first read of a handle
does the expensive setup: session registration, Forward Open and tag name resolution. Later reads on the same handle
skip this setup ([the shared session](../../AllenBradley.Documentation/libplctag/the-shared-session.md)).

Each handle must be disposed explicitly. If a finalizer frees a handle, this occurs after the CLR shutdown started. The
native library then stops the process with the fail-fast code `0xC0000602`
([tag disposal and shutdown](../../AllenBradley.Documentation/libplctag/tag-disposal-and-shutdown.md)).

The framework connects, disconnects and disposes a client. A later connect can undo a disconnect. A dispose is final.

Thus, the client needs a handle cache that:

1. Pays the setup cost one time for each tag, not one time for each poll.
2. Disposes each handle explicitly.
3. Has one owner that creates and disposes the handles, in step with connect, disconnect and dispose.

The code is in `src/AllenBradley.Logix/Client/Tags/Lifetime/`. This decision was made under
issue #5: Client base design.

## Considered Options

1. **Cache one handle for each data point,** with the data point record as the key. There is one cache for each
   controller, and it frees all handles when the connection ends.
2. **Let the finalizers free the handles.**
3. **Cache by an extracted key,** for example the tag address, not the full data point.

## Decision Outcome

Chosen option: **Option 1**. It keeps each handle for the life of the connection. It needs no second definition of
"the same tag". It disposes each handle explicitly.

- The tag manager is the cache. There is one tag manager for each controller.
- The key is the data point. Data points are C# records, so two equal data points get the same handle.
- The tag manager gives out one tag object for each data point. The tag object holds the data point, its declared type
  from the symbol table, and the access to the handle. The batches and the verification use it.
- The tag manager also owns the symbol table. Connect loads it. A request for a tag before the load throws an
  exception.
- One lock protects both create and dispose. Thus, no handle can be created after the cache was emptied, and no handle
  is left to a finalizer.
- Disconnect disposes all handles and drops the symbol table. The next connect loads the table again, and the handles
  are made again when they are needed. Dispose does the same and ends the tag manager.
- If the disposal of one handle throws, the tag manager logs the error and continues. All other handles are still
  freed.

### Consequences

- Good: The setup cost is paid one time for each tag and connection.
- Good: The fail-fast crash at shutdown cannot occur.
- Bad: The key contains fields that the handle does not need: the poll frequency and the channels. One tag configured
  at two poll frequencies gets two handles. We accept this cost.
- Bad: The first poll after a connect is slower, because it sets up each handle. A reconnect pays this cost again.
- Open: A release does not wait for the operations in flight, and the disposal is not locked. Thus, a release under
  load can free a handle during an operation. A stop-and-drain is necessary: stop the polls, let the operations
  complete, then free the handles.
- Open: There is no rule to make one handle again after a fatal error. The only recovery is a disconnect, which frees
  all handles.

## Why Not the Other Options

### Option 2: Let the finalizers free the handles

A finalizer runs after the CLR shutdown started, and the native library then stops the process with `0xC0000602`. The
tag lister of the connectivity spike did this and crashed a test run.

### Option 3: Cache by an extracted key

An extracted key is a second definition of tag identity. It must stay in sync with the fields of the record, and it
can become wrong without a warning. With the tag address as the key, two data points with different poll frequencies
would share one handle. Their polls would then wait for each other on the lock of that handle. A few extra handles cost
less.

## More Information

- Explanation: [Tags and handles](../explanation/client/tags-and-handles.md) ·
  [Connecting](../explanation/client/connecting.md)
- Background: [The shared session](../../AllenBradley.Documentation/libplctag/the-shared-session.md) ·
  [Tag disposal and shutdown](../../AllenBradley.Documentation/libplctag/tag-disposal-and-shutdown.md)
- Related decisions: [Operations, not accessors](2026-07-16-operations-not-accessors-over-libplctag.md) ·
  [Maximizing throughput with one shared connection](2026-07-16-maximizing-throughput-with-one-shared-connection.md) ·
  [Reading and writing a group of tags](2026-07-16-reading-and-writing-a-group-of-tags.md)
