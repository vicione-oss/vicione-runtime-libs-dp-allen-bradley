# Data type support — Logix

Which Logix data types this port implements, what each one carries in .NET, and how it is decoded.

This is the *port's* status. Which types a given controller family exposes at all is a separate
question, answered by the cross-port
[symbolic tag data types](../../AllenBradley.Documentation/cip-protocol/symbolic-tag-data-types.md);
the wire layout of each type is in the
[CIP data types reference](../../AllenBradley.Documentation/cip-protocol/cip-datatypes-reference.md).

## Supported

| Logix type            | Node             | Data point            | .NET type  | Wire size | Converter              | Controllers |
|-----------------------|------------------|-----------------------|------------|-----------|------------------------|-------------|
| **Bool**              |                  |                       |            |           |                        |             |
| `BOOL`                | `BoolNode`       | `BoolDataPoint`       | `bool`     | 1         | `BoolConverter`        | all         |
| **Signed integers**   |                  |                       |            |           |                        |             |
| `SINT`                | `SIntNode`       | `SIntDataPoint`       | `sbyte`    | 1         | `SIntConverter`        | all         |
| `INT`                 | `IntNode`        | `IntDataPoint`        | `short`    | 2         | `IntConverter`         | all         |
| `DINT`                | `DIntNode`       | `DIntDataPoint`       | `int`      | 4         | `DIntConverter`        | all         |
| `LINT`                | `LIntNode`       | `LIntDataPoint`       | `long`     | 8         | `LIntConverter`        | all         |
| **Unsigned integers** |                  |                       |            |           |                        |             |
| `USINT`               | `USIntNode`      | `USIntDataPoint`      | `byte`     | 1         | `USIntConverter`       | 5X80 only   |
| `UINT`                | `UIntNode`       | `UIntDataPoint`       | `ushort`   | 2         | `UIntConverter`        | 5X80 only   |
| `UDINT`               | `UDIntNode`      | `UDIntDataPoint`      | `uint`     | 4         | `UDIntConverter`       | 5X80 only   |
| `ULINT`               | `ULIntNode`      | `ULIntDataPoint`      | `ulong`    | 8         | `ULIntConverter`       | 5X80 only   |
| **Floating point**    |                  |                       |            |           |                        |             |
| `REAL`                | `RealNode`       | `RealDataPoint`       | `float`    | 4         | `RealConverter`        | all         |
| `LREAL`               | `LRealNode`      | `LRealDataPoint`      | `double`   | 8         | `LRealConverter`       | 5X80 only   |
| **String**            |                  |                       |            |           |                        |             |
| `STRING`              | `StringNode`     | `StringDataPoint`     | `string`   | 4 + n     | `LogixStringConverter` | all         |
| **Arrays**            |                  |                       |            |           |                        |             |
| `BOOL[n]`             | `BoolArrayNode`  | `BoolArrayDataPoint`  | `bool[]`   | 4 × n/32  | `BoolArrayConverter`   | all         |
| `SINT[n]`             | `SIntArrayNode`  | `SIntArrayDataPoint`  | `sbyte[]`  | n         | `SIntArrayConverter`   | all         |
| `INT[n]`              | `IntArrayNode`   | `IntArrayDataPoint`   | `short[]`  | 2 × n     | `IntArrayConverter`    | all         |
| `DINT[n]`             | `DIntArrayNode`  | `DIntArrayDataPoint`  | `int[]`    | 4 × n     | `DIntArrayConverter`   | all         |
| `LINT[n]`             | `LIntArrayNode`  | `LIntArrayDataPoint`  | `long[]`   | 8 × n     | `LIntArrayConverter`   | all         |
| `USINT[n]`            | `USIntArrayNode` | `USIntArrayDataPoint` | `byte[]`   | n         | `USIntArrayConverter`  | 5X80 only   |
| `UINT[n]`             | `UIntArrayNode`  | `UIntArrayDataPoint`  | `ushort[]` | 2 × n     | `UIntArrayConverter`   | 5X80 only   |
| `UDINT[n]`            | `UDIntArrayNode` | `UDIntArrayDataPoint` | `uint[]`   | 4 × n     | `UDIntArrayConverter`  | 5X80 only   |
| `ULINT[n]`            | `ULIntArrayNode` | `ULIntArrayDataPoint` | `ulong[]`  | 8 × n     | `ULIntArrayConverter`  | 5X80 only   |
| `REAL[n]`             | `RealArrayNode`  | `RealArrayDataPoint`  | `float[]`  | 4 × n     | `RealArrayConverter`   | all         |
| `LREAL[n]`            | `LRealArrayNode` | `LRealArrayDataPoint` | `double[]` | 8 × n     | `LRealArrayConverter`  | 5X80 only   |

