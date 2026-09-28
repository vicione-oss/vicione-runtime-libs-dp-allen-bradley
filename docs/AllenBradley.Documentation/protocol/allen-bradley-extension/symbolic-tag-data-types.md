# Symbolic Tag Data Types: Logix and Micro800

Which data types exist on the controllers that address data by symbolic tag name, what they are
called, and how the structured ones are laid out. This document is client-agnostic. It describes the
controllers, not how any specific library handles them.

Everything here is a Rockwell extension of CIP, specified in *Logix 5000 Controllers Data Access*
(1756-PM020), not by ODVA. How each type is encoded (type code, byte layout, range, .NET equivalent)
is in the [CIP data types](../cip/data-types.md) reference, which this document does not repeat.
How a tag's value travels, meaning the read and write services and the type prefix on a reply, is in
[tag services](tag-services.md). How a client learns which tags exist is in
[tag browsing](tag-browsing.md). The counterpart for the file-addressed families is
[PCCC data-file types](pccc-data-file-types.md).

Who addresses symbolically: ControlLogix, CompactLogix, GuardLogix and SoftLogix, programmed in
Studio 5000 Logix Designer, and Micro800, programmed in Connected Components Workbench. The families
and their tooling are listed in
[controller families](../../controllers/controller-families.md#the-lines).

---

## What symbolic addressing does to the type question

CIP tag access is typed and symbolic. You read and write a whole named tag whose type the controller
already knows, and the reply carries the type ahead of the data. There is no equivalent of parking
an arbitrary byte layout at an arbitrary address, the way Siemens S7comm PUT/GET reads a byte range
out of a DB.

The consequence for this document is that the type vocabulary is decided by the programming tool.
What Studio 5000 or CCW lets you declare is what exists in the symbol table, and what appears on the
wire. So the question "which types does this controller have" is answered per engineering
environment, not per protocol. CIP itself defines many more codes than any Allen-Bradley controller
will ever report for a tag.

---

## What Logix exposes

### Atomic types, in two generations

The Logix line splits by [generation](../../controllers/logix-generations.md), and the generation
decides the atomic vocabulary.

The 5X70 and earlier controllers (ControlLogix 5550/5555/5560/5570, CompactLogix 1769/5370) have
`BOOL`, `SINT`, `INT`, `DINT`, `LINT` and `REAL`. No unsigned integers, no `LREAL`.

The 5X80 controllers (ControlLogix 5580, CompactLogix 5380 and 5480) add the extended data types:
`USINT`, `UINT`, `UDINT`, `ULINT`, and `LREAL`. They need a recent controller firmware and a
matching Studio 5000 version. For a given catalog number and revision the tool is the arbiter.

### What Logix deliberately does not have

`BYTE`, `WORD`, `DWORD` and `LWORD` are not declarable tag types. Where a bit string is wanted,
Logix uses `SINT`/`INT`/`DINT`/`LINT` with a hex or binary display style. The codes still cross the
wire, as members of structures and from CIP objects other than the Symbol object.

The CIP temporal types (`DATE`, `TIME`, `ITIME`, `FTIME`, `LTIME`, `TIME_OF_DAY`, `DATE_AND_TIME`)
are not tag types either. Logix keeps wall-clock time as a `LINT` holding microseconds since the
Unix epoch, read through the `WALLCLOCKTIME` object with `GSV`, and keeps durations in the `TIMER`
structure.

The elementary CIP strings `0xD0` and `0xDA` never describe a Logix `STRING` tag. That type is a
structure; see [the Logix `STRING` structure](#the-logix-string-structure).

Everything that is not atomic is a structure: `STRING`, `TIMER`, `COUNTER`, `CONTROL`, the motion
`AXIS_*` types, `MSG`, every UDT, and every Add-On Instruction.

---

## What Micro800 exposes

Micro800 is symbolic like Logix but shares neither its type list nor its structure vocabulary. CCW
implements the IEC 61131-3 elementary type set, which is the widest atomic vocabulary of any
Allen-Bradley family here: the bit strings `BYTE`/`WORD`/`DWORD`/`LWORD`, the signed and unsigned
integers, `REAL` and `LREAL`, `STRING`, and `TIME`/`DATE`.

The differences go beyond the type list. There are no AOIs; the equivalent is a UDFB, a user-defined
function block, and UDTs are supported. Timers and counters are IEC function-block instances (`TON`,
`TOF`, `CTU`, and so on) rather than the Logix `TIMER`/`COUNTER`/`CONTROL` predefined structures, so
none of [predefined structures](#predefined-structures-timer--counter--control) applies. CIP access
is restricted: Micro800 addresses tags symbolically only, without the firmware-v21 Symbol Instance
Addressing, does not support the Multiple Service Packet service, and could not browse tags at all
before roughly firmware v10. It tolerates large single packets but depends on fragmented services
for them. Whether the 64-bit and bit-string types (`LWORD`, `LINT`, `ULINT`) exist depends on model
and firmware. Embedded Ethernet is on Micro820/850/870. The Micro810 and Micro830 are serial/USB and
reach EtherNet/IP only through a converter.

---

## Availability matrix

Native support, meaning what the programming tool exposes as a first-class tag type. Codes are the
CIP elementary codes from the [reference](../cip/data-types.md). `✅*️` marks a caveat spelled out
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
| `ARRAY`                     | n/a    | ✅ (≤3 dims)          | ✅ (≤3 dims)| ✅        |
| `TIMER` / `COUNTER` / `CONTROL` | struct | ✅                | ✅          | ✅*️      |
| AOI                         | struct | ✅                    | ✅          | ✅*️      |

The caveats: a Logix `STRING` is a predefined structure, not the elementary CIP `STRING`, see
[the Logix `STRING` structure](#the-logix-string-structure). Micro800's 64-bit and bit-string types
depend on firmware and model. Micro800's timers and counters are IEC function blocks, and its AOI
equivalent is the UDFB, see [what Micro800 exposes](#what-micro800-exposes).

---

## The Logix `STRING` structure

On a Logix controller the built-in `STRING` tag is a predefined structure, reported with the
abbreviated-structure marker described under
[the structure reply](tag-services.md#the-structure-reply) and never as elementary `0xD0`. Its
template:

| Member    | Type       | Bytes  | Notes                                                                    |
|-----------|------------|--------|--------------------------------------------------------------------------|
| `.LEN`    | `DINT`     | 4      | Current character count (little-endian). It is a **`DINT`**, not an `INT`. |
| `.DATA`   | `SINT[82]` | 82     | ASCII bytes. `MaxLen` = **82**.                                          |
| (padding) | n/a        | 2      | Alignment pad so the struct ends on a 32-bit boundary.                   |
| **Total** |            | **88** | 86 data bytes (4 + 82) padded to 88.                                     |

Custom string types (`STRING_20`, `STRING_100`, and so on) are user-defined structures with the
same `.LEN` + `.DATA[n]` shape and a different `n`. The wire size follows the declared capacity, so
the type name alone does not fix the byte count.

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

Readers only need to consume `.LEN` characters. The remaining `.DATA` bytes are unused. The `A0 02`
prefix and handle appear only in the read reply, not in the stored structure.

---

## Predefined structures (TIMER / COUNTER / CONTROL)

Each is a 12-byte structure: a hidden 32-bit status/control `DINT` whose high bits are the status
booleans, followed by two `DINT`s.

| Structure | Bytes | Members                                                                                         |
|-----------|-------|-------------------------------------------------------------------------------------------------|
| `TIMER`   | 12    | status `DINT` (`.EN` bit 31, `.TT` bit 30, `.DN` bit 29), `.PRE` `DINT` (ms), `.ACC` `DINT` (ms) |
| `COUNTER` | 12    | status `DINT` (`.CU` 31, `.CD` 30, `.DN` 29, `.OV` 28, `.UN` 27), `.PRE` `DINT`, `.ACC` `DINT`   |
| `CONTROL` | 12    | status `DINT` (`.EN`, `.EU`, `.DN`, `.EM`, `.ER`, `.UL`, `.IN`, `.FD` in high bits), `.LEN` `DINT`, `.POS` `DINT` |

These are Logix types. Micro800 uses IEC function-block instances instead.

---

## Arrays

A Logix array stores its elements contiguously, each in the element type's wire layout,
little-endian, with no padding between elements. The one exception is the BOOL-array packing
described in [BOOL handling](#bool-handling). Logix supports up to 3 dimensions, encoded in the
`0x6000` bits of the [symbol type](tag-browsing.md#the-logix-symbol-type-bitfield). How a single
element is named in a request is in [tag services](tag-services.md#how-a-tag-is-addressed).

---

## BOOL handling

BOOL is the one type whose storage differs by context.

An atomic `BOOL` tag is 1 byte on the wire (`0x00` / `0xFF`; nonzero = true).

A `BOOL` array is packed into 32-bit words. A `BOOL[]` in ControlLogix/CompactLogix is stored and
transferred as an array of 32-bit words, and its declared length must be a multiple of 32. Two
things follow. The array index addresses the word, not the bit, so `bits[3]` selects the 4th 32-bit
word (bits 96-127), and a single-element read returns a whole word from which you extract the bit
yourself. And writes are word-granular. Changing one bit rewrites the whole 32-bit word, so keep
input and output BOOL arrays separate to avoid clobbering neighbours.

A `BOOL` inside a UDT is packed into a hidden backing field. Up to 8 BOOLs share a hidden `SINT`
byte. More than 8 pack into a `BOOL[32/64]` word array with 32-bit alignment. The template read
gives the host byte offset and the bit position within it.

Whether a client models the backing word as `DWORD` (`0xD3`) or `DINT` (`0xC4`) is a modeling
choice. The load-bearing wire fact is the 32-bit packing. A `BOOL` array reports `DWORD` in the
[symbol type](tag-browsing.md#the-logix-symbol-type-bitfield) for the same reason.

---

## References

### Rockwell publications

- Rockwell Automation, *Logix 5000 Controllers Data Access* (1756-PM020), on tag type reporting, the
  Logix `STRING` structure, and structure handles:
  <https://literature.rockwellautomation.com/idc/groups/literature/documents/pm/1756-pm020_-en-p.pdf>
- Rockwell Automation, *Type Encoding of Logix Structures in CIP Data Table Read/Write*, on the
  `0xA0` abbreviated-structure marker, alignment and padding:
  <https://www.rockwellautomation.com/content/dam/rockwell-automation/sites/downloads/pdf/TypeEncode_CIPRW.pdf>
- Rockwell Automation, *Micro800 Programmable Controllers* user manuals, on IEC types, UDFBs, and
  CIP symbolic access:
  <https://literature.rockwellautomation.com/idc/groups/literature/documents/qs/2080-qs002_-en-e.pdf>

### Reference implementations

- libplctag, the "Bits and Booleans" wiki page on BOOL-array packing:
  <https://github.com/libplctag/libplctag/wiki/Bits-and-Booleans>
- pycomm3, `LogixDriver` tag and type parsing:
  <https://github.com/ottowayi/pycomm3/blob/master/pycomm3/cip/data_types.py>
