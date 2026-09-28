# Logix Generations: 5X70 and 5X80

Inside the Logix line there is a second split, next to the family, that decides which data types a
controller can declare. This repo calls it the generation, and it has two values: 5X70 and earlier,
and 5X80. This page says what is behind the word.

## Where the name comes from

Rockwell numbers each Logix product line with four digits. The first digit is always 5, the second
names the family, and the last two name the step along the line:

| Family       | 5X70 and earlier                       | 5X80                          |
|--------------|----------------------------------------|-------------------------------|
| ControlLogix | 5550, 5555, 5560, **5570** (`1756-L7x`) | **5580** (`1756-L8x`)         |
| CompactLogix | 1769, **5370** (`1769-Lxx`)             | **5380** (`5069-L3xx`), **5480** (`5069-L4xx`) |

The `X` stands in for the family digit. A ControlLogix 5580 and a CompactLogix 5380 are both 5X80,
and they share the property this split is about. Everything older than the `x80` step is folded into
"5X70 and earlier", because for the purpose of this repo nothing separates a 5570 from a 5560.

The generation cuts across the family. Family says how the hardware is shaped and which
communication path reaches it. Generation says what the controller can hold. A device is fully
described by the two together, which is what [`CONTEXT.md`](../../../CONTEXT.md) calls the
controller kind.

## What the split decides: the type vocabulary

The 5X80 controllers introduced the extended data types, meaning the unsigned integers `USINT`,
`UINT`, `UDINT`, `ULINT`, and the double-precision `LREAL`. A 5X70 controller cannot declare any of
them, and Studio 5000 will not offer them for such a project. Everything else in the atomic
vocabulary is the same on both: `BOOL`, `SINT`, `INT`, `DINT`, `LINT`, `REAL`, `STRING` and the
structures.

The full per-type table is the
[availability matrix](../protocol/allen-bradley-extension/symbolic-tag-data-types.md#availability-matrix).
Whether the port implements a type, and which generation each implemented type needs, is a question
for the Logix port's own reference section.

Two caveats travel with the split. The extended types arrived with a firmware revision and a Studio
5000 version, so for a given catalog number and revision the programming tool is the arbiter of what
can be declared. And Micro800 has no generation. It is symbolic like Logix but a separate line with
its own type set, and it is outside the Logix port; see
[controller families](controller-families.md).

## Where the generation appears in the port

The generation is a first-class concept in the Logix port, not a detail of the type table.

A device node in the configuration tree stands for one family and one generation, and its tag-scope
containers only offer the types that generation has. The decision and the alternatives it rejected
are in
[splitting the device node by family and generation](../../AllenBradley.Logix.Documentation/ADR/2026-08-31-splitting-the-device-node-by-family-and-generation.md).

A data point node states the oldest generation whose vocabulary includes its type. A type that
arrives with a later generation is therefore one declaration on the node and no edit to any
container. How that gate works is in the Logix port's
[data type support](../../AllenBradley.Logix.Documentation/reference/datatype-support.md#types-the-5x70-controllers-have-not-got)
reference.

The test bench has one controller of each generation. The borrowed CompactLogix L32E is a 1769 and
therefore 5X70. Our own CompactLogix 5069-L306ER is a 5380 and therefore 5X80, and the per-type
round-trip suite has to run on it for exactly this reason. See
[the test-device setup](../test-bench/test-device-setup.md).

## References

- Rockwell Automation, *Logix 5000 Controllers Data Access* (1756-PM020), on the data types each
  controller reports:
  <https://literature.rockwellautomation.com/idc/groups/literature/documents/pm/1756-pm020_-en-p.pdf>
- Rockwell Automation, *Logix 5000 Controllers I/O and Tag Data* (1756-PM004), on declaring tags and
  the extended data types:
  <https://literature.rockwellautomation.com/idc/groups/literature/documents/pm/1756-pm004_-en-p.pdf>
