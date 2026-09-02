# Symbolic Tag Data Types — Logix and Micro800

Which data types exist on the controllers that address data by **symbolic tag name**, what they are
called, and how the controller reports them. **Client-agnostic** — this describes the controllers and
the wire, not how any specific library handles them.

How each type is *encoded* — type code, byte layout, range, .NET equivalent — is in the
[CIP Data Types Reference](cip-datatypes-reference.md), which this document does not repeat. The
counterpart for the file-addressed families is [PCCC Data-File Types](pccc-data-file-types.md).

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
elementary codes from the [reference](cip-datatypes-reference.md); `✅*️` marks a caveat spelled out
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
abbreviated-structure marker of [§7](#7-how-logix-reports-a-structure-on-the-wire) — never as
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

## 7. How Logix reports a structure on the wire

When a structured tag — a UDT, `STRING`, `TIMER`, … — is read, the reply data begins with the
**abbreviated-structure marker** (`0xA0`, one of the constructed type codes in the
[reference](cip-datatypes-reference.md#7-constructed-type-codes)) followed by a 2-byte
**structure handle**:

```text
A0 02        ← type marker: 0xA0 (ABBREV_STRUCT), 0x02 = a 2-byte handle follows
             (read as a little-endian u16 this is 0x02A0)
HH HH        ← 2-byte structure handle: a CRC of the template's type-encoding string
.. .. ..     ← packed member data (little-endian, with alignment pad bytes)
```

The handle identifies the template; a client matches it against the template definition, read from the
[Template object](cip-networking-overview.md#the-cip-object-model), class `0x6C`. The handle is **not**
unique across differently-ordered structs, so the template must be read to learn the actual member
layout.

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

### The Logix symbol-type bitfield

When tags are enumerated through the Symbol object (class `0x6B`), each tag carries a 16-bit
**symbol type** value. Its bit layout:

| Mask     | Bits  | Meaning                                                              |
|----------|-------|----------------------------------------------------------------------|
| `0x8000` | 15    | **1 = structure** (UDT / AOI / predefined struct); 0 = atomic        |
| `0x6000` | 14–13 | **array dimension count** (0–3) = `(type & 0x6000) >> 13`            |
| `0x1000` | 12    | **system / reserved tag** (predefined; typically filtered out)       |
| `0x0FFF` | 11–0  | when structured: the **template (UDT) instance id** (max 4096)       |
| `0x00FF` | 7–0   | when atomic: the **CIP elementary type code** (e.g. `0xC4` = DINT)   |
| `0x0700` | 10–8  | when atomic: **bit position** for a BOOL aliased to a bit of a word  |

This is exactly the decomposition the in-tree tag lister uses —
[`PlcTagLister.cs`](../../../tests/AllenBradley.Logix.Tests/Integration/TagListing/PlcTagLister.cs)
defines `TypeIsStruct = 0x8000`, `TypeIsSystem = 0x1000`, and `TypeUdtIdMask = 0x0FFF`. A structured
tag with type `0x8000 | templateId` is followed up with a template read (`@udt/<templateId>`); the UDT
id is `type & 0x0FFF`.

### The `@tags` listing entry

The symbol type above arrives inside a listing entry. Reading `@tags` returns the controller's symbol
table as entries packed back to back, each a fixed 22-byte header followed by the tag's name:

| Offset | Size | Field          | Notes                                                                                     |
|--------|------|----------------|-------------------------------------------------------------------------------------------|
| 0      | 4    | instance id    | The tag's instance in the Symbol object. A client that addresses tags by name never needs it. |
| 4      | 2    | symbol type    | The bitfield above: structure flag, array rank, atomic code *or* template id.              |
| 6      | 2    | element length | Bytes occupied by **one** element — 4 for a `DINT`, 88 for a `STRING`.                     |
| 8      | 12   | dimensions     | Three `UINT32`s. Only as many as the rank in the symbol type are meaningful.               |
| 20     | 2    | name length    | Bytes of name that follow.                                                                 |
| 22     | *n*  | name           | ASCII, not null-terminated.                                                                |

The entry says what the type *is*, never what it contains: an atomic tag is fully described by its type
code, and a structured one names only the template id. `STRING` is the ordinary case of that — the
entry reports a structure of 88 bytes, and `.LEN` and `.DATA` are known only from the template
(`@udt/<id>`, class `0x6C`) or from prior knowledge of the predefined layout.

Two properties of the listing follow from this and matter to any client:

- **Only top-level tags are listed.** A structure member (`Counter.PRE`, `MyString.LEN`) is readable by
  name but has no entry of its own, so a name-keyed lookup finds nothing for it.
- **Programs appear as entries** whose name begins `Program:`. They are how a client discovers program
  scopes, each of which is then listed separately via `Program:<name>.@tags`.

Micro800 has no program scope and browses tags only from firmware v10 onward — see
[§3](#3-what-micro800-exposes).

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

- libplctag — Logix symbol-type masks (`TYPE_IS_STRUCT 0x8000`, `TYPE_IS_SYSTEM 0x1000`,
  `TYPE_DIM_MASK 0x6000`, `TYPE_UDT_ID_MASK 0x0FFF`) and the "Bits and Booleans" wiki page on
  BOOL-array packing:
  <https://github.com/libplctag/libplctag/blob/release/src/examples/list_tags_logix.c> ·
  <https://github.com/libplctag/libplctag/wiki/Bits-and-Booleans>
- pycomm3 — `LogixDriver` tag and type parsing, the symbol-type bitfield:
  <https://github.com/ottowayi/pycomm3/blob/master/pycomm3/cip/data_types.py>

### Related in-tree docs

- [`cip-datatypes-reference.md`](cip-datatypes-reference.md) — how each type is encoded on the wire
- [`pccc-data-file-types.md`](pccc-data-file-types.md) — the file-addressed legacy families
- [`controller-families-and-routing.md`](controller-families-and-routing.md) — which controller is
  which line, its programming tool, and its route path
- [`cip-networking-overview.md`](cip-networking-overview.md) — wire stack, object model, tag services
