# Reading a PCCC String Element

libplctag leaves the characters of an `ST` element swapped in the raw tag buffer. The swap is applied
only by the string functions `plc_tag_get_string` and `plc_tag_set_string`. A client that reads and
writes the element through `GetBuffer` and `SetBuffer` has to swap each character pair itself. The
layout of the element is described in
[PCCC data-file types](../protocol/allen-bradley-extension/pccc-data-file-types.md#string-elements).

All line numbers refer to the source of the native core at tag `v2.6.0`, the version that the `libplctag`
1.5.2 package ships, of the .NET wrapper `libplctag` 1.5.2, and of `ab_server` 2.7.2.

## SLC 500 and MicroLogix swap the same way

`AB_PLC_SLC` and `AB_PLC_MLGX` share one branch in `ab_common.c:322-339`, and both get
`slc_tag_byte_order`. The route to the controller, direct or over DH+, does not change the choice. The
PLC-5 has its own struct, `plc5_tag_byte_order`, with the same string settings.

The settings in `slc_tag_byte_order` (`eip_slc_pccc.c:67-87`) that apply to a string:

| Attribute | Value |
|-----------|-------|
| `str_is_byte_swapped` | 1 |
| `str_is_counted` | 1 |
| `str_count_word_bytes` | 2 |
| `str_max_capacity` | 82 |
| `str_total_length` | 84 |
| `str_is_fixed_length` | 1 |
| `str_is_zero_terminated` | 0 |
| `str_pad_bytes` | 0 |

## The raw path copies bytes unchanged

`plc_tag_get_raw_bytes` (`lib.c:3901`) and `plc_tag_set_raw_bytes` (`lib.c:3840`) copy byte by byte
from and to `tag->data`. Neither reads `byte_order`. The PCCC read and write handlers copy the reply
into `tag->data`, and the request out of it, with `mem_copy` (`eip_slc_pccc.c:432` and `:545`). The
PLC-5 handlers and the DH+ handlers do the same.

## Where the swap is applied

`lib.c` reads `str_is_byte_swapped` directly. There is no separate helper.

- `plc_tag_get_string` (`lib.c:3495`) reads character `i` at
  `(i ^ str_is_byte_swapped) + string_start_offset + str_count_word_bytes`.
- `plc_tag_set_string` (`lib.c:3591`) writes it at the same index, with the same expression.

For an `ST` element, character `n` is therefore at byte `2 + (n XOR 1)`. The count word is a plain
little-endian `INT` in bytes 0 and 1 (`lib.c:3629-3630`, `:4458`), and its bytes are never swapped.

## The tag is 84 bytes

`parse_pccc_file_type` (`pccc.c:844-849`) gives an `ST` address an element size of 84 bytes and the
file type `0x8d`. The tag size is `elem_count * elem_size` (`ab_common.c:458-463`), so a handle for
`ST9:1` is 84 bytes from creation. For PCCC the address sets the size, and an `elem_size` attribute
is ignored. A sub-element changes it. `ST9:1.LEN` is 2 bytes and `ST9:1.DATA` is 82
(`pccc.c:1043-1044`).

## SetBuffer resizes the handle

`Tag.SetBuffer(byte[])` in the wrapper calls `plc_tag_set_size(buffer.Length)` before
`plc_tag_set_raw_bytes` (`NativeTagWrapper.cs:563`). The bounds check in `plc_tag_set_raw_bytes`
therefore always passes. An array of 84 bytes leaves the handle as it is. An array of another length
changes `tag->size`, and the next write sends that many bytes (`eip_slc_pccc.c:584`). The next read
still asks for 84 bytes (`eip_slc_pccc.c:314`) but checks the reply against the new size
(`eip_slc_pccc.c:419`), and fails with `PLCTAG_ERR_TOO_LARGE` or `PLCTAG_ERR_TOO_SMALL`.

`GetBuffer()` calls only `plc_tag_get_raw_bytes` (`NativeTagWrapper.cs:545`).

## What ab_server can and cannot show

`ab_server` accepts `--plc=SLC500` and `--plc=Micrologix` (`main.c:516-537`). It knows one string
file, `ST18`, with type `0x8d` and 84 bytes per element (`main.c:626-630`, `plc.h:100-101`), so a
test uses `--tag=ST18[n]` and the address `ST18:x`. Its PCCC read and write handlers copy raw bytes
without a swap, and they refuse a sub-element with `ADDR_NOT_USABLE`, so `ST18:0.LEN` does not work.

A round trip through `ab_server` shows that libplctag does not change the bytes. It cannot show which
byte of a word holds the first character, because the simulator has no opinion about it. Only a real
controller answers that. Write `Hi` to an `ST` element in RSLogix 500 and read the element: the bytes
must be `02 00 69 48`.

## What this means for the client

The Legacy `StringConverter` swaps each character pair on decode and on encode, and leaves the count
word as it is. It always encodes the whole 84-byte element, so `SetBuffer` never changes the size of
the handle.

## Source references

| Location | Role |
|----------|------|
| `ab_common.c:322-339` | SLC and MicroLogix share `slc_tag_byte_order` |
| `ab_common.c:965-977` | `plc=slc`, `slc500`, `micrologix` and `mlgx` map to `AB_PLC_SLC` and `AB_PLC_MLGX` |
| `ab_common.c:458-463`, `:542` | Tag size from the element size of the address |
| `eip_slc_pccc.c:67-87` | `slc_tag_byte_order`, with `str_is_byte_swapped = 1` |
| `eip_plc5_pccc.c:68-88` | `plc5_tag_byte_order`, with the same string settings |
| `eip_slc_pccc.c:314`, `:419`, `:432` | Read request size, reply size check and copy into `tag->data` |
| `eip_slc_pccc.c:545`, `:584` | Copy out of `tag->data` and the size of the write request |
| `lib.c:3495`, `:3591` | The swap in `plc_tag_get_string` and `plc_tag_set_string` |
| `lib.c:3840`, `:3901` | `plc_tag_set_raw_bytes` and `plc_tag_get_raw_bytes`, byte copies without a swap |
| `pccc.c:844-849`, `:1043-1044` | Element size of `ST`, and of its `.LEN` and `.DATA` sub-elements |
| `NativeTagWrapper.cs:545`, `:563` | `GetBuffer` and `SetBuffer`, the latter with `plc_tag_set_size` first |
| `ab_server/main.c:516-537`, `:626-630`, `plc.h:100-101` | PLC types and the fixed `ST18` file of the simulator |
