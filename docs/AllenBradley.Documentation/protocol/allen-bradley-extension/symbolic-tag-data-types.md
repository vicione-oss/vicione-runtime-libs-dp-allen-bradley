# Symbolic Tag Data Types — Logix and Micro800

Which data types exist on the controllers that address data by **symbolic tag name**, what they are
called, how the controller reports them, and the Rockwell objects and services that carry a tag's
name, type and value over CIP. **Client-agnostic** — this describes the controllers and the wire, not
how any specific library handles them.

Everything here is a **Rockwell extension** of CIP, specified in *Logix 5000 Controllers Data
Access* (1756-PM020), not by ODVA. Where a mechanism is standard CIP it is marked *(std)* and linked
to [`../cip/`](../cip/). How each type is *encoded* — type code, byte layout, range, .NET
equivalent — is in the [CIP Data Types Reference](../cip/cip-datatypes-reference.md), which this
document does not repeat. The counterpart for the file-addressed families is
[PCCC Data-File Types](pccc-data-file-types.md).

**Who addresses symbolically:** ControlLogix, CompactLogix, GuardLogix and SoftLogix (programmed in
Studio 5000 Logix Designer), and Micro800 (Connected Components Workbench). The families and their
tooling are listed in [Controller families](controller-families-and-routing.md#the-lines).

---

## 1. What symbolic addressing does to the type question

CIP tag access is **typed and symbolic**: you read and write a whole named tag whose type the
controller already knows, and the reply carries the type ahead of the data. There is no equivalent of
parking an arbitrary byte layout at an arbitrary address, the way Siemens S7comm PUT/GET reads a byte
range out of a DB.

The consequence for this document is that the type vocabulary is decided by the **programming tool**.
What Studio 5000 or CCW lets you declare is what exists in the symbol table, and what appears on the
wire. So the question "which types does this controller have" is answered per engineering environment,
not per protocol — CIP itself defines many more codes than any Allen-Bradley controller will ever
report for a tag.

---

## 2. What Logix exposes

### Atomic types, in two generations

- **Classic Logix** — ControlLogix 5550/5555/5560/5570, CompactLogix 1769/5370 and earlier:
  `BOOL`, `SINT`, `INT`, `DINT`, `LINT`, `REAL`. No unsigned integers, no `LREAL`.
- **5X80 controllers** — ControlLogix 5580, CompactLogix 5380 and 5480 — add the **extended data
  types**: `USINT`, `UINT`, `UDINT`, `ULINT`, and `LREAL`. They need a recent controller firmware and
  a matching Studio 5000 version; for a given catalog number and revision the tool is the arbiter.

### What Logix deliberately does not have

- **`BYTE` / `WORD` / `DWORD` / `LWORD`** are not declarable tag types. Where a bit string is wanted,
  Logix uses `SINT`/`INT`/`DINT`/`LINT` with a hex or binary display style. The codes still cross the
  wire — as members of structures, and from CIP objects other than the Symbol object.
- **The CIP temporal types** (`DATE`, `TIME`, `ITIME`, `FTIME`, `LTIME`, `TIME_OF_DAY`,
  `DATE_AND_TIME`) are not tag types either. Logix keeps wall-clock time as a `LINT` — microseconds
  since the Unix epoch, read through the `WALLCLOCKTIME` object with `GSV` — and keeps durations in
  the `TIMER` structure.
- **The elementary CIP strings** `0xD0` and `0xDA` never describe a Logix `STRING` tag. That type is a
  structure; see [§5](#5-the-logix-string-structure-not-elementary-0xd0).

Everything that is not atomic is a **structure**: `STRING`, `TIMER`, `COUNTER`, `CONTROL`, the motion
`AXIS_*` types, `MSG`, every UDT, and every Add-On Instruction.

---

## 3. What Micro800 exposes

Micro800 is symbolic like Logix but shares neither its type list nor its structure vocabulary. CCW
implements the **IEC 61131-3 elementary type set**, which is the widest atomic vocabulary of any
Allen-Bradley family here: the bit strings `BYTE`/`WORD`/`DWORD`/`LWORD`, the signed and unsigned
integers, `REAL` and `LREAL`, `STRING`, and `TIME`/`DATE`.

What differs beyond the type list:

- **No AOIs.** The equivalent is a **UDFB** (user-defined function block). UDTs are supported.
- **Timers and counters are IEC function-block instances** — `TON`, `TOF`, `CTU`, … — not the Logix
  `TIMER`/`COUNTER`/`CONTROL` predefined structures, so none of [§6](#6-predefined-structures-timer--counter--control)
  applies.
- **CIP access is restricted.** Micro800 addresses tags **symbolically only** (no firmware-v21 Symbol
  Instance Addressing), does not support the Multiple Service Packet service, and could not browse
  tags at all before roughly firmware v10. It tolerates large single packets but depends on fragmented
  services for them.
- **64-bit and bit-string availability is model- and firmware-dependent** (`LWORD`, `LINT`, `ULINT`).
- **Embedded Ethernet** is on Micro820/850/870; the Micro810 and Micro830 are serial/USB and reach
  EtherNet/IP only through a converter.

---

## 4. Availability matrix

Native support — what the programming tool exposes as a first-class tag type. Codes are the CIP
elementary codes from the [reference](../cip/cip-datatypes-reference.md); `✅*️` marks a caveat spelled out
below the table.

| Type                        | Code   | Logix 5X70 & earlier | Logix 5X80 | Micro800 |
|-----------------------------|--------|----------------------|------------|----------|
| **Bool**                    |        |                      |            |          |
| `BOOL`                      | `0xC1` | ✅                    | ✅          | ✅        |
| **Bit strings**             |        |                      |            |          |
| `BYTE`                      | `0xD1` | ❌                    | ❌          | ✅        |
| `WORD`                      | `0xD2` | ❌                    | ❌          | ✅        |
| `DWORD`                     | `0xD3` | ❌                    | ❌          | ✅        |
| `LWORD`                     | `0xD4` | ❌                    | ❌          | ✅*️      |
| **Signed integers**         |        |                      |            |          |
| `SINT`                      | `0xC2` | ✅                    | ✅          | ✅        |
| `INT`                       | `0xC3` | ✅                    | ✅          | ✅        |
| `DINT`                      | `0xC4` | ✅                    | ✅          | ✅        |
| `LINT`                      | `0xC5` | ✅                    | ✅          | ✅*️      |
| **Unsigned integers**       |        |                      |            |          |
| `USINT`                     | `0xC6` | ❌                    | ✅          | ✅        |
| `UINT`                      | `0xC7` | ❌                    | ✅          | ✅        |
| `UDINT`                     | `0xC8` | ❌                    | ✅          | ✅        |
| `ULINT`                     | `0xC9` | ❌                    | ✅          | ✅*️      |
| **Floating point**          |        |                      |            |          |
| `REAL`                      | `0xCA` | ✅                    | ✅          | ✅        |
| `LREAL`                     | `0xCB` | ❌                    | ✅          | ✅        |
| **Date / time**             |        |                      |            |          |
| `TIME` / `DATE`             | `0xDB` / `0xCD` | ❌           | ❌          | ✅        |
| **String**                  |        |                      |            |          |
| `STRING`                    | struct | ✅*️                  | ✅*️        | ✅        |
| **Aggregate**               |        |                      |            |          |
| UDT / `STRUCT`              | `0xA0` | ✅                    | ✅          | ✅        |
| `ARRAY`                     | —      | ✅ (≤3 dims)          | ✅ (≤3 dims)| ✅        |
| `TIMER` / `COUNTER` / `CONTROL` | struct | ✅                | ✅          | ✅*️      |
| AOI                         | struct | ✅                    | ✅          | ✅*️      |

Caveats:

- **Logix `STRING`** is a predefined structure, not the elementary CIP `STRING` — see
  [§5](#5-the-logix-string-structure-not-elementary-0xd0).
- **Micro800 64-bit and bit-string types** are firmware- and model-dependent.
- **Micro800 timers and counters** are IEC function blocks, and its AOI equivalent is the UDFB — see
  [§3](#3-what-micro800-exposes).

---

## 5. The Logix `STRING` structure (not elementary `0xD0`)

On a Logix controller the built-in `STRING` tag is a **predefined structure**, reported with the
abbreviated-structure marker of [§7](#the-structure-reply) — never as
elementary `0xD0`. Its template:

| Member    | Type       | Bytes  | Notes                                                                    |
|-----------|------------|--------|--------------------------------------------------------------------------|
| `.LEN`    | `DINT`     | 4      | Current character count (little-endian). It is a **`DINT`**, not an `INT`. |
| `.DATA`   | `SINT[82]` | 82     | ASCII bytes. `MaxLen` = **82**.                                          |
| (padding) | —          | 2      | Alignment pad so the struct ends on a 32-bit boundary.                   |
| **Total** |            | **88** | 86 data bytes (4 + 82) padded to 88.                                     |

Custom string types (`STRING_20`, `STRING_100`, …) are user-defined structures with the same
`.LEN` + `.DATA[n]` shape and a different `n`. The wire size follows the declared capacity, so the
type name alone does not fix the byte count.

A read reply for a `STRING` containing `"Hi"`:

```text
Offset  Hex           Meaning
------  ----          ----------------------------
0x00    A0 02         ABBREV_STRUCT marker (0x02A0 as a little-endian u16)
0x02    HH HH         2-byte structure handle (template CRC)
0x04    02 00 00 00   .LEN = DINT 2   (little-endian)
0x08    48 69         .DATA[0]='H' (0x48), .DATA[1]='i' (0x69)
0x0A    00 … (80 more)  remaining SINT[82] bytes = 0
        00 00         2 alignment pad bytes
------
88 bytes total for the .LEN + .DATA[82] structure.
```

Readers only need to consume `.LEN` characters; the remaining `.DATA` bytes are unused. The `A0 02`
prefix and handle appear only in the **read reply**, not in the stored structure.

---

## 6. Predefined structures (TIMER / COUNTER / CONTROL)

Each is a 12-byte structure: a hidden 32-bit status/control `DINT` whose high bits are the status
booleans, followed by two `DINT`s.

| Structure | Bytes | Members                                                                                         |
|-----------|-------|-------------------------------------------------------------------------------------------------|
| `TIMER`   | 12    | status `DINT` (`.EN` bit 31, `.TT` bit 30, `.DN` bit 29), `.PRE` `DINT` (ms), `.ACC` `DINT` (ms) |
| `COUNTER` | 12    | status `DINT` (`.CU` 31, `.CD` 30, `.DN` 29, `.OV` 28, `.UN` 27), `.PRE` `DINT`, `.ACC` `DINT`   |
| `CONTROL` | 12    | status `DINT` (`.EN`, `.EU`, `.DN`, `.EM`, `.ER`, `.UL`, `.IN`, `.FD` in high bits), `.LEN` `DINT`, `.POS` `DINT` |

These are Logix types. Micro800 uses IEC function-block instances instead.

---

## 7. How Logix reads and writes a tag on the wire

CIP has no tag services of its own. Rockwell defines them in the object-class-specific code range
(*(std)*, see [Services](../cip/cip-networking-overview.md#services)), which is why `0x4C` below
means one thing on a tag and another on the Template object of [§9](#9-discovering-what-a-controller-has):

| Service                 | Code   | Target                                                        |
|-------------------------|--------|---------------------------------------------------------------|
| Read Tag                | `0x4C` | A tag, by symbolic path or Symbol instance                    |
| Write Tag               | `0x4D` | A tag, by symbolic path or Symbol instance                    |
| Read Tag Fragmented     | `0x52` | A tag whose value exceeds one packet                          |
| Write Tag Fragmented    | `0x53` | Same, for writes                                              |
| Read-Modify-Write       | `0x4E` | Masked bit write into a tag                                   |
| Multiple Service Packet | `0x0A` | Several of the above in one request (Message Router, *(std)*) |

A tag is addressed one of two ways:

- **Symbolic path** — the ANSI Extended Symbol Segment `0x91` *(std, encoding in
  [EPATH](../cip/cip-networking-overview.md#epath--how-a-request-addresses-an-object))*. `MyTag`
  becomes `91 05 4D 79 54 61 67 00`. Members (`Motor.Speed`) chain symbol segments; array elements
  (`Arr[5]`) append a member/element segment (`28 05`); a program-scoped tag is prefixed with the
  symbolic segment `Program:<name>`.
- **Symbol Instance Addressing** — class `0x6B` + the instance id learned from the listing in
  [§9](#9-discovering-what-a-controller-has). Shorter on the wire and faster in the controller,
  available from firmware v21. Micro800 does not support it.

The request data of a Read Tag is the element count (UINT). The reply data starts with the tag's
type, then the value:

- **Atomic tag** — a 2-byte type code (`C4 00` for `DINT`), then the value in the
  [reference](../cip/cip-datatypes-reference.md) layout.
- **Structured tag** — the abbreviated-structure marker and a handle, described next.

### The structure reply

When a structured tag — a UDT, `STRING`, `TIMER`, … — is read, the reply data begins with the
**abbreviated-structure marker** (`0xA0`, one of the constructed type codes in the
[reference](../cip/cip-datatypes-reference.md#7-constructed-type-codes)) followed by a 2-byte
**structure handle**:

```text
A0 02        ← type marker: 0xA0 (ABBREV_STRUCT), 0x02 = a 2-byte handle follows
             (read as a little-endian u16 this is 0x02A0)
HH HH        ← 2-byte structure handle: a CRC of the template's type-encoding string
.. .. ..     ← packed member data (little-endian, with alignment pad bytes)
```

The handle identifies the template; a client matches it against the template definition, read from
the [Template object](#the-template-object-class-0x6c), class `0x6C`. The handle is **not** unique
across differently-ordered structs, so the template must be read to learn the actual member layout.

### Arrays

A Logix array stores its elements **contiguously**, each in the element type's wire layout,
little-endian, with no padding between elements — except the BOOL-array packing in
[§8](#8-bool-handling). An element is addressed by a member/element segment in the request path
(`Arr[5]` → symbol segment for `Arr` plus member segment `28 05`). Logix supports up to **3
dimensions**, encoded in the `0x6000` bits of the symbol type below.

---

## 8. BOOL handling

BOOL is the one type whose storage differs by context:

- **Atomic `BOOL` tag** — 1 byte on the wire (`0x00` / `0xFF`; nonzero = true).
- **`BOOL` array** — packed into **32-bit words**. A `BOOL[]` in ControlLogix/CompactLogix is stored
  and transferred as an array of 32-bit words, and its declared length must be a multiple of 32.
  Consequences:
  - The array index addresses the **word, not the bit**: `bits[3]` selects the 4th 32-bit word
    (bits 96–127). A single-element read returns a whole 32-bit word; extract the bit yourself.
  - Writes are word-granular — changing one bit rewrites the whole 32-bit word, so keep input and
    output BOOL arrays separate to avoid clobbering neighbours.
- **`BOOL` inside a UDT** — packed into a hidden backing field: up to 8 BOOLs share a hidden `SINT`
  byte; more than 8 pack into a `BOOL[32/64]` word array with 32-bit alignment. The template read
  gives the host byte offset and the bit position within it.

Whether a client models the backing word as `DWORD` (`0xD3`) or `DINT` (`0xC4`) is a modeling choice;
the load-bearing wire fact is the **32-bit packing**.

---

## 9. Discovering what a controller has

Standard CIP has [no online browse](../cip/cip-networking-overview.md#discovery-in-standard-cip).
Rockwell adds two vendor-range objects that together describe every tag: the **Symbol object**
holds one instance per tag, and the **Template object** holds one instance per structure layout.
Between them, three services do the work:

| Service                     | Code   | Target          | Standard? |
|-----------------------------|--------|-----------------|-----------|
| Get Instance Attribute List | `0x55` | Symbol object   | Rockwell  |
| Get_Attribute_List          | `0x03` | Template object | *(std)*   |
| Read Template               | `0x4C` | Template object | Rockwell  |

Rockwell's manual speaks of *retrieving symbol instances*. "Tag list", "tag browsing" and "tag
upload" are informal names for the same thing; which of them this repo uses is settled in
[CONTEXT.md](../../../../CONTEXT.md).

### The Symbol object (class 0x6B)

One instance per controller-scoped tag. Program-scoped tags are separate instances, reached by
prefixing the request path with the symbolic segment `Program:<name>`. The attributes a client asks
for:

| Attribute | Content                                             |
|-----------|-----------------------------------------------------|
| 1         | Name — UINT length, then the characters             |
| 2         | Symbol type — the UINT bitfield below               |
| 7         | Base type size — bytes occupied by one element      |
| 8         | Dimensions — three UDINTs                           |

### Service 0x55: Get Instance Attribute List

The standard `Get_Attribute_List` (`0x03`) reads several attributes of *one* instance. `0x55` reads
several attributes of *many* instances in one request, which is what makes a listing affordable:

- **Path:** class `0x6B`, instance *N*. *N* is a starting point — "instances with id ≥ *N*". Start
  at 0.
- **Request data:** a UINT attribute count, then the attribute ids. For name and type:
  `02 00 | 01 00 | 02 00`.
- **Reply data:** packed records, each a UDINT instance id followed by the requested attributes in
  request order — as many records as fit in one packet.

| Status | Meaning                                                                   |
|--------|---------------------------------------------------------------------------|
| `0x06` | Packet full, more instances exist. Repeat with *N* = last received id + 1 |
| `0x00` | List complete                                                             |

Instance ids increase but are **not contiguous**. Continue from the last id received, never count
upwards. Entries whose name begins `Program:` are programs, not tags; each is listed separately with
the `Program:<name>` prefix.

### The Logix symbol-type bitfield

Each Symbol instance carries a 16-bit **symbol type** value (attribute 2). Its bit layout:

| Mask     | Bits  | Meaning                                                              |
|----------|-------|----------------------------------------------------------------------|
| `0x8000` | 15    | **1 = structure** (UDT / AOI / predefined struct); 0 = atomic        |
| `0x6000` | 14–13 | **array dimension count** (0–3) = `(type & 0x6000) >> 13`            |
| `0x1000` | 12    | **system / reserved tag** (predefined; typically filtered out)       |
| `0x0FFF` | 11–0  | when structured: the **template (UDT) instance id** (max 4096)       |
| `0x00FF` | 7–0   | when atomic: the **CIP elementary type code** *(std)* (e.g. `0xC4` = DINT) |
| `0x0700` | 10–8  | when atomic: **bit position** for a BOOL aliased to a bit of a word  |

A `BOOL` array reports `DWORD` (`0xD3`), the packing word of [§8](#8-bool-handling). The
production decoder of this decomposition is
[`SymbolType.cs`](../../../../src/AllenBradley.Logix/Client/Tags/Definitions/SymbolType.cs).

### The Template object (class 0x6C)

A structured symbol names only its template id. The template is what turns that id into members,
and it takes two services to read.

**Get_Attribute_List (`0x03`, *(std)*)** on the template instance:

| Attribute | Content                                              |
|-----------|------------------------------------------------------|
| 1         | Structure handle (CRC), the one a read reply carries |
| 2         | Member count                                         |
| 4         | Definition size, in 32-bit words                     |
| 5         | Structure size, in bytes                             |

**Read Template (`0x4C`)** with an offset and a byte count of `(attribute 4 × 4) − 23`. Loop while
the status is `0x06`, advancing the offset. The payload, concatenated:

1. `member count` × 8-byte member descriptors:

   | Field  | Type  | Content                                    |
   |--------|-------|--------------------------------------------|
   | info   | UINT  | Array length, or bit number for a `BOOL`   |
   | type   | UINT  | Same encoding as the symbol type above     |
   | offset | UDINT | Byte offset of the member in the structure |

2. The template name, null-terminated. Ignore everything after `;`.
3. The member names, each null-terminated, in descriptor order.

A member whose type has bit 15 set is itself a structure, and its `0x0FFF` bits name the child
template — another template read. Members named `ZZZZZZZZZZ…` or `__…` are the hidden host bytes
that back packed `BOOL`s ([§8](#8-bool-handling)).

### The browse flow

1. `0x55` on class `0x6B` until status `0x00` → the controller-scoped tags.
2. Collect the `Program:*` entries; repeat step 1 with the `Program:<name>` prefix for each.
3. For each tag with bit 15 set, read its template by the id in bits 11–0. Cache by id.
4. Recurse into members that are themselves structures.
5. Read and write tags; decode structures with the cached templates.

Micro800 has no program scope and browses tags only from firmware v10 onward — see
[§3](#3-what-micro800-exposes). How libplctag packages this flow behind its `@tags` and
`@udt/<id>` pseudo-tags, and the buffers it hands back, is in
[reading a UDT definition](../../libPlcTag/reading-a-udt-definition.md).

---

## 10. References

### Rockwell publications

- Rockwell Automation — *Logix 5000 Controllers Data Access* (1756-PM020) — tag type reporting, the
  Logix `STRING` structure, structure handles:
  <https://literature.rockwellautomation.com/idc/groups/literature/documents/pm/1756-pm020_-en-p.pdf>
- Rockwell Automation — *Type Encoding of Logix Structures in CIP Data Table Read/Write* — the `0xA0`
  abbreviated-structure marker, alignment and padding:
  <https://www.rockwellautomation.com/content/dam/rockwell-automation/sites/downloads/pdf/TypeEncode_CIPRW.pdf>
- Rockwell Automation — *Micro800 Programmable Controllers* user manuals — IEC types, UDFBs, CIP
  symbolic access:
  <https://literature.rockwellautomation.com/idc/groups/literature/documents/qs/2080-qs002_-en-e.pdf>

### Reference implementations

- libplctag — `list_tags_logix.c`, an end-to-end `0x55` listing and template walk, and the "Bits
  and Booleans" wiki page on BOOL-array packing:
  <https://github.com/libplctag/libplctag/blob/release/src/tools/list_tags_logix/list_tags_logix.c> ·
  <https://github.com/libplctag/libplctag/wiki/Bits-and-Booleans>
- pycomm3 — `LogixDriver` tag and type parsing, the symbol-type bitfield:
  <https://github.com/ottowayi/pycomm3/blob/master/pycomm3/cip/data_types.py>

### Related in-tree docs

- [`../cip/cip-datatypes-reference.md`](../cip/cip-datatypes-reference.md) — how each type is encoded
  on the wire
- [`pccc-data-file-types.md`](pccc-data-file-types.md) — the file-addressed legacy families
- [`controller-families-and-routing.md`](controller-families-and-routing.md) — which controller is
  which line, its programming tool, and its route path
- [`../cip/cip-networking-overview.md`](../cip/cip-networking-overview.md) — the standard wire stack,
  object model and services these extensions build on
- [`../../libPlcTag/reading-a-udt-definition.md`](../../libPlcTag/reading-a-udt-definition.md) — how
  libplctag packages the browse flow
