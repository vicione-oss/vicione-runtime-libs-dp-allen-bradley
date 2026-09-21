# Reading a UDT Definition

Learning what a structured tag contains takes **two reads, not one.** `@tags` says that a tag is a
structure and which template it uses; `@udt/<id>` says what is in that template. Neither substitutes
for the other, and a client that wants field names, offsets or member types needs both.

## The pseudo-tags and the CIP services behind them

`@tags` and `@udt/<id>` are names the library intercepts, not tags the controller has. Each one
stands for a paged CIP exchange from
[the Rockwell browse flow](../protocol/allen-bradley-extension/symbolic-tag-data-types.md#9-discovering-what-a-controller-has);
the library does the paging and hands back one buffer, and you parse the buffer.

| Pseudo-tag              | CIP exchange                                                                 |
|-------------------------|------------------------------------------------------------------------------|
| `@tags`                 | `0x55` Get Instance Attribute List on the Symbol object `0x6B`, looped until status `0x00` |
| `Program:<name>.@tags`  | The same, with the request path prefixed by the program's symbolic segment   |
| `@udt/<id>`             | `0x03` Get_Attribute_List, then `0x4C` Read Template, on Template instance `<id>` |

The buffer layouts below are **the library's own**, not the wire format: `@tags` is the `0x55`
record with the attributes in the order the library asked for them, and `@udt/<id>` is a header the
library synthesises from the attribute reply, followed by the raw template payload.

## `@tags` identifies, it does not describe

Each listing entry is a fixed 22-byte header followed by the tag's name (`eip_cip_special.c:133-140`):

| Offset | Size | Field          | Notes                                                                                     |
|--------|------|----------------|-------------------------------------------------------------------------------------------|
| 0      | 4    | instance id    | The tag's instance in the Symbol object. A client that addresses tags by name never needs it. |
| 4      | 2    | symbol type    | The bitfield: structure flag, array rank, atomic code *or* template id.                   |
| 6      | 2    | element length | Bytes occupied by **one** element — 4 for a `DINT`, 88 for a `STRING`.                     |
| 8      | 12   | dimensions     | Three `UINT32`s. Only as many as the rank in the symbol type are meaningful.               |
| 20     | 2    | name length    | Bytes of name that follow.                                                                 |
| 22     | *n*  | name           | ASCII, not null-terminated.                                                                |

The type reference is packed into the 16-bit **symbol type**, whose `0x0FFF` bits are the UDT id
when `0x8000` marks the entry as a structure. The full bitfield is in
[symbolic tag data types](../protocol/allen-bradley-extension/symbolic-tag-data-types.md#the-logix-symbol-type-bitfield);
what matters here is that the entry stops at the id. Element length tells you the instance is 88 bytes
long, never that those bytes are `.LEN` and `.DATA[82]`.

Two properties of the listing follow from this and matter to any client:

- **Only top-level tags are listed.** A structure member (`Counter.PRE`, `MyString.LEN`) is readable
  by name but has no entry of its own, so a name-keyed lookup finds nothing for it.
- **Programs appear as entries** whose name begins `Program:`. They are how a client discovers program
  scopes, each of which is then listed separately via `Program:<name>.@tags`.

## `@udt/<id>` returns a synthesised header, then raw template bytes

One `@udt/` handle performs **two CIP exchanges** behind a single read: a metadata request for the
template's attributes (`eip_cip_special.c:2241`), then a fielded template read that may fragment
across several round-trips (`:2517`).

The buffer you get back is **not a wire structure.** libplctag builds a 14-byte header of its own
from the `Get_Attribute_List` reply (`eip_cip_special.c:2130-2179`) and appends the raw template
payload after it (`:2455`):

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

In this tree the same two halves are
[`TagsDecoder.cs`](../../../src/AllenBradley.Logix/Client/Tags/Symbols/TagsListing/TagsDecoder.cs) and
[`TemplateDecoder.cs`](../../../src/AllenBradley.Logix/Client/Tags/Symbols/Templates/TemplateDecoder.cs),
both pure functions over the buffer, and the worklist walk is in
[`SymbolTableLoader.cs`](../../../src/AllenBradley.Logix/Client/Tags/Symbols/SymbolTableLoader.cs).

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
