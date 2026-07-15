# CIP Data Types Reference

Complete reference for CIP data types as used by Allen-Bradley controllers over EtherNet/IP:
type codes, wire format, encoding, value ranges, and .NET equivalents. **Client-agnostic** —
describes the protocol, not how any specific library handles it.

**General notes:**

- CIP is **little-endian** (least-significant-byte-first) on the wire for all multi-byte values
  — the encapsulation header, the message-router fields, and all tag data. This is the opposite
  of big-endian protocols (Siemens S7comm, Modbus/TCP). Because .NET also runs little-endian,
  **scalar CIP values need no byte swapping** (see the [.NET mapping](#9-net--cts-mapping)).
- Every CIP type has a **type code** (mostly one byte, `0xC1`–`0xDE` for elementary types)
  reported alongside the data when a tag is read. The per-type tables below list the type code,
  wire size, encoding, and range.
- **Which types exist on which controller family** — plus the file-based type mapping for legacy
  PLCs — lives in the companion [CIP Data Type Support Matrix](cip-datatype-support-matrix.md).
  This document is the source of truth for *how each type is encoded*; the matrix answers *which
  controllers expose it*.
- On **Logix** controllers, `STRING`, `TIMER`, `COUNTER`, and every UDT are **structures**, not
  elementary types — they are reported with a structure marker and a template handle (see
  [§7](#7-aggregate--structured-types)). The elementary `STRING` codes (`0xD0` / `0xDA`) below
  describe the generic CIP string forms, which is not how a Logix `STRING` *tag* is encoded.

---

## Table columns

- **CIP type** — the CIP / IEC type keyword.
- **Code** — the one-byte CIP type code reported on the wire.
- **Size** — wire byte count (`var` = variable-length).
- **Encoding** — how the bytes are interpreted (two's complement, IEEE 754, bit string, …).
- **Layout** — per-byte order (little-endian: `byte 0 = LSB, byte N = MSB`); `—` when single-byte.
- **Range** — value range on the wire (omitted where not meaningful).
- **C#-alias** — the C# keyword alias.
- **CTS-name** — the fully qualified .NET Common Type System name (`System.<Name>`).

---

## 1. Bool

| CIP type | Code   | Size | Encoding                 | Range     | C#-alias | CTS-name         |
|----------|--------|------|--------------------------|-----------|----------|------------------|
| `BOOL`   | `0xC1` | 1    | 1 byte: nonzero = true   | `0`/`1`   | `bool`   | `System.Boolean` |

- **Wire representation:** a single byte. Writers send `0xFF` for true and `0x00` for false;
  readers should treat **any nonzero byte** as true (do not test `== 0x01`).
- **BOOL arrays and BOOL-in-UDT** pack differently — see [§8](#8-bool-handling-on-logix).

---

## 2. Bit strings

Fixed-width registers with no arithmetic interpretation — used for bit masks and status flags.
They share their wire layout with the unsigned integers of the same width but are distinct CIP
type names.

| CIP type | Code   | Size | Encoding             | Range                      | C#-alias | CTS-name        |
|----------|--------|------|----------------------|----------------------------|----------|-----------------|
| `BYTE`   | `0xD1` | 1    | 8-bit bit string     | `0x00`..`0xFF`             | `byte`   | `System.Byte`   |
| `WORD`   | `0xD2` | 2    | 16-bit bit string    | `0x0000`..`0xFFFF`         | `ushort` | `System.UInt16` |
| `DWORD`  | `0xD3` | 4    | 32-bit bit string    | `0x00000000`..`0xFFFFFFFF` | `uint`   | `System.UInt32` |
| `LWORD`  | `0xD4` | 8    | 64-bit bit string    | `0`..`2^64-1`              | `ulong`  | `System.UInt64` |

> **Not a Logix tag type.** Studio 5000 / Logix Designer does not expose `BYTE`/`WORD`/`DWORD`/
> `LWORD` as declarable atomic tag types — Logix uses `SINT`/`INT`/`DINT`/`LINT` (displayed as
> hex/binary where a bit string is wanted). These codes still appear on the wire for structure
> members and for controllers/objects that use them.

---

## 3. Signed / unsigned integers

| CIP type | Code   | Size | Encoding                | Layout                      | Range             | C#-alias | CTS-name        |
|----------|--------|------|-------------------------|-----------------------------|-------------------|----------|-----------------|
| `SINT`   | `0xC2` | 1    | Two's complement        | —                           | `-128`..`127`     | `sbyte`  | `System.SByte`  |
| `USINT`  | `0xC6` | 1    | Unsigned                | —                           | `0`..`255`        | `byte`   | `System.Byte`   |
| `INT`    | `0xC3` | 2    | Little-endian two's complement | byte 0 = LSB, byte 1 = MSB | `-32768`..`32767` | `short`  | `System.Int16`  |
| `UINT`   | `0xC7` | 2    | Little-endian unsigned  | byte 0 = LSB, byte 1 = MSB  | `0`..`65535`      | `ushort` | `System.UInt16` |
| `DINT`   | `0xC4` | 4    | Little-endian two's complement | byte 0 = LSB, byte 3 = MSB | `-2^31`..`2^31-1` | `int`    | `System.Int32`  |
| `UDINT`  | `0xC8` | 4    | Little-endian unsigned  | byte 0 = LSB, byte 3 = MSB  | `0`..`2^32-1`     | `uint`   | `System.UInt32` |
| `LINT`   | `0xC5` | 8    | Little-endian two's complement | byte 0 = LSB, byte 7 = MSB | `-2^63`..`2^63-1` | `long`   | `System.Int64`  |
| `ULINT`  | `0xC9` | 8    | Little-endian unsigned  | byte 0 = LSB, byte 7 = MSB  | `0`..`2^64-1`     | `ulong`  | `System.UInt64` |

The unsigned types (`USINT`/`UINT`/`UDINT`/`ULINT`) are available as tag types only on newer
Logix controllers and on Micro800 — see the [support matrix](cip-datatype-support-matrix.md).

---

## 4. Floating point (IEEE 754)

| CIP type | Code   | Size | Encoding                       | Layout                     | Bits                          | Range                    | C#-alias | CTS-name        |
|----------|--------|------|--------------------------------|----------------------------|-------------------------------|--------------------------|----------|-----------------|
| `REAL`   | `0xCA` | 4    | IEEE 754 single, little-endian | byte 0 = LSB, byte 3 = MSB | 1 sign + 8 exp + 23 mantissa  | ±1.18e-38 .. ±3.4e+38    | `float`  | `System.Single` |
| `LREAL`  | `0xCB` | 8    | IEEE 754 double, little-endian | byte 0 = LSB, byte 7 = MSB | 1 sign + 11 exp + 52 mantissa | ±2.23e-308 .. ±1.79e+308 | `double` | `System.Double` |

---

## 5. Date / time / duration

CIP defines several temporal types. Each is built on an integer base type; the table gives the
**code, size, and base type**. The exact *unit* and *epoch* of some of these are specified in
ODVA CIP Vol. 1 Appendix C and are noted below where not independently confirmed here.

| CIP type        | Code   | Size | Base type / encoding    | Notes                                                      | CTS-name          |
|-----------------|--------|------|-------------------------|------------------------------------------------------------|-------------------|
| `ITIME`         | `0xD8` | 2    | `INT` (signed)          | Short duration. Unit per ODVA App. C (commonly ms).        | `System.TimeSpan` |
| `TIME`          | `0xDB` | 4    | `DINT` (signed)         | Duration in **milliseconds**.                              | `System.TimeSpan` |
| `FTIME`         | `0xD6` | 4    | `DINT` (signed)         | High-resolution duration. Unit per App. C (commonly µs).   | `System.TimeSpan` |
| `STIME`         | `0xCC` | 4    | `DINT` (signed)         | "Synchronous time".                                        | `System.TimeSpan` |
| `LTIME`         | `0xD7` | 8    | `LINT` (signed)         | Long duration. Unit per App. C (commonly µs/ns).           | `System.TimeSpan` |
| `DATE`          | `0xCD` | 2    | `UINT` (days)           | Days since an epoch defined in App. C.                     | `System.DateTime` |
| `TIME_OF_DAY`   | `0xCE` | 4    | `UDINT` (since midnight)| Unit per App. C.                                           | `System.TimeSpan` |
| `DATE_AND_TIME` | `0xCF` | 8    | struct: `UDINT` + `UINT`| Time-of-day value + date value (with padding).             | `System.DateTime` |

> **Rarely seen on Logix.** Studio 5000 does not expose these elementary temporal types as tag
> types. Logix keeps wall-clock time as a `LINT` (microseconds since the Unix epoch, via the
> `WALLCLOCKTIME` object / `GSV`) and durations inside the predefined `TIMER` structure (see
> [§7](#7-aggregate--structured-types)). Treat this section as the CIP protocol definition; for
> exact unit/epoch semantics consult ODVA CIP Vol. 1 Appendix C, which is the arbiter.

---

## 6. Character / string

CIP defines several string forms that differ in their length prefix and per-character width:

| CIP type       | Code   | Size | Length prefix        | Per char | Encoding      | Wire layout                                  |
|----------------|--------|------|----------------------|----------|---------------|----------------------------------------------|
| `STRING`       | `0xD0` | var  | 2-byte `UINT`        | 1 byte   | ASCII/Latin-1 | `[len:u16][chars…]`                           |
| `SHORT_STRING` | `0xDA` | var  | 1-byte `USINT`       | 1 byte   | ASCII/Latin-1 | `[len:u8][chars…]`                            |
| `STRING2`      | `0xD5` | var  | 2-byte `UINT` (count)| 2 bytes  | UTF-16-LE     | `[count:u16][char pairs…]`                    |
| `STRINGN`      | `0xD9` | var  | `UINT` size + `UINT` count | 1/2/4 | per size    | `[char_size:u16][count:u16][data…]`           |
| `STRINGI`      | `0xDE` | var  | (international)       | varies   | multi-language| count of sub-strings, each with language + charset + data |

- The CIP `STRING` (`0xD0`) length prefix is **2 bytes** — do not confuse it with
  `SHORT_STRING` (`0xDA`), whose prefix is 1 byte.
- `STRINGI` (`0xDE`) is defined by ODVA but not implemented by every stack (OpENer, for
  instance, does not assign it).

### The Logix `STRING` structure (not elementary `0xD0`)

On a Logix controller, the built-in `STRING` tag is a **predefined structure**, reported with
the structure marker of [§7](#7-aggregate--structured-types) — not as elementary `0xD0`. Its
template:

| Member  | Type      | Bytes | Notes                                    |
|---------|-----------|-------|------------------------------------------|
| `.LEN`  | `DINT`    | 4     | Current character count (little-endian). It is a **`DINT`**, not an `INT`. |
| `.DATA` | `SINT[82]`| 82    | ASCII bytes. `MaxLen` = **82**.          |
| (padding) | —       | 2     | Alignment pad so the struct ends on a 32-bit boundary. |
| **Total** |         | **88**| 86 data bytes (4 + 82) padded to 88.     |

Custom string types (e.g. `STRING_20`, `STRING_100`) are user-defined structures with the same
`.LEN` + `.DATA[n]` shape and a different `n`.

---

## 7. Aggregate / structured types

### ODVA constructed type codes

Per the CIP rule that a data-type descriptor starting `0xA0`–`0xA3` is *structured* while a
single byte `0xC1`–`0xDE` is *elementary*:

| Code   | Name           | Meaning                                                            |
|--------|----------------|-------------------------------------------------------------------|
| `0xA0` | ABBREV_STRUCT  | Abbreviated structure — a 2-byte handle stands in for the template |
| `0xA1` | ABBREV_ARRAY   | Abbreviated array                                                 |
| `0xA2` | STRUCT         | Structure with an explicit member type list                      |
| `0xA3` | ARRAY          | Array                                                             |

### How Logix reports a structure on the wire

When a structured tag (a UDT, `STRING`, `TIMER`, …) is read, the reply data begins with the
**abbreviated-structure marker** followed by a 2-byte **structure handle**:

```text
A0 02        ← type marker: 0xA0 (ABBREV_STRUCT), 0x02 = a 2-byte handle follows
             (read as a little-endian u16 this is 0x02A0)
HH HH        ← 2-byte structure handle: a CRC of the template's type-encoding string
.. .. ..     ← packed member data (little-endian, with alignment pad bytes)
```

The handle identifies the template; a client matches it against the template definition (read
from the [Template object](cip-networking-overview.md#the-cip-object-model), class `0x6C`) to
decode the members. The handle is **not** unique across differently-ordered structs, so the
template must be read to learn the actual member layout.

### The Logix symbol-type bitfield

When tags are enumerated (via the Symbol object, class `0x6B`), each tag carries a 16-bit
**symbol type** value. Its bit layout:

| Mask     | Bits  | Meaning                                                              |
|----------|-------|---------------------------------------------------------------------|
| `0x8000` | 15    | **1 = structure** (UDT / AOI / predefined struct); 0 = atomic       |
| `0x6000` | 14–13 | **array dimension count** (0–3) = `(type & 0x6000) >> 13`           |
| `0x1000` | 12    | **system / reserved tag** (predefined; typically filtered out)      |
| `0x0FFF` | 11–0  | when structured: the **template (UDT) instance id** (max 4096)      |
| `0x00FF` | 7–0   | when atomic: the **CIP elementary type code** (e.g. `0xC4` = DINT)  |
| `0x0700` | 10–8  | when atomic: **bit position** for a BOOL aliased to a bit of a word |

This is exactly the decomposition the in-tree tag lister uses —
[`PlcTagLister.cs`](../../../tests/AllenBradley.Logix.Tests/Integration/TagListing/PlcTagLister.cs) defines
`TypeIsStruct = 0x8000`, `TypeIsSystem = 0x1000`, and `TypeUdtIdMask = 0x0FFF`. A structured tag
with type `0x8000 | templateId` is followed up with a template read (`@udt/<templateId>`); the
UDT id is `type & 0x0FFF`.

### Predefined structures (TIMER / COUNTER / CONTROL)

Each is a 12-byte structure: a hidden 32-bit status/control `DINT` (status booleans in its high
bits) followed by two `DINT`s.

| Structure | Bytes | Members                                                                             |
|-----------|-------|-------------------------------------------------------------------------------------|
| `TIMER`   | 12    | status `DINT` (`.EN` bit 31, `.TT` bit 30, `.DN` bit 29), `.PRE` `DINT` (ms), `.ACC` `DINT` (ms) |
| `COUNTER` | 12    | status `DINT` (`.CU` 31, `.CD` 30, `.DN` 29, `.OV` 28, `.UN` 27), `.PRE` `DINT`, `.ACC` `DINT` |
| `CONTROL` | 12    | status `DINT` (`.EN`, `.EU`, `.DN`, `.EM`, `.ER`, `.UL`, `.IN`, `.FD` in high bits), `.LEN` `DINT`, `.POS` `DINT` |

### Arrays

A CIP/Logix array stores its elements **contiguously**, each in the element type's wire layout,
little-endian, with no padding between elements (except the special BOOL-array packing in
[§8](#8-bool-handling-on-logix)). The array element is addressed by a member/element segment in
the request path (`Arr[5]` → symbol segment for `Arr` + member segment `28 05`). Logix supports
up to **3 dimensions** (encoded in the `0x6000` bits of the symbol type).

---

## 8. BOOL handling on Logix

BOOL is the one type whose storage differs by context:

- **Atomic `BOOL` tag** — 1 byte on the wire (`0x00` / `0xFF`; nonzero = true).
- **`BOOL` array** — packed into **32-bit words**. A `BOOL[]` in ControlLogix/CompactLogix is
  stored and transferred as an array of 32-bit words, and its declared length must be a multiple
  of 32. Consequences:
  - The array index addresses the **word, not the bit**: `bits[3]` selects the 4th 32-bit word
    (bits 96–127). A single-element read returns a whole 32-bit word; extract the bit yourself.
  - Writes are word-granular — changing one bit rewrites the whole 32-bit word, so keep input
    and output BOOL arrays separate to avoid clobbering neighbours.
- **`BOOL` inside a UDT** — packed into a hidden backing field: up to 8 BOOLs share a hidden
  `SINT` byte; more than 8 pack into a `BOOL[32/64]` word array with 32-bit alignment. The
  template read gives the host byte offset and the bit position within it.

Whether a client models the backing word as `DWORD` (`0xD3`) or `DINT` (`0xC4`) is a modeling
choice; the load-bearing wire fact is the **32-bit packing (length a multiple of 32)**.

---

## 9. .NET / CTS mapping

CIP is little-endian and .NET runs little-endian, so **scalar values need no byte swapping** —
a direct `BitConverter` / `MemoryMarshal` read of the raw payload yields the correct value.
(This is the opposite of big-endian PLC protocols, where every multi-byte value must be
reversed.)

| CIP type                              | C#-alias | CTS-name         | Conversion note                                  |
|---------------------------------------|----------|------------------|--------------------------------------------------|
| `BOOL`                                | `bool`   | `System.Boolean` | read 1 byte; nonzero = true                       |
| `SINT`                                | `sbyte`  | `System.SByte`   | direct                                            |
| `INT`                                 | `short`  | `System.Int16`   | direct (no swap)                                  |
| `DINT`                                | `int`    | `System.Int32`   | direct (no swap)                                  |
| `LINT`                                | `long`   | `System.Int64`   | direct (no swap)                                  |
| `USINT` / `BYTE`                      | `byte`   | `System.Byte`    | direct                                            |
| `UINT` / `WORD`                       | `ushort` | `System.UInt16`  | direct                                            |
| `UDINT` / `DWORD`                     | `uint`   | `System.UInt32`  | direct                                            |
| `ULINT` / `LWORD`                     | `ulong`  | `System.UInt64`  | direct                                            |
| `REAL`                                | `float`  | `System.Single`  | `BitConverter.ToSingle` directly                  |
| `LREAL`                               | `double` | `System.Double`  | `BitConverter.ToDouble` directly                  |
| `STRING` / `SHORT_STRING` / Logix `STRING` | `string` | `System.String` | decode with the length prefix + ASCII/Latin-1 |
| `STRING2`                             | `string` | `System.String`  | UTF-16-LE                                         |
| `TIME` / `ITIME` / `FTIME` / `LTIME`  | —        | `System.TimeSpan`| scale by the type's time unit (App. C)            |
| `DATE` / `DATE_AND_TIME`              | —        | `System.DateTime`| epoch/units per App. C                            |

---

## 10. Byte-offset examples

Concrete wire bytes for the common types. All multi-byte values are **little-endian**
(byte 0 = LSB); this is the key difference from big-endian PLC protocols.

### `INT` — 16-bit signed

```text
INT = 4660  (= 0x1234)
Offset  Hex   Dec   Meaning
------  ----  ----  ----------------------------
0x00    34     52   low byte  (LSB)
0x01    12     18   high byte (MSB)

INT = -1  (= 0xFFFF, two's complement)
Offset  Hex   Dec   Meaning
------  ----  ----  ----------------------------
0x00    FF    255   low byte
0x01    FF    255   high byte
```

### `DINT` — 32-bit signed

```text
DINT = 1000  (= 0x000003E8)
Offset  Hex   Dec   Meaning
------  ----  ----  ----------------------------
0x00    E8    232   byte 0 (LSB)
0x01    03      3   byte 1
0x02    00      0   byte 2
0x03    00      0   byte 3 (MSB)
```

### `REAL` — IEEE 754 single

```text
REAL = 1.0  (= 0x3F800000)
Offset  Hex   Dec   Meaning
------  ----  ----  ----------------------------
0x00    00      0   byte 0 (LSB, low mantissa)
0x01    00      0   byte 1
0x02    80    128   byte 2
0x03    3F     63   byte 3 (MSB, sign + high exponent)

REAL = -1.5  (= 0xBFC00000)
Offset  Hex   Dec   Meaning
------  ----  ----  ----------------------------
0x00    00      0   byte 0 (LSB)
0x01    00      0   byte 1
0x02    C0    192   byte 2
0x03    BF    191   byte 3 (MSB)
```

### `LREAL` — IEEE 754 double

```text
LREAL = 1.0  (= 0x3FF0000000000000)
Offset  Hex   Meaning
------  ----  ----------------------------
0x00    00    byte 0 (LSB)
0x01    00    byte 1
0x02    00    byte 2
0x03    00    byte 3
0x04    00    byte 4
0x05    00    byte 5
0x06    F0    byte 6
0x07    3F    byte 7 (MSB)
```

### Logix `STRING` — structured tag read reply

```text
Logix STRING tag containing "Hi"
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

Readers only need to consume `.LEN` characters; the remaining `.DATA` bytes are unused. The
`A0 02` + handle prefix appears only in the tag **read reply**, not in the stored structure.

---

## 11. References

### ODVA specifications

- ODVA — *The CIP Networks Library*, Volume 1, **Appendix C "Data Management"** (elementary type
  codes `0xC1`–`0xDE`, constructed codes `0xA0`–`0xA3`, temporal type base types and epochs, the
  structure-handle CRC): <https://www.odva.org/technology-standards/key-technologies/common-industrial-protocol-cip/>

### Rockwell publications

- Rockwell Automation — *Logix 5000 Controllers Data Access* (1756-PM020) — tag type reporting,
  the Logix `STRING` structure, structure handles:
  <https://literature.rockwellautomation.com/idc/groups/literature/documents/pm/1756-pm020_-en-p.pdf>
- Rockwell Automation — *Type Encoding of Logix Structures in CIP Data Table Read/Write* (the
  `0xA0` abbreviated-structure marker, alignment/padding, little-endian ordering):
  <https://www.rockwellautomation.com/content/dam/rockwell-automation/sites/downloads/pdf/TypeEncode_CIPRW.pdf>

### Reference implementations

- OpENer (`ciptypes.h`) — CIP type-code constants (`kCipBool` `0xC1` … `kCipEngUnit` `0xDD`):
  <https://github.com/EIPStackGroup/OpENer/blob/master/source/src/cip/ciptypes.h>
- pycomm3 (`cip/data_types.py`) — type codes, string length-prefix logic, BOOL `0xFF`/`0x00`,
  the symbol-type bitfield: <https://github.com/ottowayi/pycomm3/blob/master/pycomm3/cip/data_types.py>
- Wireshark CIP dissector (`packet-cip.h`) — string type codes `0xD0`/`0xD5`/`0xD9`/`0xDA`:
  <https://www.wireshark.org/docs/wsar_html/packet-cip_8h_source.html>
- libplctag (`list_tags_logix.c`) — Logix symbol-type masks (`TYPE_IS_STRUCT 0x8000`,
  `TYPE_IS_SYSTEM 0x1000`, `TYPE_DIM_MASK 0x6000`, `TYPE_UDT_ID_MASK 0x0FFF`) and the "Bits and
  Booleans" wiki (BOOL-array 32-bit packing):
  <https://github.com/libplctag/libplctag/blob/release/src/examples/list_tags_logix.c> ·
  <https://github.com/libplctag/libplctag/wiki/Bits-and-Booleans>

### Related in-tree docs

- [`cip-datatype-support-matrix.md`](cip-datatype-support-matrix.md) — which types exist per controller family
- [`cip-networking-overview.md`](cip-networking-overview.md) — wire stack, object model, tag services
- [`../../../tests/AllenBradley.Logix.Tests/Integration/TagListing/PlcTagLister.cs`](../../../tests/AllenBradley.Logix.Tests/Integration/TagListing/PlcTagLister.cs) — in-tree decoder that uses the symbol-type masks in [§7](#7-aggregate--structured-types)
