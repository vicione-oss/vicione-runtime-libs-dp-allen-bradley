# Data type support — Logix

Which Logix data types this port implements, what each one carries in .NET, and how it is decoded.

This is the *port's* status. Which types a given controller family exposes at all is a separate
question, answered by the cross-port
[symbolic tag data types](../../AllenBradley.Documentation/protocol/allen-bradley-extension/symbolic-tag-data-types.md);
the wire layout of each type is in the
[CIP data types reference](../../AllenBradley.Documentation/protocol/cip/data-types.md).

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
| `BOOL[n]`             | `BoolArrayDataPointNode`  | `BoolArrayDataPoint`  | `bool[]`   | 4 × n/32  | `BoolArrayConverter`   | all         |
| `SINT[n]`             | `SIntArrayDataPointNode`  | `SIntArrayDataPoint`  | `sbyte[]`  | n         | `SIntArrayConverter`   | all         |
| `INT[n]`              | `IntArrayDataPointNode`   | `IntArrayDataPoint`   | `short[]`  | 2 × n     | `IntArrayConverter`    | all         |
| `DINT[n]`             | `DIntArrayDataPointNode`  | `DIntArrayDataPoint`  | `int[]`    | 4 × n     | `DIntArrayConverter`   | all         |
| `LINT[n]`             | `LIntArrayDataPointNode`  | `LIntArrayDataPoint`  | `long[]`   | 8 × n     | `LIntArrayConverter`   | all         |
| `USINT[n]`            | `USIntArrayDataPointNode` | `USIntArrayDataPoint` | `byte[]`   | n         | `USIntArrayConverter`  | 5X80 only   |
| `UINT[n]`             | `UIntArrayDataPointNode`  | `UIntArrayDataPoint`  | `ushort[]` | 2 × n     | `UIntArrayConverter`   | 5X80 only   |
| `UDINT[n]`            | `UDIntArrayDataPointNode` | `UDIntArrayDataPoint` | `uint[]`   | 4 × n     | `UDIntArrayConverter`  | 5X80 only   |
| `ULINT[n]`            | `ULIntArrayDataPointNode` | `ULIntArrayDataPoint` | `ulong[]`  | 8 × n     | `ULIntArrayConverter`  | 5X80 only   |
| `REAL[n]`             | `RealArrayDataPointNode`  | `RealArrayDataPoint`  | `float[]`  | 4 × n     | `RealArrayConverter`   | all         |
| `LREAL[n]`            | `LRealArrayDataPointNode` | `LRealArrayDataPoint` | `double[]` | 8 × n     | `LRealArrayConverter`  | 5X80 only   |

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
so the editor never offers them. The container's `CanBeAdded` is the guard behind that for a configuration
the editor did not build, and `DeviceNode.CanBeAdded` refuses a container whose generation is not its
device's — the pairing the first guard rests on. See
[Splitting the device node by family and generation](../ADR/2026-08-31-splitting-the-device-node-by-family-and-generation.md).

The node states the rule itself, as `ILogixDataPointNode.MinimumGeneration`: the oldest generation whose
vocabulary has the type. `ControllerTagsNode` and `ProgramTagsNode` compare it against their own
generation in `CanBeAdded`, and an array container is gated the same way by its element type, so a type that arrives with a later generation is
one line on the node and no edit to a container. The default is the oldest generation the dataport
addresses, which is why `BoolNode`, `SIntNode`, `IntNode`, `DIntNode`, `LIntNode`, `RealNode`,
`StringNode`, and the array nodes of `BOOL`, the signed integers and `REAL`, say nothing. The comparison reads
`LogixGeneration` in declaration order, and the members are numbered — `Logix5X70 = 70` — so a later
generation slots in at its own number.

`LREAL` was the only type carrying that line for a while, so the mechanism had a single witness and
could as well have been a special case. The four unsigned integers are the check that it is not: each
declares the same one line, and both tag-scope containers turn it away on a 5X70 with nothing added to
either — no container gained a rule for any of them.

The five arrays of those types say what the rule is *about*. `USIntArrayDataPointNode` carries what
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
the data point's `DataType` against the controller's declaration, and `Sint != Usint` is the same
comparison as `Int != Dint`. There is nothing special about the unsigned types there.

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

- **Characters are Latin-1**, one byte each, matching the sibling S7 dataport. Anything outside Latin-1
  is written as `?`, which is lossy and unavoidable: a `STRING` stores one byte per character.
- **A value longer than the declared capacity is rejected, not truncated.** The write fails while the
  batch is being built, before any bytes reach the controller.

Verification checks the declared capacity as well as the shape, because a round trip cannot: writing
`"Hi"` into a `STRING_20` and reading `"Hi"` back says nothing about how much the tag holds.

### Arrays

