# Modelling conventions: the types we define

**Every domain type we introduce is a `readonly record struct` or an `enum`. Never a bare
primitive.** A gateway is not a `string`, and a CIP type code is not a `byte`. Give each domain
concept its own named type, and the compiler will stop you passing a path where a gateway was meant,
or a raw `ushort` where a decoded symbol type belongs.

This rule governs the types **we** define to carry domain data across our own boundaries. It does not
reach the primitives we borrow from the wire, from libplctag, or from
`ViciOne.Suite.DataPort.Extensions`. Those are covered under
[What the rule does not touch](#what-the-rule-does-not-touch).

## The default: `readonly record struct`

A model type is a value. Two of them with the same contents are the same thing, and none has an
identity you mutate over time. A `readonly record struct` matches that shape exactly, giving you
value equality, no heap allocation, and immutability the compiler enforces.

Wrap a single primitive in a one-field record struct rather than passing it raw. The tree already
does this with `Gateway` and `Path` in
[`Model/DataPort/Device/LogixClientInformation.cs`](../../src/AllenBradley.Logix/Model/DataPort/Device/LogixClientInformation.cs):

```csharp
public readonly record struct Gateway(string Value);

public readonly record struct Path(string Value);
```

For a multi-field value, or one that needs named construction and outcome logic, give the struct a
body. `LogixTagReadResult` in
[`Client/Tags/LogixTagReadResult.cs`](../../src/AllenBradley.Logix/Client/Tags/LogixTagReadResult.cs)
does that, keeping its constructor private behind `Ok(...)` and `Failed(...)` factories.

House rules for these types:

- **`readonly` always.** A mutable struct is a footgun. The record is a snapshot, not a variable.
- **Positional for pure carriers.** Prefer `record struct Foo(Bar Bar, Baz Baz)` and document each
  parameter with `<param>` XML doc, as `LogixClientInformation` does.
- **Name the concept, not the primitive.** `Gateway`, not `GatewayString`. The type already says it
  is a value.

## Closed sets: `enum`

A fixed set of alternatives is an `enum`, not a struct of constants and not a `string`. An enum is a
named domain type, so it satisfies the "never a bare primitive" rule too. The tree already does this
for [`AllenBradleyDataType`](../../src/AllenBradley.Logix/Model/AllenBradleyDataType.cs),
[`LogixQuality`](../../src/AllenBradley.Logix/Model/DataPort/DataPoints/LogixQuality.cs) and
[`LogixControllerType`](../../src/AllenBradley.Logix/Model/DataPort/Device/LogixControllerType.cs).

Back an enum with its wire representation (`: byte`) only when the values *are* the wire codes, as
[`CipTypeCode`](../../src/AllenBradley.Logix/Client/Tags/Definitions/CipTypeCode.cs) does. Otherwise
leave it a plain `int`-backed enum. A wire-backed enum is a decoder's type, not the model's: it lives
next to the code that reads the bytes and translates into a model enum on the way out, which is why
`CipTypeCode` sits under `Client/` while `AllenBradleyDataType` is what a tag definition carries.

## When a struct genuinely will not do

A `readonly record struct` cannot be abstract, cannot inherit, and copies on every pass. A few model
types need reference semantics for a real reason. Those may be a **`sealed record`**, or a
**`sealed class`** if records do not fit at all. Always `sealed`, and still immutable. Two cases in
the tree qualify.

- **Polymorphic hierarchies behind an interface.** `ILogixDataPoint` and the generic
  [`LogixDataPoint<TDomain>`](../../src/AllenBradley.Logix/Model/DataPort/DataPoints/LogixDataPoint.cs)
  its implementations derive from need a reference type to carry an open type parameter and be
  substituted through the interface. So do the value records they nest.
- **Types that must satisfy a base contract from the extensions package.** `LogixClientInformation`
  is a `sealed record` because it implements `IClientInformation` and is used as a cache key by
  reference. Its *fields* are still `Gateway` and `Path` record structs, not raw strings.

Reach for this deliberately, and prefer `sealed record` over `sealed class` so you keep value
equality. If you are only wrapping data, you almost certainly want a struct.

## What the rule does not touch

The rule is about **modelling the domain**, not about banning the `int` keyword. Leave alone:

- **Wire and library primitives we do not own.** The `ushort` a symbol-type field arrives as, the
  `ReadOnlyMemory<byte>` a tag read produces, the `byte` a CIP code travels as. Decode them *into*
  our types at the boundary (`SymbolType.AtomicType(ushort) → AllenBradleyDataType`) rather than
  wrapping them in transit.
- **Local arithmetic and scratch.** Loop indices, buffer lengths, byte offsets, bitmasks inside a
  decoder. A number used for counting is a number, not a domain value.
- **Types from `ViciOne.Suite.DataPort.Extensions`.** Use them as given. The rule is for the types we
  introduce.

Ask whether the value names a domain concept that crosses one of our method boundaries. If it does,
it gets a type. If it is a number you are doing maths on inside a single method, it does not.
