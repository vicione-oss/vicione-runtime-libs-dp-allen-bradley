# Modelling conventions: the types we define

**Every domain type we introduce is a `readonly record struct` or an `enum`. Never a bare
primitive.** A slot number is not an `int`, and a firmware revision is not a `string`. Give each
domain concept its own named type, and the compiler will stop you handing a slot number to something
that wanted a rack address, or a raw `ushort` to something that wanted a decoded symbol type.

This rule governs the types **we** define to carry domain data across our own boundaries. It does not
reach the primitives we borrow from the wire, from libplctag, or from
`ViciOne.Suite.DataPort.Extensions`. Those are covered under
[What the rule does not touch](#what-the-rule-does-not-touch).

## The default: `readonly record struct`

A model type is a value. Two of them with the same contents are the same thing, and none has an
identity you mutate over time. A `readonly record struct` matches that shape exactly, giving you
value equality, no heap allocation, and immutability the compiler enforces.

Wrap a single primitive in a one-field record struct rather than passing it raw:

```csharp
public readonly record struct SlotNumber(int Value);

public readonly record struct FirmwareRevision(string Value);
```

For a multi-field value, or one that needs named construction and outcome logic, give the struct a
body. Keep its constructor private behind factory methods that each say what the result means: an
outcome that either carries a value or carries the reason it has none gets one factory per case, and
no caller can build a half-populated one.

House rules for these types:

- **`readonly` always.** A mutable struct is a footgun. The record is a snapshot, not a variable.
- **Positional for pure carriers.** Prefer `record struct Foo(Bar Bar, Baz Baz)` and document each
  parameter with `<param>` XML doc.
- **Name the concept, not the primitive.** `SlotNumber`, not `SlotNumberInt`. The type already says
  it is a value.

## Closed sets: `enum`

A fixed set of alternatives is an `enum`, not a struct of constants and not a `string`. An enum is a
named domain type, so it satisfies the "never a bare primitive" rule too.

Back an enum with its wire representation (`: byte`) only when the values *are* the wire codes.
Otherwise leave it a plain `int`-backed enum. A wire-backed enum is a decoder's type rather than the
model's, so it lives next to the code that reads the bytes and translates into a model enum on the
way out. That is why the codes a decoder recognises sit under the client, while the data type a tag
definition carries is a model enum of its own.

## When a struct genuinely will not do

A `readonly record struct` cannot be abstract, cannot inherit, and copies on every pass. A few model
types need reference semantics for a real reason. Those may be a **`sealed record`**, or a
**`sealed class`** if records do not fit at all. Always `sealed`, and still immutable. Two cases
qualify.

- **Polymorphic hierarchies behind an interface.** A generic base that carries an open type parameter
  and is substituted through its interface needs a reference type. So do the value records it nests.
- **Types that must satisfy a contract from the extensions package.** Where the package hands us a
  base class or an interface to implement, implement it the way the package expects, including when
  that means the type is held by reference as a cache key. Its *fields* are still our own record
  structs, not raw strings.

Reach for this deliberately, and prefer `sealed record` over `sealed class` so you keep value
equality. If you are only wrapping data, you almost certainly want a struct.

## What the rule does not touch

The rule is about **modelling the domain**, not about banning the `int` keyword. Leave alone:

- **Wire and library primitives we do not own.** The `ushort` a symbol-type field arrives as, the
  `ReadOnlyMemory<byte>` a tag read produces, the `byte` a CIP code travels as. Decode them *into*
  our types at the boundary rather than wrapping them in transit.
- **Local arithmetic and scratch.** Loop indices, buffer lengths, byte offsets, bitmasks inside a
  decoder. A number used for counting is a number, not a domain value.
- **Types from `ViciOne.Suite.DataPort.Extensions`.** Use them as given. The rule is for the types we
  introduce.

Ask whether the value names a domain concept that crosses one of our method boundaries. If it does,
it gets a type. If it is a number you are doing maths on inside a single method, it does not.

## A note on the examples

`SlotNumber` and `FirmwareRevision` are invented for this page and exist nowhere in the tree. That is
deliberate: a convention page that cites real types becomes wrong the first time one of them is
renamed, and the reader learns the tree's current contents instead of the rule. Add an example here
only where the rule is genuinely unclear without one, and invent it rather than borrowing it. See
[What belongs where](documentation-principles.md#what-belongs-where).