Two shapes. The first, and the one this section is about, is a **one-dimensional array of an
elementary type, transferred whole**. An `ARRAY[0..9] OF INT` is configured as one node carrying a
tag name, an element count and a poll frequency; a poll delivers one `short[10]` with the elements in
index order, and a write sends a `short[10]` back the same way. The second is the same array opened
up as a container with one data point per element; see [Array elements](#array-elements). Ten more element types are that sentence with the
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

- **Not written in part.** A write is every element or none. To write one element and leave the
  rest alone, configure that element under an array container instead.
- **Not per element.** Whole-array access is one handle, one CIP request and one value. `myArray[3]`
  as a data point of its own is the other shape, below.
- **Not rank 2 or 3.** Both exist on Logix and both are rejected at connect. `TagsDecoder` folds the
  dimension words into their product, so `INT[2,3]` and `INT[3,2]` are one thing in this model;
  supporting them means keeping the dimensions rather than the product, and nothing new comes off the
  wire.

### Array elements

An `ARRAY[0..9] OF INT` can also be configured as an **array container** — `IntArrayContainer`,
carrying the tag name — with an `IntNode` under it for each element the configuration wants. The
element node's tag name is the subscript, `[3]`, and nothing else; the tree walk composes
`Program:MainProgram.testIntArray[3]` from the program, the container and the subscript, the way it
composes a program-scoped tag's address from the program and the tag. From there down the element is
a scalar `INT` in every respect: a one-element handle, `IntConverter`, a `short` in and out, and a
write that touches that element and no other. Every element type but `BOOL` has a container; a
`BOOL` element is a bit inside a word, and writing one is a read-modify-write the port has not proved.

Each element is a data point of its own, with its own poll frequency and its own channels. Element 3
at 100 ms and element 7 at 5 s fall into two poll groups of the same array. The cost is one libplctag
handle and one CIP request per element where a whole-array read is one of each; `AllowPacking` is set
on every handle, so the library packs them into as few packets as it can, but a configuration that
wants all ten elements at one frequency should be a whole-array node instead.

The container declares no element count. The controller does, and the lookup reads every subscript
against it: the flat `@tags` listing never names an element, so `testIntArray[3]` walks to the
declaration of `testIntArray` and then into one element of it, a scalar of the array's element type
at the element's own address, and `LogixTypeComparison` compares that scalar. Three things are
reported at connect that a whole-array node cannot run into: a subscript on a tag the controller
declares as a scalar and a subscript at or past the declared count, both of which stop the walk and
are reported as *not found on the controller*, and — the ordinary type mismatch, read the other
way — an element configured as a `DINT` on an `INT[10]`.

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
  `DeclaredType` speaks one vocabulary throughout and `LogixTypeComparison` compares bits against
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
| A range of elements as one value | `myArray[2..8]` as a single `short[7]` is neither whole-array nor per-element; needs its own shape |
| Multi-dimensional arrays        | Rank 2 and 3; the model keeps the product of the dimensions, not the dimensions                  |
| Arrays of `STRING` or of a UDT  | Declared, and a string array's capacity known from its template; no data point has the shape     |
| `TIMER` / `COUNTER` / `CONTROL` | 12-byte predefined structures                                                                    |
| A UDT as one value              | A UDT is opened into members, each a data point of the member's own type (see [Structure members](#structure-members)); a whole structure as one data point has no converter |

A shape, a type or a capacity that disagrees with the controller is reported at connect by
`LogixConfigurationVerifier`, not misread at poll time.

### Templates

The `@tags` listing names a structure only by its **template id**, so a structure leaves `TagsDecoder`
as a `STRUCTURE` naming that id, and nothing about the members. The browse then reads every
template the listing names — `@udt/<id>` per distinct id, and again for any member that is itself a
structure — and holds each as a `TemplateDefinition` beside the tags. A template says what the
structure is called, how many bytes an instance occupies, and for every member its name, byte offset,
declared type, and the bit position of a packed `BOOL`. The wire layout of the two reads is in
[reading a UDT definition](../../AllenBradley.Documentation/libplctag/reading-a-udt-definition.md);
the decoder is `TemplateDecoder`, a pure function over the bytes, like `TagsDecoder`.

`STRING` is the one structure this dataport reads as a value, so it is the one data type a template can
give a structure, and the lookup is where it does: `SymbolTable.GetDeclaredTypeAtPath` walks to the
tag or the member the path names (see [Structure members](#structure-members)), and if what it found
names a template with a `.DATA : SINT[n]` member, hands it back as a `String` of capacity `n`. So a `STRING_20`
tag and a `STRING_20` member inside a UDT both verify against a configured capacity of 20, and a
`TIMER` configured as a `STRING` is a data-type mismatch, not a capacity one: it comes back
`STRUCTURE`, and nothing turns it into a string. The listing's element length is not read at all. A system structure — one whose symbol type has bit `0x1000` set — names no template, because the
controller serves none for it, and stays `STRUCTURE`. A template the controller will not serve, or one
that does not decode, fails the connect the way a listing that will not read does: the browse is what
connect is.

## Structure members

A tag address may reach into a structure — `Program:MainProgram.Counter.PRE` is a `DINT` inside a
`COUNTER`, and the client reads it correctly — and a member is verified at connect the way a tag is.
The tree has a node for a UDT's members, the walk builds a data point per member, and the symbol-table
lookup follows the tag's template to the member's declaration. What is not yet in place is a round
trip against a UDT on the 5X80: no tag in that suite is one.

The node is the **UDT container**, `UDT Instance` in the editor, one per generation like the scopes. It is the
array container's pattern for members: the container carries the tag name, each child carries a member
name as its tag name, and a member that is itself a UDT is the same container nested with the member
name as its own. `TagNamePropertyValidator` accepts a plain name at every level and refuses a dotted
one, because a dotted name is two nodes. The container states no type: the controller's template says
what the members are, and each child is gated by the generation the container carries down from its
scope, so an `LREAL` member is refused on a 5X70 the way an `LREAL` tag is. An array container nests
under it the same way, gated by the same generation, so an array member is opened per element and
`MyMotor.Readings[3]` is one data point. The round trip of that address against a controller is not
yet proved.

The node names UDTs only. Whether the predefined structures — `TIMER`, `COUNTER`, `STRING` — and
Add-On Instruction instances are opened the same way, or get a node of their own, is **not decided**.
Nothing refuses one today: the container states no type, and the lookup follows whatever template the
tag names, so `Timer1.PRE` under a UDT container resolves to a `DINT`. The template's name is visible
there, and that is where a refusal would go.

The data point carries the member path. `TagPath` holds the program, the tag, the members reached
through, and the element, each apart: the first two say where the declaration is, the last two where
inside it the value is. The walk fills the members from the UDT containers above the leaf, the way it
fills the subscript from an array container, and `Motor.Ramp.Target` renders from the parts when
libplctag asks. A dotted address parses back the same way, so `TagPath.Parse("Motor.Speed")` is the
tag `Motor` and the member `Speed`; whether `Motor` has one is the symbol table's question.

A member is **absent from the flat `@tags` listing**, so `SymbolTable.GetDeclaredTypeAtPath` is asked
by path, not by address. The program and the tag find the tag's declaration; each member name is then found in a
template — the tag's for the first, the member's own for the next — matched case-insensitively, the
way the controller matches tag names. The subscript is the last step: the listing names no element,
so `Motor.Readings[3]` walks to the array member `Readings` and then into one element of it, a scalar
of the member's type, exactly as `Readings[3]` does for an array tag.

What comes back, for a tag, a member and an element alike, is a `DeclaredType`: the address the path
renders to, the data type, a string's capacity, the rank and the element count. A member has no
address of its own (CONTEXT.md, "Member"), so the one on its declared type is the tag's with the member
path behind it — `Motor.Ramp.Target` — which is also what libplctag is handed. Inside the lookup, the
listing's entry and the template's member both hold a `TagDefinition`, the walk's own node, which also
names the template to walk into next; nothing outside `Client/Tags/Symbols` sees it. A `STRING`
member therefore compares like a `STRING` tag, and a member of a non-string structure configured as a
`STRING` is a data-type mismatch.

A path that stops on a structure is answered rather than refused. `Motor`, and `Motor.Ramp` where
`Ramp` is a nested UDT, both come back as a scalar `STRUCTURE` at that address. `STRUCTURE` is the
dataport's own spelling for a structure it reads as members rather than as one value, and it is distinct
from `UNKNOWN`, which now means one thing only: an elementary type code outside the range this dataport
decodes. A UDT container builds no data point of its own, so a path stops on a structure only when a
leaf node carries a structure's tag name, and the verifier says so — *configured DINT, controller
reports STRUCTURE*.

The lookup answers with the definition, or with nothing when the walk stops short: the listing has not
got the tag, the template has not got the member, a member is asked of something that has none — an
atomic value, an array (`Motor.History.Target` would need a subscript first, and that is a shape the
port does not accept), or a structure whose template the controller does not serve — or an element is
asked of a scalar or past the declared count. Every such miss is reported by
`LogixConfigurationVerifier` as *not found on the controller*, the same as a tag the listing lacks.

## Related

- [Decoding tag bytes into typed values](../ADR/2026-07-16-decoding-tag-bytes-into-typed-values.md)
- [Verifying configuration against the controller symbol table](../ADR/2026-07-21-verifying-configuration-against-the-symbol-table.md)
- [Client architecture](../explanation/client/architecture.md)
