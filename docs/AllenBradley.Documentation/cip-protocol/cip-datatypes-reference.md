# CIP Data Types Reference

How CIP data types are **encoded**: type codes, wire format, byte layout, value ranges, and .NET
equivalents. **Client-agnostic** — this describes the protocol, not how any specific library handles
it.

This document deliberately says nothing about which controller has which type. That question is
answered per **addressing mode**, because the addressing mode decides the type vocabulary:

- [Symbolic Tag Data Types](symbolic-tag-data-types.md) — Logix and Micro800, which name tags and
  report a type code with the data.
- [PCCC Data-File Types](pccc-data-file-types.md) — MicroLogix, SLC 500, and PLC-5, where a file's type
  letter fixes the type and no code crosses the wire.

Both link back here for encoding rather than restating it. Note that CIP defines many more type codes
than any Allen-Bradley controller exposes as a tag type — a code listed below is not a promise that
some controller will report it.

**General notes:**

- CIP is **little-endian** (least-significant-byte-first) on the wire for all multi-byte values — the
  encapsulation header, the message-router fields, and all tag data. This is the opposite of big-endian
  protocols (Siemens S7comm, Modbus/TCP). Because .NET also runs little-endian, **scalar CIP values
  need no byte swapping** (see the [.NET mapping](#8-net--cts-mapping)).
- Every CIP type has a **type code**. A single byte in `0xC1`–`0xDE` is an **elementary** type; a
  descriptor starting `0xA0`–`0xA3` is a **constructed** one (see [§7](#7-constructed-type-codes)).
- On Logix, `STRING`, `TIMER`, `COUNTER`, and every UDT are constructed types, so the elementary
  `STRING` codes (`0xD0` / `0xDA`) below describe the generic CIP string forms and **not** a Logix
  `STRING` tag. Its structure is documented with
  [the symbolic types](symbolic-tag-data-types.md#5-the-logix-string-structure-not-elementary-0xd0).

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

- **Wire representation:** a single byte. Writers send `0xFF` for true and `0x00` for false; readers
  should treat **any nonzero byte** as true (do not test `== 0x01`).
- BOOL arrays and BOOL-in-UDT pack differently on Logix — see
  [BOOL handling](symbolic-tag-data-types.md#8-bool-handling).

---

## 2. Bit strings

Fixed-width registers with no arithmetic interpretation — used for bit masks and status flags. They
share their wire layout with the unsigned integers of the same width but are distinct CIP type names.

| CIP type | Code   | Size | Encoding             | Range                      | C#-alias | CTS-name        |
|----------|--------|------|----------------------|----------------------------|----------|-----------------|
| `BYTE`   | `0xD1` | 1    | 8-bit bit string     | `0x00`..`0xFF`             | `byte`   | `System.Byte`   |
| `WORD`   | `0xD2` | 2    | 16-bit bit string    | `0x0000`..`0xFFFF`         | `ushort` | `System.UInt16` |
| `DWORD`  | `0xD3` | 4    | 32-bit bit string    | `0x00000000`..`0xFFFFFFFF` | `uint`   | `System.UInt32` |
| `LWORD`  | `0xD4` | 8    | 64-bit bit string    | `0`..`2^64-1`              | `ulong`  | `System.UInt64` |

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

---

## 4. Floating point (IEEE 754)

| CIP type | Code   | Size | Encoding                       | Layout                     | Bits                          | Range                    | C#-alias | CTS-name        |
|----------|--------|------|--------------------------------|----------------------------|-------------------------------|--------------------------|----------|-----------------|
| `REAL`   | `0xCA` | 4    | IEEE 754 single, little-endian | byte 0 = LSB, byte 3 = MSB | 1 sign + 8 exp + 23 mantissa  | ±1.18e-38 .. ±3.4e+38    | `float`  | `System.Single` |
| `LREAL`  | `0xCB` | 8    | IEEE 754 double, little-endian | byte 0 = LSB, byte 7 = MSB | 1 sign + 11 exp + 52 mantissa | ±2.23e-308 .. ±1.79e+308 | `double` | `System.Double` |

---

## 5. Date / time / duration

CIP defines several temporal types. Each is built on an integer base type; the table gives the **code,
size, and base type**. The exact *unit* and *epoch* of some of these are specified in ODVA CIP Vol. 1
Appendix C and are noted below where not independently confirmed here.

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

For exact unit and epoch semantics consult ODVA CIP Vol. 1 Appendix C, which is the arbiter.

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

- The CIP `STRING` (`0xD0`) length prefix is **2 bytes** — do not confuse it with `SHORT_STRING`
  (`0xDA`), whose prefix is 1 byte.
- `STRINGI` (`0xDE`) is defined by ODVA but not implemented by every stack (OpENer, for instance, does
  not assign it).
- Neither form is how a controller in this repo stores a string. A Logix `STRING` is a
  [structure](symbolic-tag-data-types.md#5-the-logix-string-structure-not-elementary-0xd0); a legacy
  `ST` file element is [42 words with swapped character pairs](pccc-data-file-types.md#string-elements).

---

## 7. Constructed type codes

Per the CIP rule that a data-type descriptor starting `0xA0`–`0xA3` is *structured* while a single byte
`0xC1`–`0xDE` is *elementary*:

| Code   | Name           | Meaning                                                            |
|--------|----------------|--------------------------------------------------------------------|
| `0xA0` | ABBREV_STRUCT  | Abbreviated structure — a 2-byte handle stands in for the template |
| `0xA1` | ABBREV_ARRAY   | Abbreviated array                                                  |
| `0xA2` | STRUCT         | Structure with an explicit member type list                        |
| `0xA3` | ARRAY          | Array                                                              |

`0xA0` is the one an Allen-Bradley client meets constantly: every structured Logix tag read begins with
it, followed by the template handle. That exchange, the handle's meaning, and the template read that
resolves it are documented with
[the symbolic types](symbolic-tag-data-types.md#7-how-logix-reports-a-structure-on-the-wire).

---

## 8. .NET / CTS mapping

CIP is little-endian and .NET runs little-endian, so **scalar values need no byte swapping** — a direct
`BitConverter` / `MemoryMarshal` read of the raw payload yields the correct value. (This is the
opposite of big-endian PLC protocols, where every multi-byte value must be reversed.)

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

## 9. Byte-offset examples

Concrete wire bytes for the common types. All multi-byte values are **little-endian** (byte 0 = LSB);
this is the key difference from big-endian PLC protocols.

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

A structured example — the 88-byte Logix `STRING` read reply — is with
[the symbolic types](symbolic-tag-data-types.md#5-the-logix-string-structure-not-elementary-0xd0),
because its layout is a controller fact rather than a CIP encoding rule.

---

## 10. References

### ODVA specifications

- ODVA — *The CIP Networks Library*, Volume 1, **Appendix C "Data Management"** (elementary type codes
  `0xC1`–`0xDE`, constructed codes `0xA0`–`0xA3`, temporal type base types and epochs, the
  structure-handle CRC):
  <https://www.odva.org/technology-standards/key-technologies/common-industrial-protocol-cip/>

### Rockwell publications

- Rockwell Automation — *Logix 5000 Controllers Data Access* (1756-PM020) — tag type reporting and the
  Logix structures:
  <https://literature.rockwellautomation.com/idc/groups/literature/documents/pm/1756-pm020_-en-p.pdf>
- Rockwell Automation — *Type Encoding of Logix Structures in CIP Data Table Read/Write* (the `0xA0`
  abbreviated-structure marker, alignment/padding, little-endian ordering):
  <https://www.rockwellautomation.com/content/dam/rockwell-automation/sites/downloads/pdf/TypeEncode_CIPRW.pdf>

### Reference implementations

- OpENer (`ciptypes.h`) — CIP type-code constants (`kCipBool` `0xC1` … `kCipEngUnit` `0xDD`):
  <https://github.com/EIPStackGroup/OpENer/blob/master/source/src/cip/ciptypes.h>
- pycomm3 (`cip/data_types.py`) — type codes, string length-prefix logic, BOOL `0xFF`/`0x00`:
  <https://github.com/ottowayi/pycomm3/blob/master/pycomm3/cip/data_types.py>
- Wireshark CIP dissector (`packet-cip.h`) — string type codes `0xD0`/`0xD5`/`0xD9`/`0xDA`:
  <https://www.wireshark.org/docs/wsar_html/packet-cip_8h_source.html>

### Related in-tree docs

- [`symbolic-tag-data-types.md`](symbolic-tag-data-types.md) — which of these types Logix and Micro800
  expose, plus structures, arrays, and the symbol table
- [`pccc-data-file-types.md`](pccc-data-file-types.md) — the file-addressed legacy families
- [`cip-networking-overview.md`](cip-networking-overview.md) — wire stack, object model, tag services
