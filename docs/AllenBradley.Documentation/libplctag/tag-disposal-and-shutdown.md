# Tag Disposal and Shutdown

Every `Tag` must be disposed deterministically. A `Tag` owns a native handle in the libplctag C core.
Disposing it frees that handle while the runtime is still alive. Leaking a `Tag` to the finalizer
instead runs the native free after CLR teardown has begun, and freeing a native handle against a
half-torn-down runtime fail-fasts the process with `0xC0000602` (`STATUS_FAIL_FAST_EXCEPTION`).

The failure is worse than a leak because of when it strikes. The finalizers run at process exit, so
the crash lands during shutdown, after the work has succeeded. A run that did everything right
reports a non-zero exit anyway, and the cause is nowhere near the code that looks at fault.

## Why this bites the test suite specifically

Under MTP the test assembly is the process, so its exit code is the suite's exit code. A leaked `Tag`
that fail-fasts from a finalizer turns a green run red. This is not hypothetical. The connectivity
spike's `PlcTagLister` did not dispose its `Tag` objects, and the process fail-fasted with
`0xC0000602` on a passing 3-of-3 run. The old VSTest host, which ran tests in a separate worker
process, hid the same bug. See [`AGENTS.md`](../../../AGENTS.md) for the suite-level framing.

## The consequence for the client

The reusable unit is the warm handle (see [the-shared-session.md](the-shared-session.md)), so handles
live as long as the tag cache. But that cache must own their disposal, not leave it to the GC. The
Logix ADR on [reusing and releasing tag handles](../../AllenBradley.Logix.Documentation/ADR/2026-07-16-reusing-and-releasing-tag-handles.md)
makes `ILogixTagAccess` `IDisposable` and has the lifecycle manager dispose every tag on release.
Acquiring warms the cache, and releasing frees every native handle deterministically. Any code that
creates a `Tag`, whether production, spike, or one-off probe, owns disposing it before the process
exits.

## Rule

> Dispose every `Tag`. If a `Tag` (or anything holding one) can outlive an explicit `Dispose`, the
> process can fail-fast on a run where every test passed.
