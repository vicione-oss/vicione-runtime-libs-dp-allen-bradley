# A Testable Interface over libplctag

## Context and Problem Statement

This decision covers the client's tag-access folder, `src/AllenBradley.Logix/Client/Tags/Access/`. That
folder holds our own access interface, the decorator that serializes calls to it, and the libplctag
adapter beneath both. It was made under
[issue #5: Client base design](https://gitlab.com/vicione-oss/addons/allen-bradley/cip/-/work_items/5).

The client talks to the controller through libplctag, which is two layers. A C library (the "core") does
the protocol work, and a thin .NET wrapper exposes the `Tag` class our code calls. A `Tag` is a
**handle**, one object per PLC tag, holding that tag's connection state. libplctag has no call that reads
several tags at once, so *N* tags means *N* handles. A handle exposes raw byte buffers (`GetBuffer` and
`SetBuffer`), its own width (`GetSize`) and typed getters, plus async read and write methods that honour a
`CancellationToken` and connect on first use.

The wrapper's `Tag` class is `sealed` with an internal-only constructor. We can neither subclass it nor
mock it from our own assembly. The maintainer's advice for exactly this situation is to "abstract within
your own application", putting your own interface over the library
([libplctag.NET#450](https://github.com/libplctag/libplctag.NET/issues/450)). The question is what that
interface looks like.

Two facts sharpen the question. Handles are **shared**, because the cache hands the same connected handle
to every data point that names the same tag (see
[Reusing and releasing tag handles](2026-07-16-reusing-and-releasing-tag-handles.md)). A read and a write
can therefore run on one handle at the same time, and that is unsafe in two distinct ways.

The core rejects the second of two overlapping operations with `PLCTAG_ERR_BUSY`, and the wrapper hides
that error. It also matches completions off a stack without recording which operation a completion belongs
to. Put the two together and the rejected call waits for a completion that never comes, times out, and
calls `Abort()`. That abort disrupts operations started after it.

The other way is the raw buffer. `GetBuffer` and `SetBuffer` touch the handle's buffer directly, outside
the read/write state machine. A write's `SetBuffer` can land between a read finishing and the `GetBuffer`
that picks up its bytes, which corrupts what the read returns.

Both problems were reproduced against the real controller and are described in
[concurrent-operations-on-a-handle.md](../../AllenBradley.Documentation/libPlcTag/concurrent-operations-on-a-handle.md).

## Considered Options

- **Option 1: Our own small access interface** with whole read and whole write methods, guarded by a
  decorator that serializes them
- **Option 2: A thin wrapper over `libplctag.NativeImport`**, the raw bindings to the C core
- **Option 3: Bind our logic directly onto the wrapper's `Tag`**
- **Option 4: Lock the finer accessors**, meaning read, `GetBuffer`, `SetBuffer` and write separately, or
  keep a standalone status call

## Decision Outcome

The chosen option is **Option 1**, our own small access interface, with a trivial adapter that forwards to
`Tag`. It gives us the mockable boundary the maintainer's guidance asks for. Because every member is one
complete exchange, it closes both handle problems in one place. A read carries its status and its raw
bytes home. A write takes its bytes in and answers with a status. There is no status member of its own,
and no way to reach the handle's buffer.

One member is not an exchange. It hands back a new, empty array of the width libplctag opened the handle
at, and it is deliberately not one of the finer accessors Option 4 would have locked. It reads no tag
state, returns nothing the handle owns, and touches nothing the read/write state machine does, so there is
no race for a gate to close. It exists because the controller owns a tag's width rather than the
configuration. A `STRING` and a `STRING_20` are one converter and two widths (see [Decoding tag bytes into
typed values](2026-07-16-decoding-tag-bytes-into-typed-values.md)), so a write takes its buffer from the
tag and the converter only fills it.

The decorator guards each exchange with a semaphore. On a shared handle, a read and a write then run as
separate, non-overlapping units. That closes both problems at once. No second operation is ever in flight,
so the timeout-and-abort cascade cannot start, and every buffer access happens inside its own guarded
unit.

A device failure comes back as a **failed result** carrying its reason, not as an exception. A tag that
will not read is a normal event in a polled group rather than an exceptional one. Cancellation still
throws, because cancelling is the caller's decision and not the device's answer.

Disposal is **not** guarded. Freeing the handle is the owner's job, and taking the lock there could
deadlock if an operation never finishes.

Because `Tag` is sealed and loads native code, the place we can substitute a fake in tests is our own
interface. It sits below the batch and lifecycle logic and above the wrapper, so everything above it can
be tested in-process against a fake. That is our concrete form of the maintainer's advice.

### Enforcement

Two test suites hold the gating in place. An in-process suite pins the behaviour: two callers on one
access never overlap, and the buffer-handing member still answers while a read is in flight. A device-tier
suite exercises a shared handle against the real controller and fails if overlapping operations become
possible again.

Confining the library itself is a **code-review rule, not a build failure.** The `libplctag` package
reference sits on the whole project, and today only the adapter folder uses it. An architecture test that
fails the build on a reference from anywhere else would make that a guarantee rather than a habit. It is
worth adding and does not exist yet.

## Pros and Cons of the Options

### Option 1: Our own small access interface (chosen)

#### Pros

The batch, caching and conversion code depends only on our interface, so all of it is tested in-process
against a fake instead of a real device. The concurrency rules, meaning the whole-read and whole-write
gating, live in one reviewed class rather than spread across every caller. The adapter itself is about a
line per method. It turns the library's exception type into a failed result, so nothing above it sees that
type or the library's buffer protocol.

#### Cons

The adapter earns its keep only by making a sealed type mockable. That is real boilerplate, though it
stays confined to one small class. A fake is also only as truthful as our knowledge of the device. One
that gets the byte layout or the error behaviour wrong is worse than no test, so its accuracy is bounded
by the layouts we pin in byte arrays and by the tests that run against the real controller.

### Option 2: A thin wrapper over `libplctag.NativeImport` (rejected)

`NativeImport` is the raw binding to the C core. Going through it means rebuilding ourselves what the
wrapper already provides. That is the handle state machine, the async completion callbacks, the attribute
string (the key=value configuration string a handle is created from), and connection sharing.

#### Pros

It gives maximum control over every layer.

#### Cons

It re-implements the wrapper for control the C core already handles. Worth revisiting only if testing
shows the wrapper is inadequate *and* no wrapper-level setting fixes it.

### Option 3: Bind logic directly onto the wrapper's `Tag` (rejected)

#### Cons

It ties batch and lifecycle logic to a sealed class that loads native code, so none of it can be tested
without a device. That is the exact trap the maintainer's guidance
([libplctag.NET#450](https://github.com/libplctag/libplctag.NET/issues/450)) warns about.

### Option 4: Lock the finer accessors, or keep a standalone status call (rejected)

#### Cons

Locking read, `GetBuffer`, `SetBuffer` and write each on their own does not help, because the unsafe units
are larger than any one call. A read is issue, then wait, then `GetBuffer`. A write is `SetBuffer`, then
issue, then wait. Locks around the individual calls still let those units interleave. A standalone status
call has the same flaw. On a shared handle, a status read after an operation is not reliably *that*
operation's status, since another operation may have run in between, which is why the status is returned
inside each result instead. The device failure we first saw turned out to be the wrapper's
timeout-and-abort cascade rather than the buffer corruption we assumed, and that is why the fix is whole
read and write methods instead of finer locks.

## More Information

The tag metadata that the type check in [Decoding tag bytes into typed
values](2026-07-16-decoding-tag-bytes-into-typed-values.md) compares against comes over this same seam. A
symbol-table browse is a read of the `@tags` pseudo-tag through a transient access, so nothing above the
adapter touches the sealed wrapper type to get it. See [Verifying configuration against the controller
symbol table](2026-07-21-verifying-configuration-against-the-symbol-table.md).

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