A supported type is supported end to end: the manifest declares the node, a node mapper claims it,
`LogixDataPointsGroupsMapper` turns it into the data point, and `DataPointConverterRegistry` holds a
converter keyed by that data point. Every row round-trips, the array whole: see [Arrays](#arrays) for
what "whole" rules out.

Every converter decodes a raw little-endian span. CIP and .NET are both little-endian, so the
atomic types need no byte swap. A `STRING`'s `n` is its declared capacity: 82 for the built-in type,
88 bytes on the wire once `.LEN` and the alignment pad are counted. An array's `n` is its declared
element count, and both are configuration rather than type: the same converter serves every `n`.

### Types the 5X70 controllers have not got

Every entry in the table marked `5X80 only` is there for the same reason: the 5X70 controllers have no
`LREAL` and no unsigned integer at all, so configuring one of those there — or an array of one —
addresses a type the controller cannot resolve.

The device node type is what decides. A 5X80 device node's controller-scope container is
`ControllerTags5X80`, which lists those types among its children; the 5X70 container does not,
so the editor never offers them. `ITagScopeNode.CanBeAdded` is the guard behind that for a configuration
the editor did not build, and `DeviceNode.CanBeAdded` refuses a container whose generation is not its
device's — the pairing the first guard rests on. See
[Splitting the device node by family and generation](../ADR/2026-08-31-splitting-the-device-node-by-family-and-generation.md).

The node states the rule itself, as `ILogixTagNode.MinimumGeneration`: the oldest generation whose
vocabulary has the type. `ITagScopeNode` compares it against the container's own generation and
implements `CanBeAdded` for every scope from that, so a type that arrives with a later generation is
one line on the node and no edit to a container. The default is the oldest generation the addon
addresses, which is why `BoolNode`, `SIntNode`, `IntNode`, `DIntNode`, `LIntNode`, `RealNode`,
`StringNode`, and the array nodes of `BOOL`, the signed integers and `REAL`, say nothing. The comparison reads
`LogixGeneration` in declaration order, and the members are numbered — `Logix5X70 = 70` — so a later
generation slots in at its own number.

`LREAL` was the only type carrying that line for a while, so the mechanism had a single witness and
could as well have been a special case. The four unsigned integers are the check that it is not: each
declares the same one line, and both tag-scope containers turn it away on a 5X70 with nothing added to
either — no container gained a rule for any of them.

The five arrays of those types say what the rule is *about*. `USIntArrayNode` carries what
`USIntNode` carries, because an array of a type the controller cannot resolve is not a different
question from a scalar of it: the generation follows the element type, and the shape has nothing to do
with it.

### The unsigned integers

All four: `USINT`, `UINT`, `UDINT` and `ULINT`, carried as `byte`, `ushort`, `uint` and `ulong`. A
`USINT` is one byte with no byte order to get wrong, and the wider three are little-endian reads like
their signed counterparts.

What is worth knowing is what each shares with its signed twin: **the two are the same width and the
same bytes on the wire**, and differ only in the type the controller declares. A `SINT` read through
the `USINT` codec hands back `200` where the controller holds `-56`; an `INT` read as a `UINT` hands
back `65535` where it holds `-1`; and so on up to a `LINT` read as a `ULINT`.

Verification catches that the way it catches every other type mismatch — `LogixTypeComparison` compares
the converter's `ExpectedDataType` against the controller's declaration, and `Sint != Usint` is the
same comparison as `Int != Dint`. There is nothing special about the unsigned types there.

What is different is how little else there is. A type configured *wider* than its tag has a second tell
even if verification never ran: the decode runs out of buffer and `LogixReadBatch` reports it as that
tag's failure, and a write is refused by `SetBuffer` as out of bounds. A type configured *narrower*
loses that tell but still misreads as soon as the tag holds a value outside the narrow range. Two types
of equal width have neither — every byte pattern is legal for both, in both directions, and the value
that comes back is always the value that went in. Verification is the only thing standing there.

An array of one of them is that case one element at a time, and no worse: the array converter is the
scalar's element codec and a stride, so a `USINT[10]` configured over a `SINT[10]` is the same
mistake ten times over, and the same declaration at connect catches it.

### `STRING`

A Logix `STRING` is a predefined structure — `.LEN : DINT` then `.DATA : SINT[82]`, padded to 88
bytes — but a **scalar** in this model: one value, not an array. `StringDataPoint` carries its own
`StringMaxLength`, because the wire size follows the declared capacity rather than the type: a
`STRING_20` is the same converter and 24 bytes.

Two things are worth knowing before configuring one:

- **Characters are Latin-1**, one byte each, matching the sibling S7 addon. Anything outside Latin-1
  is written as `?`, which is lossy and unavoidable: a `STRING` stores one byte per character.
- **A value longer than the declared capacity is rejected, not truncated.** The write fails while the
  batch is being built, before any bytes reach the controller.

Verification checks the declared capacity as well as the shape, because a round trip cannot: writing
`"Hi"` into a `STRING_20` and reading `"Hi"` back says nothing about how much the tag holds.

### Arrays

One shape, and its boundaries are worth stating exactly: a **one-dimensional array of an elementary
type, transferred whole**. An `ARRAY[0..9] OF INT` is configured as one node carrying a tag name, an
element count and a poll frequency; a poll delivers one `short[10]` with the elements in index order,
and a write sends a `short[10]` back the same way. Ten more element types are that sentence with the
element type swapped — every atomic type the port has. `BOOL[n]` is the same sentence too, but it
reaches it differently; see [`BOOL[n]`](#booln).

The wire layout is the whole of what the codec needs. For ten of the eleven element types, elements
are contiguous, little-endian and
unpadded, so element *i* is the scalar codec of the element type at offset *i* × its width. An array
converter names that width and those two codec calls and nothing else, and takes all three off the
scalar converter: `IntArrayConverter` asks `IntConverter`, so a scalar `INT` and an element of an
`INT[n]` cannot be read differently. The declared extent is checked against the buffer whole before any
element is read, which turns a reply too short for the count into that tag's named failure rather than
an array filled as far as the bytes went.

A written value is checked the same way and for a sharper reason: `SetBuffer` fills the handle from
the start, so a `short[6]` sent to an `INT[10]` would leave the last four elements as the controller
had them. A value of any other length is refused while the batch is being built, with the tag in the
message, and nothing is sent for any tag in the batch.

The configured count also sizes the libplctag handle. Without it the handle reads a single element
whatever the tag holds, because the library treats every tag as an array and defaults the count to
one — which is why the scalar handles never had to say anything.

Verification checks the rank and the element count as well as the type, because a read cannot: ten
`INT`s off an `INT[20]` decode exactly as well as ten off an `INT[10]`. Both directions of a rank
disagreement are reported at connect — an array tag configured as a scalar, and a scalar tag
configured as an array — as is a count the controller does not agree with.

An array element type is matched exactly on the way in, which a scalar's never had to be. The CLR
holds arrays of same-width signed and unsigned elements assignment-compatible — `ushort[]` *is* a
`short[]` as far as a type test goes, and reads back 40000 as -25536 — so `LogixDataPoint.ConvertValue`
compares the runtime type rather than asking whether the value fits. The eight integer element types
are four such pairs, and nothing further down would notice: the bytes are identical, so the write
succeeds and the tag holds the wrong number. The two floating-point types are the exception that shows
what the rule is for — nothing is assignment-compatible with `float[]` or `double[]`, so a same-width
`int[]` or `long[]` is turned away by the type test itself. Boxed scalars are already exact, so the
rule costs the eleven scalar types nothing.

What this shape is not:

- **Not written in part.** A write is every element or none. Writing a range of elements is
  per-element addressing under another name, and waits on it.
- **Not per element.** `myArray[3]` is not an address the port takes. Whole-array access is one
  handle, one CIP request and one value; indexed access is a container node with a child per element,
  each with a symbolic path of its own.
- **Not rank 2 or 3.** Both exist on Logix and both are rejected at connect. `TagsDecoder` folds the
  dimension words into their product, so `INT[2,3]` and `INT[3,2]` are one thing in this model;
  supporting them means keeping the dimensions rather than the product, and nothing new comes off the
  wire.

### `BOOL[n]`

A `BOOL` array is configured, verified, polled and written exactly as the other ten are — one node
with a tag name, an element count and a poll frequency; one `bool[n]` in either direction. What is
different is everything underneath, because **a Logix `BOOL` array is not an array of `BOOL`s**. The
controller packs the bits 32 to a 32-bit word and allocates the words, so three things that hold for
every other element type do not hold here:

- **The controller calls it a `DWORD` array.** The `@tags` entry carries type code `0xD3`, not the
  `0xC1` of a scalar `BOOL`, and its element length is 4. `CipTypeCodeExtensions` maps that code back
  to `Bool`, because the packing is how the tag is stored and not what it holds. Logix has no `DWORD`
  a project can declare, so a rank-1 `0xD3` means a `BOOL` array and nothing else.
- **The dimension is counted in words.** A `BOOL[64]` reports `2`. `TagsDecoder` turns that into the
  64 bits it holds, the way a string structure's 86 member bytes leave it as a capacity of 82, so
  `TagDefinition` speaks one vocabulary throughout and `LogixTypeComparison` compares bits against
  bits.
- **The request is counted in words too**, and that one is not converted away: libplctag puts the
  handle's element count straight onto the CIP request, so `LogixTagAccessFactory` sizes a `BOOL`
  array's handle from `BoolArrayDataPoint.WordCount`, the `n / 32` the controller expects, while
  `ElementCount` stays the `n` the configuration declares. Every other array is sized from its
  `ElementCount`, and a scalar is sized to one.

`n` must be a multiple of 32 — the only length Studio 5000 declares a `BOOL` array with. The node
validator enforces it rather than leaving it to the connect, since no controller is needed to know
that `BOOL[10]` is not a tag anyone has.

That rule is also what makes the write safe. The issue this slice came from worried about a
read-modify-write clobbering bits the port was not asked to touch; there are none. A whole-array
write of a multiple-of-32 array is whole words from the first bit to the last, so every bit in the
buffer belongs to the tag being written. Writing *one* bit of an array would be a read-modify-write,
and that is per-element access, which this shape does not offer.

The codec is the one array converter that does not extend `AtomicArrayDataPointConverter`, whose
whole contract is *n* contiguous unpadded elements. `BoolArrayConverter` sits beside it instead:
element *i* is bit *i* mod 8 of byte *i* div 8, so bit 32 opens the second word. The raw buffer is
enough for that — libplctag's `GetBit`/`SetBit` exist but would push bit arithmetic into the access
layer, which holds one whole operation per member and nothing smaller.

## Not supported yet

| Logix type                      | Notes                                                                                            |
|---------------------------------|--------------------------------------------------------------------------------------------------|
| Writing part of an array        | A write is the whole array; a range of elements is per-element addressing under another name     |
| Multi-dimensional arrays        | Rank 2 and 3; the model keeps the product of the dimensions, not the dimensions                  |
| Arrays of `STRING` or of a UDT  | Need `TagsEntryHeader.ElementLength`, which is kept only for structures, as `MaxLength`          |
| `TIMER` / `COUNTER` / `CONTROL` | 12-byte predefined structures                                                                    |
| UDTs                            | Need the `@udt/<id>` template read to learn the member layout                                    |

A shape, a type or a capacity that disagrees with the controller is reported at connect by
`LogixConfigurationVerifier`, not misread at poll time.

## Structure members

A tag address may reach into a structure — `Program:MainProgram.Counter.PRE` is a `DINT` inside a
`COUNTER`, and the client reads it correctly — but **it cannot be configured today**.
`TagNodePropertyValidator` accepts only a plain tag name, so a dotted address fails validation
before anything reaches the controller.

That rule is about the *configured* name, and it does not stand in the way of program scope: a tag
inside a program is configured as `Count` under a `ProgramTags` container, and
`Program:MainProgram.Count` is composed from the container's `ProgramName` while the tree is walked
into data points. See [tag scoping](../explanation/tag-scoping.md).

One thing would still stand in the way if that gate opened. A member is **absent from the flat
`@tags` listing**, so the tag definitions hold nothing for it and `LogixConfigurationVerifier` reports
it as *not found on the controller*, which fails the connect. Since the poll trusts what verification
checked, a member that cannot be verified is a member nothing checks at all. That is why structure
members arrive with the structured-data-point slice rather than by relaxing the tag-name rule.

## Related

- [Decoding tag bytes into typed values](../ADR/2026-07-16-decoding-tag-bytes-into-typed-values.md)
- [Verifying configuration against the controller symbol table](../ADR/2026-07-21-verifying-configuration-against-the-symbol-table.md)
- [Client architecture](../explanation/client/architecture.md)
