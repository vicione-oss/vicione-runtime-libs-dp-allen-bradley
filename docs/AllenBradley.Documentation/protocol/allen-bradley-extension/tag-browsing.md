# Tag Browsing: Discovering What a Controller Has

How a client learns which tags a Logix controller holds, what type each one is, and what the members
of a structure are. This document is client-agnostic. It describes the Rockwell objects and services
on the wire, not how any specific library packages them.

Standard CIP has [no online browse](../cip/networking-overview.md#discovery-in-standard-cip).
Rockwell adds two vendor-range objects that together describe every tag. The Symbol object holds one
instance per tag, and the Template object holds one instance per structure layout. Between them,
three services do the work:

| Service                     | Code   | Target          | Standard? |
|-----------------------------|--------|-----------------|-----------|
| Get Instance Attribute List | `0x55` | Symbol object   | Rockwell  |
| Get_Attribute_List          | `0x03` | Template object | *(std)*   |
| Read Template               | `0x4C` | Template object | Rockwell  |

Rockwell's manual speaks of *retrieving symbol instances*. "Tag list", "tag browsing" and "tag
upload" are informal names for the same thing. Which of them this repo uses is settled in
[CONTEXT.md](../../../../CONTEXT.md).

## The Symbol object (class 0x6B)

One instance per controller-scoped tag. Program-scoped tags are separate instances, reached by
prefixing the request path with the symbolic segment `Program:<name>`. The attributes a client asks
for:

| Attribute | Content                                             |
|-----------|-----------------------------------------------------|
| 1         | Name: UINT length, then the characters              |
| 2         | Symbol type: the UINT bitfield below                |
| 7         | Base type size: bytes occupied by one element       |
| 8         | Dimensions: three UDINTs                            |

## Service 0x55: Get Instance Attribute List

The standard `Get_Attribute_List` (`0x03`) reads several attributes of one instance. `0x55` reads
several attributes of many instances in one request, which is what makes a listing affordable.

The path is class `0x6B`, instance *N*, where *N* is a starting point meaning "instances with id at
or above *N*". Start at 0. The request data is a UINT attribute count followed by the attribute ids,
so `02 00 | 01 00 | 02 00` asks for name and type. The reply data is a run of packed records, each a
UDINT instance id followed by the requested attributes in request order, as many as fit in one
packet.

| Status | Meaning                                                                   |
|--------|---------------------------------------------------------------------------|
| `0x06` | Packet full, more instances exist. Repeat with *N* = last received id + 1 |
| `0x00` | List complete                                                             |

Instance ids increase but are not contiguous. Continue from the last id received, never count
upwards. Entries whose name begins `Program:` are programs, not tags. Each is listed separately with
the `Program:<name>` prefix.

## The Logix symbol-type bitfield

Each Symbol instance carries a 16-bit symbol type value (attribute 2). Its bit layout:

| Mask     | Bits  | Meaning                                                              |
|----------|-------|----------------------------------------------------------------------|
| `0x8000` | 15    | **1 = structure** (UDT / AOI / predefined struct); 0 = atomic        |
| `0x6000` | 14-13 | **array dimension count** (0-3) = `(type & 0x6000) >> 13`            |
| `0x1000` | 12    | **system / reserved tag** (predefined; typically filtered out)       |
| `0x0FFF` | 11-0  | when structured: the **template (UDT) instance id** (max 4096)       |
| `0x00FF` | 7-0   | when atomic: the **CIP elementary type code** *(std)* (e.g. `0xC4` = DINT) |
| `0x0700` | 10-8  | when atomic: **bit position** for a BOOL aliased to a bit of a word  |

A `BOOL` array reports `DWORD` (`0xD3`), the packing word described under
[BOOL handling](symbolic-tag-data-types.md#bool-handling). The production decoder of this
decomposition is
[`SymbolType.cs`](../../../../src/AllenBradley.Logix/Client/Tags/Symbols/SymbolTypes/SymbolType.cs).

## The Template object (class 0x6C)

A structured symbol names only its template id. The template is what turns that id into members,
and it takes two services to read.

The first is `Get_Attribute_List` (`0x03`, *(std)*) on the template instance:

| Attribute | Content                                              |
|-----------|------------------------------------------------------|
| 1         | Structure handle (CRC), the one a read reply carries |
| 2         | Member count                                         |
| 4         | Definition size, in 32-bit words                     |
| 5         | Structure size, in bytes                             |

The second is Read Template (`0x4C`) with an offset and a byte count of `(attribute 4 × 4) − 23`.
Loop while the status is `0x06`, advancing the offset. The payload, concatenated, holds three
things in order:

1. `member count` × 8-byte member descriptors:

   | Field  | Type  | Content                                    |
   |--------|-------|--------------------------------------------|
   | info   | UINT  | Array length, or bit number for a `BOOL`   |
   | type   | UINT  | Same encoding as the symbol type above     |
   | offset | UDINT | Byte offset of the member in the structure |

2. The template name, null-terminated. Ignore everything after `;`.
3. The member names, each null-terminated, in descriptor order.

A member whose type has bit 15 set is itself a structure, and its `0x0FFF` bits name the child
template, which means another template read. Members named `ZZZZZZZZZZ…` or `__…` are the hidden
host bytes that back packed `BOOL`s, see [BOOL handling](symbolic-tag-data-types.md#bool-handling). The production decoder of the payload is
[`TemplateDecoder.cs`](../../../../src/AllenBradley.Logix/Client/Tags/Symbols/Templates/TemplateDecoder.cs).

## The browse flow

1. `0x55` on class `0x6B` until status `0x00` gives the controller-scoped tags.
2. Collect the `Program:*` entries and repeat step 1 with the `Program:<name>` prefix for each.
3. For each tag with bit 15 set, read its template by the id in bits 11-0. Cache by id.
4. Recurse into members that are themselves structures.
5. Read and write tags, and decode structures with the cached templates.

Micro800 has no program scope and browses tags only from firmware v10 onward; see
[what Micro800 exposes](symbolic-tag-data-types.md#what-micro800-exposes). How libplctag packages
this flow behind its `@tags` and `@udt/<id>` pseudo-tags, and the buffers it hands back, is in
[reading a UDT definition](../../libplctag/reading-a-udt-definition.md). What the two-pass listing
means for the port's configuration tree is in the Logix port's
[node model](../../../AllenBradley.Logix.Documentation/explanation/model/node-model.md#scope-is-a-container-not-a-property).

## References

### Rockwell publications

- Rockwell Automation, *Logix 5000 Controllers Data Access* (1756-PM020), on the Symbol and
  Template objects, service `0x55`, and the symbol type:
  <https://literature.rockwellautomation.com/idc/groups/literature/documents/pm/1756-pm020_-en-p.pdf>

### Reference implementations

- libplctag, `list_tags_logix.c`, an end-to-end `0x55` listing and template walk:
  <https://github.com/libplctag/libplctag/blob/release/src/tools/list_tags_logix/list_tags_logix.c>
- pycomm3, `LogixDriver` tag and type parsing, the symbol-type bitfield:
  <https://github.com/ottowayi/pycomm3/blob/master/pycomm3/cip/data_types.py>
