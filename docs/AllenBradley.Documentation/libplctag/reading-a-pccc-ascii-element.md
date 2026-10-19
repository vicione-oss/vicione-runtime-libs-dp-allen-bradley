# Reading a PCCC ASCII Element

libplctag gives an `A` element a size of 1 byte, but an SLC 500 or MicroLogix keeps two characters in
each `A` element, one word. A handle for `A9:0` with the default element count therefore asks for one
byte, and gets at most one of the two characters. A client that wants the whole word sets
`elem_count=2`.

All line numbers refer to the source of the native core at tag `v2.6.0`, the version that the
`libplctag` 1.5.2 package ships, and of `ab_server` 2.7.2.

## The size of the handle

`parse_pccc_file_type` (`pccc.c:702-708`) gives the letter `A` the file type `PCCC_FILE_ASCII` and an
element size of 1 byte. The comment at the top of the file (`pccc.c:73`) describes the file as "1 byte
per character". The tag size is `elem_count * elem_size` (`ab_common.c:442`, `:463`), and
`elem_count` is 1 unless the tag string sets it.

The read request asks for `elem_size * elem_count` bytes (`eip_slc_pccc.c:314`), and the reply must
have exactly `tag->size` bytes (`eip_slc_pccc.c:419`). The write request sends `tag->size` bytes
(`eip_slc_pccc.c:584`).

## Why the element count is 2

Two settings can give a 2-byte handle. An `elem_size` attribute is ignored for a PCCC address, because
the address sets the size. A `SetBuffer` of 2 bytes resizes the handle
([reading a PCCC string element](reading-a-pccc-string-element.md#setbuffer-resizes-the-handle)), but
only at the first write. A read before that asks for 1 byte, and a read after it asks for 1 byte and
fails the size check, because the handle is then 2 bytes.

`elem_count=2` sets the size when the handle is created, so each read asks for 2 bytes and each write
sends 2. The Legacy `LegacyTagAccessFactory` sets it for every `A` address.

## The two characters

The raw buffer holds the two bytes of the word unchanged. The Legacy `AsciiConverter` swaps the two
characters the same way as in an `ST` element: the first character is in the high byte, which comes
second on the wire. No manual says this for `A` files.

## What a real controller must confirm

`ab_server` knows only the files `B3`, `N7`, `F8`, `ST18` and `L19` (`main.c:611-635`). It has no
`A` file, so only a controller can answer these questions:

1. Does a 2-byte read of `A9:0` return the word of element 0, and not two half-words?
2. Is the first character in the high byte, as in an `ST` element?

To check both, write `Hi` to `A9:0` in RSLogix 500 and read the element. The bytes must be `69 48`.

## Source references

| Location | Role |
|----------|------|
| `pccc.c:73`, `:702-708` | `A` is `PCCC_FILE_ASCII`, with 1 byte per element |
| `ab_common.c:442`, `:463` | `elem_count` defaults to 1, and the tag size is `elem_count * elem_size` |
| `eip_slc_pccc.c:314`, `:419` | Read request size and the reply size check |
| `eip_slc_pccc.c:584` | Size of the write request |
| `ab_server/main.c:611-635` | The data files of the simulator, with no `A` file |
