# Writing a Single Bit

A PCCC tag name that ends in a bit number, such as `B3:2/5`, gives a bit tag. libplctag reads and
writes such a tag through `GetBit` and `SetBit` and refuses every call on its buffer. On an SLC 500 or
a MicroLogix with its own Ethernet port, a write sends a mask, and the controller changes only the
named bit. Over a DH+ bridge, and on a Logix controller that answers PCCC, the write sends the whole
word, and the other 15 bits are replaced with the content of the handle.

Everything here comes from the source of core 2.6.0, the version that the `libplctag` 1.5.2 package
ships, and of libplctag.NET 1.5.2. A bit write has run against the `ab_server` simulator, but not yet
against a Legacy controller.

## How the core recognises a bit tag

The PCCC address parser accepts a `/` and a bit number after the element (`pccc.c:1155-1183`). The
number must be from 0 to 15, and a larger one fails with `PLCTAG_ERR_OUT_OF_BOUNDS`. The element must
be two bytes wide, and any other size fails with `PLCTAG_ERR_BAD_PARAM`. `B3:2/5` and `N7:0/15` are bit
tags, and `F8:0/3` is refused.

`ab_common.c:1020-1055` then marks the tag with `is_bit` and keeps the bit number. For every PCCC PLC
type, the core takes the element size from the file type in the address (`ab_common.c:542`). This
happens before the `elem_size` attribute is read (`ab_common.c:292` against `:458`), so an
`ElementSize` set on the `Tag` has no effect.

## What the handle accepts

| Call | On a bit tag |
|------|--------------|
| `GetBuffer`, `SetBuffer` | Refused with `PLCTAG_ERR_UNSUPPORTED` (`lib.c:3931`, `lib.c:3870-3891`) |
| `GetBit(n)`, `SetBit(n, v)` | Read or set the bit from the name. The offset `n` is ignored (`lib.c:2267`, `lib.c:2314`) |
| `GetUInt8`, `SetUInt8` | Forwarded to `get_bit` and `set_bit` for the bit from the name (`lib.c:3030`, `lib.c:3082-3087`) |
| `GetFloat64`, `SetFloat64` | Refused with `PLCTAG_ERR_UNSUPPORTED` (`lib.c:3223`, `lib.c:3286`) |

A read brings the whole word into the handle. `GetBit` picks the named bit out of it. `SetBit` changes
that one bit in the handle and leaves the other 15 as they were: the value of the last read, or zero
on a handle that was never read.

## What a write sends

The core picks the request from the PLC type and from whether the session goes through a DH+ bridge
(`ab_common.c:322-337`). The bridge case applies when the `path` attribute ends in a DH+ address.

| Controller and route | Request | Effect on the other 15 bits |
|----------------------|---------|-----------------------------|
| SLC 500 or MicroLogix, own Ethernet port | Function `0xAB`, the word with a mask that has only the named bit set (`eip_slc_pccc.c:524-583`) | Unchanged. The controller applies the mask |
| SLC 500 or MicroLogix, DH+ bridge | Function `0xAA`, the whole word, no mask (`eip_slc_dhp.c:455`) | Replaced with the content of the handle |
| PLC-5, own Ethernet port or DH+ bridge | Function `0x26`, read-modify-write with an AND mask and an OR mask (`eip_plc5_pccc.c:627`, `eip_plc5_dhp.c:518`) | Unchanged. The controller applies both masks |
| Logix controller answering PCCC | Function `0x67`, the whole word, no mask (`eip_lgx_pccc.c:602`) | Replaced with the content of the handle |

The masked requests make the controller do the read-modify-write in one step, so no change by the
program can be lost. On the other two routes, a client that has never read the handle writes zeros
into 15 bits. A client that reads first narrows the gap to the time between its read and its write,
but a bit that the program changes in that time is still overwritten.

## The typed `Tag<BoolPlcMapper, bool>`

The scalar `BoolPlcMapper` decodes with `GetUInt8(0) != 0` and encodes with `SetUInt8(0, 255)` or
`SetUInt8(0, 0)`, and it asks for an element size of 1 (`BoolPlcMapper.cs`). On a PCCC bit tag the
core forwards both calls to the named bit and ignores the element size, so in core 2.6.0 the typed
tag reads and writes the right bit, bits 8 to 15 included. Our client still does not use the typed
wrapper, for the reasons in [writing into the tag buffer](writing-into-the-tag-buffer.md).

## What this means for the client

`LibPlcBitTagAccess` opens a plain `Tag` with the bit address as its name and sets no element size. A
read calls `ReadAsync` and then `GetBit(0)`. A write calls `SetBit(0, value)` and then `WriteAsync`,
with no read before it. Both first initialize the handle if it is new, because `SetBit` fails on a
handle that is not initialized ([writing into the tag buffer](writing-into-the-tag-buffer.md)). The rest
of the client works in bytes, so the access hands the bit on as one byte through `BitCodec`, `0x01` for
a set bit and `0x00` for a clear one.

Writing without a read is correct only where the controller applies a mask. The Legacy dataport
supports bits on SLC 500 and MicroLogix controllers with their own Ethernet port. A bit write over a
DH+ bridge would replace the other 15 bits of the word, so the dataport must not write bits on that
route.

## How it was checked

The behaviour above was read from the source of the libplctag core at tag `v2.6.0`, the version that
the `libplctag` 1.5.2 package of this repository ships, and of libplctag.NET at tag `libplctag-v1.5.2`.
An integration test against libplctag's `ab_server` 2.7.2, which simulates an SLC 500, writes bit
`B3:2/5` through the client and reads it back, and it passes. `ab_server` is a simulator, so this does
not prove how a real controller behaves, and the test does not look at the other bits. An integration
test on a Legacy controller still has to confirm that a write of one bit leaves the other 15 unchanged.

## Source references

| Location | Role |
|----------|------|
| `pccc.c:1155-1183` | Parses `/b`: bit 0 to 15, two-byte elements only |
| `ab_common.c:1020-1055` | Sets `is_bit` and `bit` for PLC-5, Logix PCCC, SLC 500 and MicroLogix tags |
| `ab_common.c:292`, `:542`, `:458` | The element size comes from the address before the `elem_size` attribute is read |
| `ab_common.c:322-337` | Picks the SLC request code for a direct session or a DH+ bridge |
| `lib.c:2245-2335` | `plc_tag_get_bit` and `plc_tag_set_bit` use the bit from the name on a bit tag |
| `lib.c:3030`, `:3082-3087` | `plc_tag_get_uint8` and `plc_tag_set_uint8` forward to the bit on a bit tag |
| `lib.c:3870-3891`, `:3931` | `plc_tag_set_raw_bytes` and `plc_tag_get_raw_bytes` refuse a bit tag |
| `eip_slc_pccc.c:524-583` | The mask and function `0xAB` of a direct SLC bit write |
| `eip_slc_dhp.c:455` | Function `0xAA` of an SLC write over DH+, with no bit handling |
| `eip_plc5_pccc.c:525-560`, `:627` | The AND and OR masks and function `0x26` of a PLC-5 bit write |
| `eip_lgx_pccc.c:602` | Function `0x67` of a Logix PCCC write, with no bit handling |
| `defs.h:102-108` | The PCCC function codes |
| `BoolPlcMapper.cs` (wrapper) | The scalar decode and encode through `GetUInt8` and `SetUInt8` |
