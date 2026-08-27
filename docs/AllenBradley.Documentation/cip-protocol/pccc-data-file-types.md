# PCCC Data-File Types — MicroLogix, SLC 500, PLC-5

Which data types exist on the controllers that address data by **numbered data file**, how each file's
elements are laid out, and what these families do not have. **Client-agnostic** — this describes the
controllers and the wire, not how any specific library handles them.

Where the values overlap with CIP types — a 16-bit integer is a 16-bit integer — the encoding is in the
[CIP Data Types Reference](cip-datatypes-reference.md) and is not repeated here. The counterpart for
the tag-addressed families is [Symbolic Tag Data Types](symbolic-tag-data-types.md).

**Who addresses by file:** MicroLogix 1000/1100/1200/1400 and SLC 500 (RSLogix 500), and PLC-5
(RSLogix 5). All three speak **PCCC**, tunneled inside EtherNet/IP where they have Ethernet at all —
see [Legacy PCCC tunneling](cip-networking-overview.md#legacy-pccc-tunneling) and
[Controller families](controller-families-and-routing.md#the-lines).

---

## 1. The file letter *is* the type

There are no tags here, no declarations, and no user-defined structures. Memory is a set of numbered
**data files**, and each file has a **type letter** that fixes the type of every element in it: an `N`
file holds 16-bit integers, an `F` file holds 32-bit floats, and nothing can change that.

This is the deep difference from symbolic addressing. On Logix the reply carries a type code because
the controller knows the tag's type and the client may not. In PCCC there is no type code on the wire:
the request names a file, an element, and a word count, and both ends already know what those words
mean because the file letter said so. A client that reads `F8:0` as two words of integer will get two
words of integer.

### Address syntax

```text
N7:30        file letter N, file 7, element 30
B3:2/0       bit 0 of element 2 in binary file 3
T4:0.ACC     the ACC field of timer element 0 in file 4
F8:12        float element 12 in file 8
ST9:1        string element 1 in file 9
```

Files 0, 1, and 2 are reserved for the output image, input image, and processor status, so their
letters are redundant with their numbers. Files 3–8 have the conventional defaults below. Files 9–255
are user-defined and may be given any of the data types.

---

## 2. File types and element sizes

| Letter | Type                | Default file | Element size | Element holds                                        |
|--------|---------------------|--------------|--------------|------------------------------------------------------|
| `O`    | Output image        | 0            | 1 word       | Output bits for one I/O slot                         |
| `I`    | Input image         | 1            | 1 word       | Input bits for one I/O slot                          |
| `S`    | Processor status    | 2            | 1 word       | Status bits; layout is processor-dependent           |
| `B`    | Binary / bit        | 3            | 1 word       | 16 individually addressable bits                     |
| `T`    | Timer               | 4            | 3 words      | status word, `.PRE`, `.ACC`                          |
| `C`    | Counter             | 5            | 3 words      | status word, `.PRE`, `.ACC`                          |
| `R`    | Control             | 6            | 3 words      | status word, `.LEN`, `.POS`                          |
| `N`    | Integer (16-bit)    | 7            | 1 word       | Signed two's complement, `-32768`..`32767`           |
| `F`    | Float (32-bit)      | 8            | 2 words      | IEEE 754 single                                      |
| `L`    | Long (32-bit)       | user (9–255) | 2 words      | Signed two's complement, `-2^31`..`2^31-1`           |
| `ST`   | String              | user (9–255) | 42 words     | Length word + 82 ASCII characters                    |
| `A`    | ASCII               | user (9–255) | 1 word       | Two ASCII characters                                 |

Instruction-support files — `MG` (message), `PD` (PID), `BT` (block transfer), `SC` (SFC status), and
PLC-5's `D` (BCD) — exist alongside these but are operands of specific instructions rather than data
types a client reads for process values.

### Mapping to CIP and .NET types

| File type | CIP equivalent | C#-alias | CTS-name        |
|-----------|----------------|----------|-----------------|
| `B` bit   | `BOOL`         | `bool`   | `System.Boolean`|
| `N`       | `INT`          | `short`  | `System.Int16`  |
| `L`       | `DINT`         | `int`    | `System.Int32`  |
| `F`       | `REAL`         | `float`  | `System.Single` |
| `ST`      | `STRING`       | `string` | `System.String` |
| `A`       | `SINT` pairs   | `char`   | `System.Char`   |

---

## 3. Element layouts

### Timer, counter, and control elements

Three 16-bit words each, with the status bits in word 0:

| Element | Word 0 (status bits)                                                    | Word 1 | Word 2 |
|---------|-------------------------------------------------------------------------|--------|--------|
| `T`     | `.EN` bit 15, `.TT` bit 14, `.DN` bit 13                                | `.PRE` | `.ACC` |
| `C`     | `.CU` 15, `.CD` 14, `.DN` 13, `.OV` 12, `.UN` 11, `.UA` 10              | `.PRE` | `.ACC` |
| `R`     | `.EN` 15, `.EU` 14, `.DN` 13, `.EM` 12, `.ER` 11, `.UL` 10, `.IN` 9, `.FD` 8 | `.LEN` | `.POS` |

`.PRE` and `.ACC` are 16-bit signed values, so they cap at 32767. They count **time-base ticks, not
milliseconds** — the base is chosen per instruction (1.0 s or 0.01 s; some MicroLogix models add
0.001 s), which is unlike the Logix `TIMER`, whose `.PRE`/`.ACC` are milliseconds in a `DINT`. A client
that reports a duration has to know the base, and the base is not in the data file.

### String elements

An `ST` element is **42 words = 84 bytes**:

```text
Offset  Size      Meaning
------  --------  --------------------------------------------
0x00    1 word    Length — number of valid characters (0..82)
0x02    41 words  82 ASCII characters, two per word
```

The characters are **byte-swapped within each word**: the first character of a pair is in the word's
**high** byte, the second in its low byte. Read the element as a little-endian byte stream and every
character pair comes out reversed — `"Hi"` reads as `iH`. This is the one place in these families where
byte order is not simply little-endian, and it is why `ST` files need a swap step that `N` and `F` files
do not. Only the first `Length` characters are meaningful.

### Bit access

A `B` file's element is a 16-bit word whose bits are addressed individually (`B3:2/0`), and a flat form
addresses the bit across the whole file (`B3/32` is bit 0 of element 2). Bits inside other file types
are addressed the same way — `N7:0/5` is bit 5 of an integer, and `T4:0/DN` is a named status bit.

---

## 4. What these families do not have

- **No 8-bit type**, signed or unsigned. The narrowest data value is the 16-bit word.
- **No unsigned types** at all. `N` and `L` are two's complement.
- **No 64-bit type** and **no `LREAL`** — `F` is single precision, and that is the widest float.
- **No user-defined structures.** `T`/`C`/`R` are the only structured elements, and their shape is
  fixed by the processor.
- **No 32-bit integer on PLC-5.** The `L` file is a MicroLogix 1400 and newer-SLC feature; on a PLC-5,
  32-bit values must be moved as pairs of `N` words.
- **No symbolic names on the wire.** RSLogix 500 symbols and descriptions live in the program file, not
  in the controller, so there is nothing for a client to browse — the equivalent of a `@tags` listing
  does not exist. The addresses a client uses come from the program printout or from whoever wrote it.

---

## 5. Availability matrix

Native support — what the programming tool exposes as a data-file type.

| Type                        | MicroLogix        | SLC 500           | PLC-5      |
|-----------------------------|-------------------|-------------------|------------|
| `BOOL` (bit)                | ✅ (`B`)           | ✅ (`B`)           | ✅ (`B`)    |
| 16-bit integer              | ✅ (`N`)           | ✅ (`N`)           | ✅ (`N`)    |
| 32-bit integer              | ✅*️ (`L`)         | ✅*️ (`L`)         | ❌          |
| 32-bit float                | ✅ (`F`)           | ✅*️ (`F`)         | ✅ (`F`)    |
| String                      | ✅ (`ST`)          | ✅ (`ST`)          | ✅ (`ST`)   |
| ASCII                       | ✅ (`A`)           | ✅ (`A`)           | ✅ (`A`)    |
| Timer / Counter / Control   | ✅ (`T`/`C`/`R`)   | ✅ (`T`/`C`/`R`)   | ✅ (`T`/`C`/`R`) |
| Array                       | ✅*️               | ✅*️               | ✅*️        |
| 8-bit, unsigned, 64-bit, `LREAL`, UDT | ❌      | ❌                 | ❌          |

Caveats:

- **32-bit integer** is the `L` file — MicroLogix 1400 and newer SLC processors only.
- **SLC float** requires an SLC 5/03 with OS301 or later, or a 5/04 or 5/05.
- **Arrays** are the natural sequence of elements inside a data file, not a declared array type: `N7:0`
  through `N7:9` is a ten-element integer array only because a client chooses to read ten words.

---

## 6. Byte order and transport

Values in PCCC payloads are **little-endian**, like CIP — the `ST` character swap of
[§3](#string-elements) is the exception, and it is a swap inside each word rather than a different
endianness for the value.

Access is by **typed read/write services** naming a file, element, and word count (the SLC protected
typed logical read/write family). Over Ethernet these are carried by the PCCC object, class `0x67`,
service `0x4B`; controllers without native Ethernet are reached through a bridge, and the route path
hops through it. Both are covered in
[Legacy PCCC tunneling](cip-networking-overview.md#legacy-pccc-tunneling) and
[Reaching a legacy controller through a bridge](controller-families-and-routing.md#reaching-a-legacy-controller-through-a-bridge).

---

## 7. References

### Rockwell publications

- Rockwell Automation — *SLC 500 Family Addressing Reference Manual* (5000-RM005) — file letters,
  default file numbers, element sizes, address syntax:
  <https://literature.rockwellautomation.com/idc/groups/literature/documents/rm/5000-rm005_-en-p.pdf>
- Rockwell Automation — *SLC 500 Instruction Set Reference Manual* (1747-RM001) — Timer/Counter/Control
  layouts and status bits, string and ASCII files:
  <https://literature.rockwellautomation.com/idc/groups/literature/documents/rm/1747-rm001_-en-p.pdf>
- Rockwell Automation — *DF1 Protocol and Command Set Reference Manual* (1770-6.5.16) — PCCC commands
  and the data-file model:
  <https://literature.rockwellautomation.com/idc/groups/literature/documents/rm/1770-rm516_-en-p.pdf>

### Reference implementations

- libplctag — *Strings and String Handling*, on the `ST` element and its within-word byte swap:
  <https://github.com/libplctag/libplctag/wiki/Strings-and-String-Handling>
- pycomm3 — `SLCDriver`, PCCC file access and per-file-type parsing:
  <https://github.com/ottowayi/pycomm3>

### Related in-tree docs

- [`symbolic-tag-data-types.md`](symbolic-tag-data-types.md) — the tag-addressed families
- [`cip-datatypes-reference.md`](cip-datatypes-reference.md) — how each type is encoded on the wire
- [`controller-families-and-routing.md`](controller-families-and-routing.md) — which controller is
  which line, its programming tool, and its route path
- [`cip-networking-overview.md`](cip-networking-overview.md) — wire stack, object model, PCCC tunneling
