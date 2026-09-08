# Data type support — Logix

Which Logix data types this port implements, what each one carries in .NET, and how it is decoded.

This is the *port's* status. Which types a given controller family exposes at all is a separate
question, answered by the cross-port
[symbolic tag data types](../../AllenBradley.Documentation/cip-protocol/symbolic-tag-data-types.md);
the wire layout of each type is in the
[CIP data types reference](../../AllenBradley.Documentation/cip-protocol/cip-datatypes-reference.md).

## Supported

| Logix type | Data point        | .NET type | Wire size | Converter              | Controllers |
|------------|-------------------|-----------|-----------|------------------------|-------------|
| `BOOL`     | `BoolDataPoint`   | `bool`    | 1         | `BoolConverter`        | all         |
| `SINT`     | `SIntDataPoint`   | `sbyte`   | 1         | `SIntConverter`        | all         |
| `INT`      | `IntDataPoint`    | `short`   | 2         | `IntConverter`         | all         |
| `DINT`     | `DIntDataPoint`   | `int`     | 4         | `DIntConverter`        | all         |
| `LINT`     | `LIntDataPoint`   | `long`    | 8         | `LIntConverter`        | all         |
| `USINT`    | `USIntDataPoint`  | `byte`    | 1         | `USIntConverter`       | 5X80 only   |
| `UINT`     | `UIntDataPoint`   | `ushort`  | 2         | `UIntConverter`        | 5X80 only   |
| `REAL`     | `RealDataPoint`   | `float`   | 4         | `RealConverter`        | all         |
| `LREAL`    | `LRealDataPoint`  | `double`  | 8         | `LRealConverter`       | 5X80 only   |
| `STRING`   | `StringDataPoint` | `string`  | 88        | `LogixStringConverter` | all         |

Every converter decodes a raw little-endian span. CIP and .NET are both little-endian, so the
atomic types need no byte swap.

### Types the 5X70 controllers have not got

Every entry in the table marked `5X80 only` is there for the same reason: the 5X70 controllers have no
`LREAL` and no unsigned integer at all, so configuring one of those there addresses a type the
controller cannot resolve.

The device node type is what decides. A 5X80 device node's controller-scope container is
`ControllerTags5X80`, which lists those types among its children; the 5X70 container does not,
so the editor never offers them. `ITagScopeNode.CanBeAdded` is the guard behind that for a configuration
the editor did not build, and `DeviceNode.CanBeAdded` refuses a container whose generation is not its
device's — the pairing the first guard rests on. See
[Splitting the device node by family and generation](../ADR/2026-08-31-splitting-the-device-node-by-family-and-generation.md).

The node states the rule itself, as `ILogixScalarNode.MinimumGeneration`: the oldest generation whose
vocabulary has the type. `ITagScopeNode` compares it against the container's own generation and
implements `CanBeAdded` for every scope from that, so a type that arrives with a later generation is
one line on the node and no edit to a container. The default is the oldest generation the addon
addresses, which is why `BoolNode`, `SIntNode`, `IntNode`, `DIntNode`, `LIntNode` and `StringNode` say
nothing. The comparison reads `LogixGeneration`
in declaration order, and the members are numbered — `Logix5X70 = 70` — so a later generation slots in
at its own number.

`LREAL` was the only type carrying that line for a while, so the mechanism had a single witness and
could as well have been a special case. The unsigned integers are the check that it is not: each
declares the same one line, and both tag-scope containers turn it away on a 5X70 with nothing added to
either.

### The unsigned integers

`USINT` and `UINT` so far, carried as `byte` and `ushort`; `UDINT` and `ULINT` follow. A `USINT` is one
byte with no byte order to get wrong, and the wider ones are little-endian reads like their signed
counterparts.

What is worth knowing is what each shares with its signed twin: **the two are the same width and the
same bytes on the wire**, and differ only in the type the controller declares. A `SINT` read through
the `USINT` codec hands back `200` where the controller holds `-56`; an `INT` read as a `UINT` hands
back `65535` where it holds `-1`.

Verification catches that the way it catches every other type mismatch — `LogixTypeComparison` compares
the converter's `ExpectedDataType` against the controller's declaration, and `Sint != Usint` is the
same comparison as `Int != Dint`. There is nothing special about the unsigned types there.

What is different is how little else there is. A type configured *wider* than its tag has a second tell
even if verification never ran: the decode runs out of buffer and `LogixReadBatch` reports it as that
tag's failure, and a write is refused by `SetBuffer` as out of bounds. A type configured *narrower*
loses that tell but still misreads as soon as the tag holds a value outside the narrow range. Two types
of equal width have neither — every byte pattern is legal for both, in both directions, and the value
that comes back is always the value that went in. Verification is the only thing standing there.

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

## Not supported yet

| Logix type                          | Notes                                                              |
|-------------------------------------|--------------------------------------------------------------------|
| `BOOL[]`                            | Packs into 32-bit words; the atomic `BOOL` is supported             |
| `UDINT` / `ULINT`                   | 5X80 controllers only, gated the way `USINT` is; one slice each     |
| `TIMER` / `COUNTER` / `CONTROL`     | 12-byte predefined structures                                       |
| UDTs                                | Need the `@udt/<id>` template read to learn the member layout       |
| Arrays of any type                  | The model carries scalars only; an array tag is a shape mismatch    |

An array or a structure configured as a scalar is reported at connect by
`LogixConfigurationVerifier`, not misread at poll time.

## Structure members

A tag address may reach into a structure — `Program:MainProgram.Counter.PRE` is a `DINT` inside a
`COUNTER`, and the client reads it correctly — but **it cannot be configured today**.
`ScalarNodePropertyValidator` accepts only a plain tag name, so a dotted address fails validation
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
