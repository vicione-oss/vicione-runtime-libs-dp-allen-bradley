# Allen-Bradley Logix DataPort

Documentation for the **Logix** Allen-Bradley DataPort — **symbolic tag addressing** (e.g.
`Motor.Speed`, `Arr[5]`) over **CIP / EtherNet/IP**. It targets the Logix controller family across
**all its generations** — **ControlLogix**, **CompactLogix**, **GuardLogix**, **SoftLogix**,
programmed in Studio 5000 Logix Designer — spanning both the classic type set and the extended one
the 5X80 controllers add.

**Micro800** (programmed in Connected Components Workbench) speaks the same symbolic tag protocol
but is **not in scope**: whether it lands here as a device family or in a port of its own is an open
decision, taken once Logix is done.

This implementation builds on the reusable `ViciOne.Suite.DataPort.Extensions` base classes, which
handle the generic data-port machinery (connection lifecycle, polling, write queuing, retry, value
validation, the typed-node framework). That machinery is **documented with the package**, not here.
These pages cover only what the Logix port adds on top.

> **Status.** The port reads and writes tags against a real controller, and is being built slice by
> slice — so these pages describe a moving target. `reference/` and `ADR/` track what has landed;
> `how-to/` is still scaffolding.
>
> **Addressing is a plain tag name today.** A configuration node carries the tag name the controller
> knows, and `TagNamePropertyValidator` accepts nothing more elaborate: no dotted structure members.
> Two container nodes compose an address from more than one node instead: an element node under an
> array container carries the subscript, `[3]`, and a member node under a UDT container carries the
> member name, and the lookup verifies the member against the tag's template at connect. What that
> rules out, and why it is a validation rule rather than a parser, is in
> [`reference/datatype-support.md`](reference/datatype-support.md).

## New here?

- **Protocol background** — start with the
  [CIP / EtherNet/IP networking overview](../AllenBradley.Documentation/protocol/cip/cip-networking-overview.md)
  for the wire stack, session registration, the CIP object model and EPATH, and how a Logix tag read
  becomes a message-router request.
- **Data types** —
  [Symbolic tag data types](../AllenBradley.Documentation/protocol/allen-bradley-extension/symbolic-tag-data-types.md)
  covers what Logix exposes, the `STRING`/`TIMER` structures, BOOL packing, and the symbol-type
  bitfield used when enumerating tags; the
  [CIP data types reference](../AllenBradley.Documentation/protocol/cip/cip-datatypes-reference.md)
  has the wire encoding of each type.
- **Test devices** — a borrowed CompactLogix L32E, and our own CompactLogix 5069-L306ER once it is
  commissioned; see [the test-device setup](../AllenBradley.Documentation/context/TEST-DEVICE-SETUP.md).

## Components

### Explanation

| Document | Covers |
|----------|--------|
| [`explanation/model/node-model.md`](explanation/model/node-model.md) | The configured tree: the three kinds of node, why the marker interfaces exist, what each container accepts as a child, and why there is one record per data type |
| [`explanation/client/architecture.md`](explanation/client/architecture.md) | The entry point to the client architecture: the overview diagram, and one linked page per axis (reading and writing, values, connecting, tags and handles, the symbol table, verification) |

### How-to *(planned)*

| Document | Covers |
|----------|--------|
| `how-to/connect-to-a-device.md` | Connecting to a Logix controller (gateway, backplane path, connection sizing) |
| `how-to/add-a-data-type.md` | Step-by-step recipe for adding a new Logix data-point type |

### Reference

| Document | Covers |
|----------|--------|
| [`reference/datatype-support.md`](reference/datatype-support.md) | Which Logix types the port implements, their .NET mapping and wire size, and what is not supported yet |

### Tree-editor icons

| Document | Covers |
|----------|--------|
| [`logix-icons/README.md`](logix-icons/README.md) | The SVG sources of the manifest's icons, the Studio 5000 shapes they follow, and the script that embeds them in the YAML |

### Decision records

Architecture decision records land under [`ADR/`](ADR/) as design decisions are made.

---

Cross-port material — the protocol/PLC background, the test-device inventory, and process docs — lives
in the [AllenBradley.Documentation](../AllenBradley.Documentation/README.md) hub. These docs follow the
[Diátaxis](https://diataxis.fr/) framework; see the
[documentation principles](../AllenBradley.Documentation/documentation-principles.md) before adding to them.
