# Writing into the Tag Buffer

`Tag.SetBuffer(byte[])` resizes the handle to the length of the array and then copies the array into
it. The wrapper calls `plc_tag_set_size` with the length of the array first, and `plc_tag_set_raw_bytes`
after it (`NativeTagWrapper.cs:563-570`). A failed status from either call becomes a
`LibPlcTagException`. Thus, the handle always has the length of the array that was set last, and
`plc_tag_write` sends exactly that many bytes.

The bounds check of `plc_tag_set_raw_bytes` cannot refuse the array, because the size check runs
against the size that `plc_tag_set_size` has just set. Nothing in the wrapper or in the core compares
the array with the width of the tag on the controller.

## A longer array is sent to the controller

The core grows the handle and sends the full array. What happens next depends on the controller.
Measured with core 2.6.0 against `ab_server` 2.7.2:

| Tag on the controller | Handle before | Array | Write | The controller afterwards |
|-----------------------|---------------|-------|-------|---------------------------|
| Logix `DINT[3]` | 1 element, 4 bytes | 8 bytes | `PLCTAG_ERR_BAD_PARAM` | unchanged |
| PCCC `N7:3` | 1 element, 2 bytes | 4 bytes | `PLCTAG_STATUS_OK` | `N7:3` **and `N7:4`** written |

A PCCC write addresses a data file and a byte count, so a longer array writes into the next element
and reports success. `ab_server` only simulates a controller. How a real SLC 500 or a real Logix
controller answers has not been measured.

## A shorter array is a shorter write

The core shrinks the handle and sends only the bytes of the array:

| Tag on the controller | Handle before | Array | Write | The controller afterwards |
|-----------------------|---------------|-------|-------|---------------------------|
| Logix `DINT[3]` | 1 element, 4 bytes | 2 bytes | `PLCTAG_STATUS_OK` | only the first 2 bytes written |
| PCCC `N7:3` | 1 element, 2 bytes | 1 byte | `PLCTAG_STATUS_OK` | only the low byte written |

No stale bytes from an earlier read go to the controller, because the handle no longer holds them.
The bytes that the array does not cover keep their value on the controller.

## The typed `Tag<M,T>` path

Our client does not use the typed wrapper. `Tag<M,T>.WriteAsync` initializes the handle if it is
new, then calls `EncodeAll()` before it hands off to the native write (`TagOfT.cs:194-201`), and
`PlcMapperBase.EncodeArray` encodes element by element through `plc_tag_set_int8`, `plc_tag_set_int32`
and their siblings (`PlcMapperBase.cs:47-60`). These setters do not resize the handle. Each of them checks its offset
against the size of the handle:

| Call | Result |
|------|--------|
| `plc_tag_set_int8` at offset 29 | `0` |
| `plc_tag_set_int8` at offset 30 | `-27` |
| `plc_tag_set_int8` at offset 31 | `-27` |

The elements that fit are copied into the local buffer, and the first one past the end throws out
of `EncodeAll()`. The controller write still never happens, because the exception surfaces on the
returned task before the native write starts. The local buffer is now the head of your oversized
array, though. Catch the exception and write again, or have `AutoSyncWriteInterval` running, and that
partial data is what goes out. Anyone using the typed path has to guard the length themselves.

## A handle that is not initialized refuses the buffer

`SetBuffer` and `SetBit` work only on an initialized handle. On a new handle the wrapper has no native
handle yet, the core answers `plc_tag_set_size` with "Tag not found", and the wrapper throws
`LibPlcTagException` with `Status.ErrorNotFound`. `ReadAsync` initializes the handle itself, so a read
is not affected. A write sets the buffer before it writes, so the first write to a new handle failed.

Both adapters therefore initialize the handle at the start of each read and each write, if it is not
initialized yet. The integration tests of the Legacy dataport against `ab_server` found this.

## What this means for the client

`LibPlcTagAccess.WriteAsync` hands the converter's bytes to `SetBuffer` exactly as encoded, and the
client has no write-side call to `GetSize` at all. Thus, the converter alone decides how many bytes
go to the controller. The core does not refuse a payload of the wrong length. A payload that is too
long can write into the next element of a PCCC data file, and the write reports success.

For a Logix `STRING`, the payload is `.LEN` plus `.DATA`, and the handle shrinks from the full width
of the type to that length. The alignment padding after `.DATA` is not sent.

All of this relies on the tag on the controller being the type the configuration says, and on each
converter encoding the exact width of its type. For Logix, that is what
[configuration verification](../../AllenBradley.Logix.Documentation/ADR/2026-07-21-verifying-configuration-against-the-symbol-table.md)
settles at connect. The interface decision that removed the client's own write buffer is recorded in
[Operations, not accessors](../../AllenBradley.Logix.Documentation/ADR/2026-07-16-operations-not-accessors-over-libplctag.md).

## How it was checked

The behaviour of `SetBuffer` was read from the libplctag.NET source at tag `libplctag-v1.5.2`. The
tables of the longer and the shorter array come from a probe that makes the same two native calls as
`SetBuffer`, with the core 2.6.0 that the `libplctag` 1.5.2 package ships, against `ab_server` 2.7.2.
The table of the typed path comes from a probe that calls `plc_tag_set_int8` on a 30-byte handle,
with the same core 2.6.0, against `ab_server` 2.7.2. Neither probe is checked in.

## Source references

| Location | Role |
|----------|------|
| `NativeTagWrapper.cs:563-570` | `SetBuffer` calls `plc_tag_set_size` with the length of the array, then `plc_tag_set_raw_bytes` |
| `plc_tag_set_size` (core) | Resizes `tag->data` to the new length |
| `plc_tag_set_raw_bytes` (core) | The bounds check `start_offset + buffer_length > tag->size`, then a copy into `tag->data` with no type inspection |
| `TagOfT.cs:194-201` | `Tag<M,T>.WriteAsync` initializes a new handle and runs `EncodeAll()` before the native write |
| `PlcMapperBase.cs:47-60` | `EncodeArray` encodes element by element through the typed setters, each bounds-checked on its own |
