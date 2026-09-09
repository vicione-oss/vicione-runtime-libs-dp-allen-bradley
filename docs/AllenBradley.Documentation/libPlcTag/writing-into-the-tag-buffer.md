# Writing into the Tag Buffer

`Tag.SetBuffer(byte[])` is a **bounds-checked copy into the handle's buffer, and the handle's width
does not change to fit what you pass.** The wrapper forwards the array straight to
`plc_tag_set_raw_bytes` (`Tag.cs:976`) and turns the returned status into an exception. The
buffer it copies into is `tag->data`, whose size is the tag's size as the core settled it at creation,
from `elem_size * elem_count` or from the first read. The array you hand in is measured against that
size, not the other way round.

This is the write-side mirror of [what the tag buffer holds](what-the-tag-buffer-holds.md), and it
has the same shape: no decode step, no resize, one bounds check.

## A longer array is refused, and nothing moves

The core checks `start_offset + buffer_length > tag->size` before copying a byte. If it fails, the
call returns `PLCTAG_ERR_OUT_OF_BOUNDS` (`-27`), the buffer is untouched, and the wrapper throws
`LibPlcTagException` with `Status.ErrorOutOfBounds`. The throw is synchronous and comes out of
`SetBuffer` itself. `WriteAsync` is never reached and nothing goes to the controller.

Measured against a 30-byte tag, core 2.6.3:

| Call | Result |
|------|--------|
| `plc_tag_set_raw_bytes`, 30 bytes | `0` (`PLCTAG_STATUS_OK`) |
| `plc_tag_set_raw_bytes`, 31 bytes | `-27` (`PLCTAG_ERR_OUT_OF_BOUNDS`) |
| `plc_tag_set_raw_bytes`, 130 bytes | `-27` |
| buffer afterwards | all zero, unchanged |

There is no short-copy. It is all or nothing, exactly as `plc_tag_get_raw_bytes` is on the read side.

## A shorter array fills from the start and leaves the tail

The same check lets a shorter array through. The core copies `buffer_length` bytes at the offset and
leaves the rest of `tag->data` as it was: zero on a handle that has never been read, or whatever the
last read brought back. `plc_tag_write` then sends the whole `tag->size`, tail included.

So a short array is not an error, and it is not a partial write either. The controller receives a
full-width value whose tail you did not supply. For a Logix `STRING` that tail is the alignment
padding after `.DATA`, which is not a member and does not matter. For anything else it is stale
member bytes going out under your name.

## The typed `Tag<M,T>` path is worse

Our client does not use the typed wrapper, and this is one reason to keep it that way.
`Tag<M,T>.WriteAsync` calls `EncodeAll()` before it hands off to the native write
(`TagOfT.cs:195-202`), and `PlcMapperBase.EncodeArray` encodes element by element through
`plc_tag_set_int8`, `plc_tag_set_int32` and their siblings (`PlcMapperBase.cs:49-62`). Each of those
carries the same per-call bounds check:

| Call | Result |
|------|--------|
| `plc_tag_set_int8` at offset 29 | `0` |
| `plc_tag_set_int8` at offset 30 | `-27` |
| `plc_tag_set_int8` at offset 31 | `-27` |

The elements that fit are copied into the local buffer, and the first one past the end throws out
of `EncodeAll()`. The controller write still never happens, since the exception surfaces on the
returned task before the native write starts. The local buffer is now the head of your oversized
array, though. Catch the exception and write again, or have `AutoSyncWriteInterval` running, and that
partial data is what goes out. Anyone using the typed path has to guard the length themselves.

## Unverified: an array that fits the handle but not the controller's tag

If the handle was created with an explicit `elem_count` and `elem_size` that overstate the tag on
the controller, the local copy passes and the mismatch can only surface from the device, as a
protocol-level status on the write. That case needs the L32E and has not been pinned. The client
never sets those attributes, so its handles are sized by the first read and the case does not arise
for it.

## What this means for the client

`LogixTagAccess.WriteAsync` hands the converter's bytes to `SetBuffer` exactly as encoded, and the
client has no write-side call to `GetSize` at all. A payload longer than the handle is refused by the
core and lands in the adapter's `catch (LibPlcTagException)`, so it comes home as a failed outcome
naming the tag, the same way any device failure does. A shorter payload is a `STRING`'s `.LEN` plus
`.DATA` going into a handle two bytes wider, and the two bytes the core keeps are padding.

Both of those rely on the tag on the controller being the type the configuration says. That is what
[configuration verification](../../AllenBradley.Logix.Documentation/ADR/2026-07-21-verifying-configuration-against-the-symbol-table.md)
settles at connect, and the reason the write path does not check widths of its own. The interface
decision that removed the client's own write buffer is recorded in
[Operations, not accessors](../../AllenBradley.Logix.Documentation/ADR/2026-07-16-operations-not-accessors-over-libplctag.md).

## How it was checked

The numbers above come from a throwaway probe over `libplctag.NativeImport` against the native core
2.6.3 that ships in the `libplctag` 1.5.2 package this repository builds with. The probe is not
checked in. The wrapper line numbers are from the libplctag.NET source at that version.

## Source references

| Location | Role |
|----------|------|
| `Tag.cs:976` | `Tag.SetBuffer` forwards to `plc_tag_set_raw_bytes`; `ThrowIfStatusNotOk` turns the status into `LibPlcTagException` |
| `plc_tag_set_raw_bytes` (core) | The bounds check `start_offset + buffer_length > tag->size`, then a copy into `tag->data` with no type inspection |
| `TagOfT.cs:195-202` | `Tag<M,T>.WriteAsync` runs `EncodeAll()` before the native write |
| `PlcMapperBase.cs:49-62` | `EncodeArray` encodes element by element through the typed setters, each bounds-checked on its own |
