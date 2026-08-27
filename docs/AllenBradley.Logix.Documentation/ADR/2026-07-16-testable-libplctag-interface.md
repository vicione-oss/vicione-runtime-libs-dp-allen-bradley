# A Testable Interface over libplctag

## Context and Problem Statement

This decision covers `src/AllenBradley.Logix/Client/Tags/`, namely `ILogixTagAccess`, its
libplctag adapter `LogixTagAccess`, and `SynchronizedLogixTagAccess`. It was made under
[issue #5: Client base design](https://gitlab.com/vicione-oss/addons/allen-bradley/cip/-/work_items/5).

The client talks to the controller through libplctag, which is two layers: a C library (the
"core") that does the actual protocol work, and a thin .NET wrapper whose `Tag` class is what our
code calls. A `Tag` is a **handle**, one object per PLC tag, holding that tag's connection state.
libplctag has no call that reads several tags at once, so *N* tags means *N* handles. A handle
exposes raw byte buffers (`GetBuffer` / `SetBuffer`) and typed getters, plus async read and write
methods that honour a `CancellationToken` and connect on first use.

The wrapper's `Tag` class is `sealed` with an internal-only constructor. We can neither subclass
nor mock it from our own assembly. The maintainer's advice for exactly this situation is to
"abstract within your own application", putting your own interface over the library
([libplctag.NET#450](https://github.com/libplctag/libplctag.NET/issues/450)). The question is what
that interface looks like.

Two facts sharpen the question. Handles are **shared**. The cache hands the same connected handle
to every data point that names the same tag (see
[Reusing and releasing tag handles](2026-07-16-reusing-and-releasing-tag-handles.md)). So a read
and a write can run on one handle at the same time, and that is unsafe, in two distinct ways:

- The core rejects the second of two overlapping operations with `PLCTAG_ERR_BUSY`. The wrapper
  hides that error. It also matches completions off a stack without recording which operation a
  completion belongs to. The combination: the rejected call waits for a completion that never
  comes, times out, and calls `Abort()`. That abort disrupts operations started after it.
- `GetBuffer` and `SetBuffer` touch the handle's buffer directly, outside the read/write state
  machine. A write's `SetBuffer` can therefore land between a read finishing and the `GetBuffer`
  that picks up its bytes, corrupting what the read returns.

Both problems were reproduced against the real controller and are described in
[concurrent-operations-on-a-handle.md](../../AllenBradley.Documentation/libPlcTag/concurrent-operations-on-a-handle.md).

## Considered Options

- **Option 1: Our own small `ILogixTagAccess` interface** with **whole read / whole write**
  methods, guarded by `SynchronizedLogixTagAccess`
- **Option 2: A thin wrapper over `libplctag.NativeImport`**, the raw bindings to the C core
- **Option 3: Bind our logic directly onto `libplctag.Tag`**
- **Option 4: Lock the finer accessors**, meaning `ReadAsync` / `GetBuffer` / `SetBuffer` /
  `WriteAsync` separately, or keep a standalone `GetStatus`

## Decision Outcome

The chosen option is **Option 1**, our own small `ILogixTagAccess` interface, with a trivial
`LogixTagAccess` adapter that forwards to `Tag`. It gives us the mockable boundary the maintainer's
guidance asks for. Because each method is one complete read or one complete write, it closes both
handle problems in one place. The interface has no separate buffer or status methods:

```csharp
internal interface ILogixTagAccess : IDisposable
{
    Task<LogixTagReadResult> ReadAsync(CancellationToken ct);                  // status + raw bytes out
    Task<LogixTagWriteResult> WriteAsync(byte[] buffer, CancellationToken ct); // raw bytes in + status
}
```

`SynchronizedLogixTagAccess` guards each method with a semaphore. On a shared handle, a read and
a write therefore run as separate, non-overlapping units. That closes both problems at once. No
second operation is ever in flight (no timeout-and-abort cascade), and every buffer access happens
inside its own guarded unit (no raw-buffer race).

A device failure comes back as a **failed result** (`Succeeded == false`, with the reason on
`Error`), not as an exception. A tag that will not read is a normal event in a polled group, not
an exceptional one. Cancellation still throws, because cancelling is the caller's decision, not
the device's answer.

`Dispose` is **not** guarded. Disposal is the owner's job, and taking the lock inside `Dispose`
could deadlock if an operation never finishes.

Because `Tag` is sealed and loads native code, the place we can substitute a fake in tests is our
own interface. `ILogixTagAccess` sits *below* the batch and lifecycle logic and *above* the
wrapper, so everything above it can be tested in-process against a fake. This is our concrete form
of the maintainer's "abstract within your own application".

### Enforcement

Two standing checks hold this decision in place. `SharedAccessConcurrencyTests` exercises a shared
handle against the real controller and fails if overlapping operations become possible again. An
architecture test fails the build if anything outside `Client` references the libplctag assembly.
Code review holds the narrower rule that even inside `Client`, only the `LogixTagAccess` adapter
touches `Tag`.

## Pros and Cons of the Options

### Option 1: Our own small `ILogixTagAccess` interface (chosen)

#### Pros

The batch, caching and conversion code depends only on `ILogixTagAccess`, so all of it is tested
in-process against a fake instead of a real device. The tricky concurrency rules, meaning the
whole-read and whole-write gating, live in one reviewed class, `SynchronizedLogixTagAccess`, rather
than spread across every caller. The adapter itself is about a line per method. It turns the
library's `LibPlcTagException` into a failed result, so nothing above it sees the library's exception
type or its buffer protocol.

#### Cons

The adapter earns its keep only by making a sealed type mockable. That is real boilerplate, though it
stays confined to one small class. A fake is also only as truthful as our knowledge of the device.
One that gets the byte layout or the error behaviour wrong is worse than no test, so its accuracy is
bounded by the captured buffers and by the tests that run against the real controller.

### Option 2: A thin wrapper over `libplctag.NativeImport` (rejected)

`NativeImport` is the raw binding to the C core. Going through it means rebuilding ourselves what
the wrapper already provides: the handle state machine, the async completion callbacks, the
attribute string (the key=value configuration string a handle is created from), and connection
sharing.

#### Pros

It gives maximum control over every layer.

#### Cons

It re-implements the wrapper for control the C core already handles. Worth revisiting only if testing
shows the wrapper is inadequate *and* no wrapper-level setting fixes it.

### Option 3: Bind logic directly onto `libplctag.Tag` (rejected)

#### Cons

It ties batch and lifecycle logic to a sealed class that loads native code, so none of it can be
tested without a device. That is the exact trap the maintainer's guidance
([libplctag.NET#450](https://github.com/libplctag/libplctag.NET/issues/450)) warns about.

### Option 4: Lock the finer accessors, or keep a standalone `GetStatus` (rejected)

#### Cons

Locking `ReadAsync`, `GetBuffer`, `SetBuffer` and `WriteAsync` each on their own does not help,
because the unsafe units are larger than any one call. A read is "issue + wait + `GetBuffer`" and a
write is "`SetBuffer` + issue + wait", so locks around the individual calls still let those units
interleave. A standalone `GetStatus` has the same flaw. On a shared handle, a status read after an
operation is not reliably *that* operation's status, since another operation may have run in between,
and that is why the status is returned inside each result instead. (The device failure we first saw
turned out to be the wrapper's timeout-and-abort cascade, not the buffer corruption we first assumed,
which is why the fix is whole read/write methods rather than finer locks.)

## More Information

An open question at decision time, where the tag metadata for the runtime type-code check comes
from (see [Decoding tag bytes into typed values](2026-07-16-decoding-tag-bytes-into-typed-values.md)),
has since been settled by
[Verifying configuration against the controller symbol table](2026-07-21-verifying-configuration-against-the-symbol-table.md).
The source is the controller's symbol table.

- [concurrent-operations-on-a-handle.md](../../AllenBradley.Documentation/libPlcTag/concurrent-operations-on-a-handle.md)
  (the two problems the whole read/write methods close)
- Related: [Reading and writing a group of tags](2026-07-16-reading-and-writing-a-group-of-tags.md) ·
  [Reusing and releasing tag handles](2026-07-16-reusing-and-releasing-tag-handles.md) ·
  [Decoding tag bytes into typed values](2026-07-16-decoding-tag-bytes-into-typed-values.md)
- Upstream: [libplctag.NET#450](https://github.com/libplctag/libplctag.NET/issues/450)
  (why `Tag` cannot be mocked, and the maintainer's advice to wrap it) ·
  [libplctag.NET#406](https://github.com/libplctag/libplctag.NET/issues/406)
  (removal of the typed-mapper API)
- [Issue #5: Client base design](https://gitlab.com/vicione-oss/addons/allen-bradley/cip/-/work_items/5)
