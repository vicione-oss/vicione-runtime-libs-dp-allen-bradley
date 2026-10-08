# Operations, Not Accessors: Our Interface over libplctag

## Context and Problem Statement

The client talks to the controller through libplctag. The .NET wrapper gives one `Tag` object for each PLC tag, and we
call this object a handle. A handle holds the connection state and the byte buffer of its tag. It has `ReadAsync` and
`WriteAsync` for the device, and `GetBuffer` and `SetBuffer` for the buffer. The code is in
`src/AllenBradley.Logix/Client/Tags/Access/`. This decision was made under
issue #5: Client base design.

We need our own interface over the handle, for two reasons:

1. **Tests.** `Tag` is `sealed`, has an internal constructor and loads native code. We cannot mock it. The maintainer
   of libplctag.NET recommends an interface in our own code
   ([libplctag.NET#450](https://github.com/libplctag/libplctag.NET/issues/450)).
2. **Safety on a shared handle.** The tag manager gives one handle to all equal data points
   ([Reusing and releasing tag handles](2026-07-16-reusing-and-releasing-tag-handles.md)). Thus, two operations can run
   on one handle at the same time. This fails in two ways
   ([concurrent operations on a handle](../../AllenBradley.Documentation/libplctag/concurrent-operations-on-a-handle.md)):
   - libplctag refuses the second operation with `PLCTAG_ERR_BUSY`, and the wrapper hides this error. The refused call
     waits until its timeout and then calls `Abort()`. The abort breaks the operations that started after it.
   - `GetBuffer` and `SetBuffer` touch the buffer outside the read and the write. A write can call `SetBuffer` after a
     read completes, but before the read calls `GetBuffer`. The read then returns the bytes of the write.

The question is the granularity of the interface. Is one member one complete operation, or one accessor of the
library?

## Considered Options

1. **Our own small interface with one member for each complete operation.** A decorator lets only one operation at a
   time run on a handle.
2. **A thin wrapper over `libplctag.NativeImport`,** the raw bindings to the C library.
3. **Use the `Tag` of the wrapper directly.**
4. **Lock each accessor of the library separately,** and keep a separate status call.

## Decision Outcome

Chosen option: **Option 1**. One member for each complete operation is the unit that we can mock. It is also the unit
that cannot interleave with a different operation. Thus, one design solves both problems.

- The interface has two members. `ReadAsync` returns the status and the raw bytes. `WriteAsync` takes the bytes and
  returns the status. There is no member for the buffer and no member for the status.
- A decorator puts a semaphore on each handle. Only one operation at a time runs on a handle. Different handles do not
  wait for each other.
- An adapter forwards each member to the libplctag `Tag`. Only the adapter and its factory use libplctag types.
- A device failure is a failed result, not an exception. A tag that does not answer is a normal event in a poll.
  Cancellation still throws, because the caller decides it.
- `Dispose` does not wait for the semaphore. If an operation never completes, a wait there causes a deadlock.

### Consequences

- Good: The batches, the tag cache and the converters use only our interface. Their tests use a fake, not a device.
- Good: The rule "one operation at a time" is in one class, not in each caller.
- Bad: The adapter is boilerplate. Its only purpose is to make a sealed type replaceable.
- Bad: A fake is only as correct as our knowledge of the device. A fake with a wrong byte layout gives tests that pass
  for the wrong reason.
- Bad: Two operations on one handle wait for each other. Two equal data points in one group need two round trips, not
  one.

## Why Not the Other Options

### Option 2: A thin wrapper over `libplctag.NativeImport`

We would have to build again what the wrapper already gives: the state machine of the handle, the async completion,
the attribute string and the session sharing. This is necessary only if the wrapper fails, and no wrapper setting
corrects it.

### Option 3: Use `Tag` directly

The batch and lifecycle logic would depend on a sealed class that loads native code. No part of it could be tested
without a device.

### Option 4: Lock each accessor separately

The unsafe unit is larger than one call. A read is: send, wait, `GetBuffer`. A write is: `SetBuffer`, send, wait. A
lock on each call gives isolation, but these units can still interleave. They also need atomicity, and only one member
for the complete operation gives it.

A separate status call has the same problem. Another operation can run between the operation and the status call.
Thus, each result carries its own status.

## More Information

- Explanation: [Tags and handles](../explanation/client/tags-and-handles.md)
- Background: [Concurrent operations on a handle](../../AllenBradley.Documentation/libplctag/concurrent-operations-on-a-handle.md),
  with the two failures found on the real controller
- Related decisions: [Reusing and releasing tag handles](2026-07-16-reusing-and-releasing-tag-handles.md) ·
  [Reading and writing a group of tags](2026-07-16-reading-and-writing-a-group-of-tags.md)
- Upstream: [libplctag.NET#450](https://github.com/libplctag/libplctag.NET/issues/450), why `Tag` cannot be mocked
