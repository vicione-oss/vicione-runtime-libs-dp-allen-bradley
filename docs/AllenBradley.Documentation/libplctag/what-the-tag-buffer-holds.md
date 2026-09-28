# What the Tag Buffer Holds

The tag buffer is payload only, in the controller's own memory layout. `plc_tag_get_raw_bytes`
(`lib.c:4294`) is a byte-for-byte copy out of `tag->data + offset` (`lib.c:4328-4340`) that never
inspects the tag's data type, and the .NET wrapper's `Tag.GetBuffer()` copies the whole buffer
through it. There is no decode step in that path to reason about. Whatever the protocol layer put in
the buffer is exactly what comes back out.

Two questions follow from that, and they have different answers: what the protocol layer stripped
before filling the buffer, and what it left in.

## Framing is already gone

The protocol layer removes its own envelope before the copy, so the first byte of the buffer is the
first byte of the value.

For CIP, the reply's leading type prefix is split off into `tag->encoded_type_info` rather than the
data buffer (`eip_cip.c:1365-1399`), and re-attached when the tag is written back
(`eip_cip.c:1033-1035`, `:1185-1187`). This is why a Logix `STRING` buffer starts at `.LEN` and not
at the `A0 02 HH HH` abbreviated-structure marker. The stripped bytes are not lost. They are readable
as the `raw_tag_type_bytes` byte-array attribute, sized by `raw_tag_type_bytes.length`
(`ab_common.c:1102`, `:1025`).

For Modbus, the copy starts past the MBAP header, the function code and the byte count
(`modbus.c:2617`).

## The controller's layout is still there

What the library does not do is normalise the payload. The bytes are the PLC's in-memory
representation. A Logix `STRING` carries its `DINT` count word and its trailing alignment padding. A
UDT carries the member alignment padding between its fields, so offsets come from the template, not
from summing member sizes. BOOL arrays are bit-packed into 32-bit words. And everything is in wire
byte order. The tag's byte-order attributes are applied by the typed accessors (`plc_tag_get_uint32`
and friends), not by the raw copy, so a raw buffer is unswapped.

The layouts themselves (`STRING`, `TIMER`, BOOL packing, the structure marker) are documented once
in [symbolic tag data types](../protocol/allen-bradley-extension/symbolic-tag-data-types.md), which
this does not repeat.

## The `@` tags are the deliberate exception

`@raw`, `@tags` and `@udt/<id>` put protocol metadata into the buffer on purpose. That is their
entire point, and it is what makes browsing possible from the ordinary read path. A `@tags` buffer is
a run of listing entries, and a `@udt/<id>` buffer is a header libplctag synthesises followed by raw
template bytes (see [reading a UDT definition](reading-a-udt-definition.md)). Everywhere else, the
"payload only" rule holds.

## Gotchas

| Behaviour | Detail |
|-----------|--------|
| Bit tags are rejected | A tag opened as a single bit returns `PLCTAG_ERR_UNSUPPORTED`; there is no byte to copy |
| Bounds are checked, not clamped | `offset + buffer_size` must be `<= plc_tag_get_size(tag)`, else `PLCTAG_ERR_OUT_OF_BOUNDS` and nothing is copied. It never short-copies, so size the destination from `plc_tag_get_size` first. The write direction has the same check; see [writing into the tag buffer](writing-into-the-tag-buffer.md) |
| The return is a status | `PLCTAG_STATUS_OK` on success, not a byte count. The count you get is the count you asked for |

## What this means for the client

The `STRING` codec's starting offset is settled on the library side. The buffer begins at `.LEN`
because `eip_cip.c` routes the marker and handle into the type-info store. That is the assumption the
[decoding ADR](../../AllenBradley.Logix.Documentation/ADR/2026-07-16-decoding-tag-bytes-into-typed-values.md)
builds every `STRING` and UDT offset on. What remains unconfirmed there is the other side of the
comparison, the element size the controller declares for a `STRING`, which only the device round
trip can pin.

## Source references

| Location | Role |
|----------|------|
| `lib.c:4294` | `plc_tag_get_raw_bytes`, the only bulk read of the buffer |
| `lib.c:4328-4340` | The copy itself: `tag->data + offset`, no type inspection |
| `eip_cip.c:1365-1399` | CIP type prefix split off into `tag->encoded_type_info` |
| `eip_cip.c:1033-1035`, `:1185-1187` | The prefix re-attached on write |
| `ab_common.c:1102`, `:1025` | `raw_tag_type_bytes` and `raw_tag_type_bytes.length` attributes |
| `modbus.c:2617` | Modbus copy starts past MBAP header, function code and byte count |
| `ab_common.c:683-687` | `@raw`, `@tags`, `@udt/`, the metadata-bearing exceptions |
