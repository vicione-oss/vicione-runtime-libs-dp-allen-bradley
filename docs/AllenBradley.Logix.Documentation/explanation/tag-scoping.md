# Tag scoping — controller scope and program scope

A Logix controller has no address space to point at. There is no `DB100.DBW20` equivalent, because
memory layout is not exposed on the wire and does not survive a download. A tag *name* is the
address, resolved against the controller's symbol table on every request.

Names are not one flat namespace, though. Every tag lives in one of two scopes, and only one of them
contributes a segment to the address.

## The two scopes

**Controller scope** is a single global namespace. A tag in it is addressed by its bare name:
`Count`, `Motor_Speed`, `Tank.Level`. Every routine in every program can reference it, and so can
every external client.

**Program scope** is per-program and prefixes the address with the program that owns it:

```text
Program:MainProgram.Count
```

That is the entire hierarchy on the wire. Programs do not nest for addressing purposes, and routines
have no tags of their own. Add-On Instruction locals are not reachable from outside at all — only the
AOI's instance tag is, and that tag sits in one of the two scopes like anything else.

Within the controller, program scope really is private: no other program can reference a program tag,
and there is no import or cross-program alias. It is not a boundary on the wire, however. An external
CIP client reads and writes program tags exactly as it does controller tags, given the prefix.

### Shadowing

When a program tag and a controller tag share a name, the program tag wins inside that program. The
shadowing is silent, and it catches people who read `Count` in ladder logic and assume it is the
global they configured.

## Some tags cannot be program-scoped

The firmware requires controller scope for:

- **Produced and consumed tags** — the controller-to-controller sharing mechanism
- **Module tags** — the I/O structures Studio 5000 creates when you add a module, and anything
  aliased to one
- **Motion axis and motion group tags**

In practice this is why most of what an external client wants is controller-scoped already.

On GuardLogix, safety tags are controller-scoped as well, and a standard external client can read but
never write them.

## Enumeration takes two passes

Controller scope is one listing. Program scope is one listing *per program*, and the programs
themselves are discovered from the controller listing:

1. Read `@tags`. Entries whose name begins `Program:` are programs, not tags. They also carry the
   system bit (`0x1000`) in their
   [symbol type](../../AllenBradley.Documentation/protocol/allen-bradley-extension/symbolic-tag-data-types.md#the-logix-symbol-type-bitfield),
   but the name prefix is the discriminator worth relying on.
2. For each of those, read `Program:<name>.@tags` to get that program's tags.

Neither listing descends into structures. A member such as `Program:MainProgram.Counter.PRE` is
readable and absent from both, which the
[data-type support reference](../reference/datatype-support.md) covers alongside what that costs the
verifier.

> **Open.** Studio 5000 v32 and later allow programs to be nested inside other programs. Whether the
> resulting tags address as `Program:Parent.Child.Tag` has not been confirmed against hardware, and the
> bench L32E is too old to nest. The port offers flat programs only until it has been.

## What this means for the configuration tree

Scope is configuration; structure nesting is discovery. The two look alike in an editor tree and
behave nothing alike, and keeping them apart is what the node model is doing.

Each scope is a container hanging off the device, and the two are peers — the way Studio 5000's own
tree puts them, and not a program folder nested inside controller scope. `ControllerTagsNode` carries
no property that reaches an address, because controller scope contributes no segment: a tag
configured under it addresses itself. `ProgramTagsNode` is the one that prefixes, and its
`ProgramName` is what it prefixes with. A UDT container is the other kind of prefix: it carries a tag
name, and a member node under it carries the member's name, so `MyMotor.Speed` is two nodes. What the
members *are* is not configured anywhere; it comes from the tag's own template, read from the
controller.

The prefix is composed while the tree is walked into data points, never stored on the leaf. The
tree is three levels deep and no deeper — a scope container, an array container under it, tag nodes
under either — so `LogixDataPointsGroupsMapper` walks those three levels by name and builds a
`TagPath` for each leaf: the program from the scope (none for controller scope), the tag, and the
subscript. The data point carries that path and renders `Program:MainProgram.Count` from it when
libplctag asks. So a tag is configured as `Count` wherever it sits, and the address exists only from
the data point outwards. An element under an array container is the same walk with the third slot
filled: its tag name is the subscript, the array's name is the tag, and
`Program:MainProgram.Readings[3]` renders from the three.

The ordering consequence for verification is worth stating plainly. A program's tag listing cannot be
read until its name is known, so `ProgramName` is validated as a well-formed name during mapping, and
only then used to browse. Verification against the symbol table confirms the tags inside a program; it
cannot be what discovers the program.
