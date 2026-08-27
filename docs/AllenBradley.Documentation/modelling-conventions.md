# Modelling conventions — the types we define

**Every domain type we introduce is a `readonly record struct` or an `enum`. Never a bare
primitive.** A gateway is not a `string`, a CIP type code is not a `byte`, a routing path is not a
`string`. Give each domain concept its own named type, and the compiler will stop you passing a path
where a gateway was meant, or a raw `ushort` where a decoded symbol type belongs.

This rule governs the types **we** define to carry domain data across our own boundaries. It does not
reach into the primitives we borrow from the wire, from libplctag, or from
`ViciOne.Suite.DataPort.Extensions` — see [What the rule does not touch](#what-the-rule-does-not-touch).

## The default: `readonly record struct`

A model type is a value — two of them with the same contents are the same thing, and none of them has
an identity you mutate over time. That is exactly what a `readonly record struct` gives you: value
equality, no heap allocation, and immutability enforced by the compiler.

Wrap a single primitive in a one-field record struct rather than passing it raw. This is the pattern
already in the tree — see `Gateway` and `Path` in
[`Model/DataPort/Device/LogixClientInformation.cs`](../../src/AllenBradley.Logix/Model/DataPort/Device/LogixClientInformation.cs):

```csharp
public readonly record struct Gateway(string Value);

public readonly record struct Path(string Value);
```

For a multi-field value or one that needs named construction and outcome logic, give the struct a
body — see `LogixTagReadResult` in
[`Client/Tags/LogixTagReadResult.cs`](../../src/AllenBradley.Logix/Client/Tags/LogixTagReadResult.cs),
which keeps its constructor private and exposes `Ok(...)` / `Failed(...)` factories.

House rules for these types:

- **`readonly` always.** A mutable struct is a footgun; the record is a snapshot, not a variable.
- **Positional for pure carriers.** Prefer `record struct Foo(Bar Bar, Baz Baz)` and document each
  parameter with `<param>` XML doc, as `LogixClientInformation` does.
- **Name the concept, not the primitive.** `Gateway`, not `GatewayString`; the type already says it
  is a value.

## Closed sets: `enum`

A fixed set of alternatives is an `enum`, not a struct of constants and not a `string`. An enum is a
named domain type, so it satisfies the "never a bare primitive" rule too. The tree already does this
for [`CipType`](../../src/AllenBradley.Logix/Model/CipType.cs) (byte-backed to the wire codes),
[`LogixQuality`](../../src/AllenBradley.Logix/Model/DataPort/DataPoints/LogixQuality.cs), and
[`LogixControllerType`](../../src/AllenBradley.Logix/Model/DataPort/Device/LogixControllerType.cs).

Back an enum with its wire representation (`: byte`) only when the values *are* the wire codes, as
`CipType` does; otherwise leave it a plain `int`-backed enum.

## When a struct genuinely will not do

A `readonly record struct` cannot be abstract, cannot inherit, and copies on every pass. A handful of
model types need reference semantics for a real reason; those may be a **`sealed record`** or, only if
records do not fit, a **`sealed class`** — always `sealed`, still immutable:

- **Polymorphic hierarchies behind an interface.** `ILogixDataPoint` and its implementations, or the
  generic [`LogixDataPointValue<TDomain>`](../../src/AllenBradley.Logix/Model/DataPort/DataPoints/LogixDataPointValue.cs),
  need a reference type to carry an open type parameter and be substituted through the interface.
- **Types that must satisfy a base contract from the extensions package.** `LogixClientInformation`
  is a `sealed record` because it implements `IClientInformation` and is used as a cache key by
  reference — but note its *fields* are still `Gateway` and `Path` record structs, not raw strings.

Reach for this deliberately, and prefer `sealed record` over `sealed class` so you keep value
equality. If you are only wrapping data, you almost certainly want a struct.

## What the rule does not touch

The rule is about **modelling the domain**, not about banning the `int` keyword. Leave alone:

- **Wire and library primitives we do not own** — the `ushort` a symbol-type field arrives as, the
  `ReadOnlyMemory<byte>` a tag read produces, the `byte` a CIP code travels as. Decode them *into* our
  types at the boundary (`SymbolType.AtomicType(ushort) → CipType`); do not wrap them in transit.
- **Local arithmetic and scratch** — loop indices, buffer lengths, byte offsets, bitmasks inside a
  decoder. A number used for counting is a number, not a domain value.
- **Types from `ViciOne.Suite.DataPort.Extensions`** — use them as given; the rule is for the types we
  introduce.

The test: *does this value name a domain concept that crosses one of our method boundaries?* If yes,
it gets a type. If it is a number you are doing maths on inside one method, it does not.
