# Reading a UDT Definition

Learning what a structured tag contains takes **two reads, not one.** `@tags` says that a tag is a
structure and which template it uses; `@udt/<id>` says what is in that template. Neither substitutes
for the other, and a client that wants field names, offsets or member types needs both.

## `@tags` identifies, it does not describe

Each listing entry is a fixed header followed by the tag's name: instance id, symbol type, element
length, three array dimensions, name length, name (`eip_cip_special.c:133-140`). The type reference
is packed into the 16-bit **symbol type**, whose `0x0FFF` bits are the UDT id when `0x8000` marks the
entry as a structure. The full bitfield and entry layout are in
[symbolic tag data types](../cip-protocol/symbolic-tag-data-types.md#9-discovering-what-a-controller-has);
what matters here is that the entry stops at the id. Element length tells you the instance is 88 bytes
long, never that those bytes are `.LEN` and `.DATA[82]`.

## `@udt/<id>` returns a synthesised header, then raw template bytes

One `@udt/` handle performs **two CIP exchanges** behind a single read: a metadata request for the
template's attributes (`eip_cip_special.c:2241`), then a fielded template read that may fragment
across several round-trips (`:2517`).

The buffer you get back is **not a wire structure.** libplctag builds a 14-byte header of its own
(`eip_cip_special.c:2130-2179`) and appends the raw template payload after it (`:2455`):

| Offset | Size | Field |
|--------|------|-------|
| 0 | 2 | UDT id (matches the id you asked for — worth asserting) |
| 2 | 4 | Member description size, in 32-bit words |
| 6 | 4 | **Instance size in bytes** — the size of the structure as stored |
| 10 | 2 | Member count |
| 12 | 2 | Structure handle (the template CRC that read replies carry) |

From offset 14 the raw payload runs, in this order:

1. **One descriptor per member**, 8 bytes each: `uint16` metadata (array element count, or the bit
   position for a BOOL packed into a backing word), `uint16` member type, `uint32` byte offset within
   the instance.
2. **The UDT's own name**, zero-terminated — but Rockwell appends encoding junk, so the name ends at
   the first `;`.
3. **The member names**, zero-terminated, in descriptor order.

Everything is little-endian and reachable with the ordinary typed accessors on the handle.

## Nested UDTs mean more reads

A member's type word carries the same encoding as a listing entry's. A member with `0x8000` set and
`0x1000` clear is itself a structure, and `type & 0x0FFF` is the child template's id — which is
another `@udt/` read. Discovery is therefore a graph walk, not a single pass;
`list_tags_logix.c:834-840` does it with a worklist plus a memo array so each id is fetched once.

## Limits

- **Ids are 0–4095.** `@udt/` parses the id and rejects anything outside that range with
  `PLCTAG_ERR_OUT_OF_BOUNDS` (`eip_cip_special.c:1943-1947`) — the same 12-bit space the symbol type
  encodes.
- **Logix and Micro800 only.** The `@raw` / `@tags` / `@udt/` names are recognised as special only for
  those two PLC types (`ab_common.c:674-688`). On any other type the string is never treated as
  special and is parsed as an ordinary tag address.
- **Program scope needs its own listing.** `@tags` returns controller-scoped tags; a program's tags
  come from `Program:<name>.@tags`, discovered from the `Program:`-prefixed entries in the
  controller-scoped listing.

## Working reference

`src/tools/list_tags_logix/list_tags_logix.c` in the libplctag tree is the end-to-end implementation
of both halves: `process_tag_entry()` (line 510) decodes a listing entry, `get_udt_definition()`
(line 704) decodes a template and queues the nested ids it finds.

## Source references

| Location | Role |
|----------|------|
| `eip_cip_special.c:133-140` | `tag_list_entry` — the `@tags` listing entry layout |
| `eip_cip_special.c:1928-1960` | `setup_udt_tag` — id parsing, the 0–4095 range check |
| `eip_cip_special.c:2241` / `:2517` | The two request builders: template metadata, then fields |
| `eip_cip_special.c:2130-2179` | The 14-byte header libplctag synthesises |
| `eip_cip_special.c:2455` | Template payload appended after the header, fragment by fragment |
| `ab_common.c:674-688` | `@`-tag recognition, gated on Logix / Micro800 |
| `list_tags_logix.c:510` / `:704` / `:834-840` | Entry decode, template decode, nested-UDT worklist |
