# Concurrent Operations on One Handle

A single libplctag `Tag` handle is not safe to drive with two overlapping operations. Firing a read
and a write (or two reads) on the same handle before the first completes does not corrupt the tag
buffer, but it fails in a way that is worse than an honest error. The wrapper mispairs the
completions, one call starves until its timeout, and the recovery disrupts unrelated later
operations. This is verified against the real device (`SharedAccessConcurrencyTests`).

This matters because handles are shared. The tag cache hands the same warm handle to every data point
naming that tag (see [the-shared-session.md](the-shared-session.md)), so the read path and the write
path can genuinely be mid-operation on one handle at the same time.

## The two overlapping races

### 1. Operation mispairing above the native BUSY guard

The native core does guard against overlap. A second operation started while the first is running is
rejected with `PLCTAG_ERR_BUSY`. But the .NET wrapper swallows that BUSY and matches operation
completions off a LIFO stack with no operation identity. With two calls outstanding, completions pair
to the wrong waiters. The orphaned call never receives its completion, starves until `Tag.Timeout`,
throws `ErrorTimeout`, and calls `Abort()`. That abort disrupts subsequent operations on the handle,
so one collision cascades into failures that outlive it. The observed failure on the device is
exactly this timeout cascade, not the buffer corruption first assumed.

### 2. A raw-buffer race below the guard

`GetBuffer()` / `SetBuffer()` are synchronous raw accessors that sit outside the operation state
machine, so the BUSY guard does not cover them at all. A write's `SetBuffer` can land in the window
between a read's operation completing and that read calling `GetBuffer`, so the read decodes bytes
the write just planted. This race is narrower than the first, but the lock that closes one must
close both.

## The consequence for the client

Neither race is defensible by locking individual accessors. A lock over `ReadAsync`, `GetBuffer`,
`SetBuffer` and `WriteAsync` as separate operations cannot make "read = issue + wait + GetBuffer" and
"write = SetBuffer + issue + wait" mutually exclusive as units.

The Logix ADR on [Operations, not accessors](../../AllenBradley.Logix.Documentation/ADR/2026-07-16-operations-not-accessors-over-libplctag.md)
responds by making each `ILogixTagAccess` member one whole operation. `ReadAsync` returns status plus
raw bytes, `WriteAsync` takes raw bytes plus status, and there are no separate buffer accessors on
the interface, so `SynchronizedLogixTagAccess` can gate the entire sequence as a unit and close both
races at once. Folding the exchange together also retires a standalone `GetStatus`. A status queried
after the fact on a shared handle is not reliably this operation's status, so status rides home on
the result instead.

Note this is a constraint on overlapping operations on the same handle, not on concurrency in
general. Concurrent operations across different handles are not only safe but desirable, and that is
what feeds the packer (see [the-shared-session.md](the-shared-session.md)).
