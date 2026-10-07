# Protocol Background

Protocol knowledge needed to understand the Allen-Bradley implementations. These documents are
client-agnostic. They describe what is on the wire, not how this codebase implements it, and each
one says so at the top.

The folder is split by who defines the mechanism:

| Folder | Defined by | Holds |
|--------|-----------|-------|
| [`cip/`](cip/) | [ODVA](https://www.odva.org/), in the *CIP Networks Library* | The standard every EtherNet/IP device shares: the wire stack, the object model and services, the data-type encodings |
| [`allen-bradley-extension/`](allen-bradley-extension/) | Rockwell Automation | What Rockwell builds on top of that standard: the tag services, the Symbol and Template objects, the type vocabularies, the PCCC tunnel |

Nothing in `cip/` mentions a Rockwell object or a Rockwell controller except to point into the
extension folder. Everything in `allen-bradley-extension/` marks the standard pieces it reuses with
*(std)* and links back into `cip/`. Reading `cip/` alone should leave you able to talk to any
EtherNet/IP device. Reading the extension is what makes a Logix tag readable.

## `cip/`, the standard

| Document | Description |
|----------|-------------|
| [networking-overview.md](cip/networking-overview.md) | The encapsulation wire stack (TCP 44818 / UDP 2222), session registration, CPF, the object model and its class-id ranges, services and their code ranges, EPATH, what discovery standard CIP does and does not have, connected vs. unconnected messaging, Forward Open and route paths, the message-router request/reply format, security |
| [data-types.md](cip/data-types.md) | Source-of-truth wire formats for every CIP type: type codes, encoding (little-endian), byte layout, ranges, .NET equivalents, and byte-offset examples |

## `allen-bradley-extension/`, what Rockwell adds

Read them in this order. Each one assumes the one before it.

| Document | Description |
|----------|-------------|
| [tag-services.md](allen-bradley-extension/tag-services.md) | The Read Tag / Write Tag services and their fragmented variants, the Multiple Service Packet, symbolic vs. Symbol Instance addressing, the type prefix on a reply, and the structure reply with its handle |
| [symbolic-tag-data-types.md](allen-bradley-extension/symbolic-tag-data-types.md) | Which types the tag-addressed families expose (Logix by generation, Micro800), the Logix `STRING`/`TIMER`/`COUNTER` structures, array layout, and BOOL packing |
| [tag-browsing.md](allen-bradley-extension/tag-browsing.md) | The Symbol and Template objects, service `0x55` and its paging, the symbol-type bitfield, reading a template member by member, and the browse flow |
| [tag-scoping.md](allen-bradley-extension/tag-scoping.md) | Where a Logix tag lives: controller scope and program scope, what the `Program:` prefix adds to an address, shadowing, which tags the firmware keeps in controller scope, and why a full listing is one pass per scope |
| [pccc.md](allen-bradley-extension/pccc.md) | How the file-addressed families are reached: the PCCC object and its Execute PCCC service, the requestor ID and CMD/FNC command bytes, and bridging to controllers without Ethernet |
| [pccc-data-file-types.md](allen-bradley-extension/pccc-data-file-types.md) | Which types the file-addressed families expose (MicroLogix, SLC 500, PLC-5): file letters and element sizes, Timer/Counter/Control layouts, the `ST` string element, and what these families lack |

## What this folder is not

The hardware is not here. Which controller is which line, how a chassis is built, and why a route
path reads `1,0` are in [`../controllers/`](../controllers/). This folder only encodes the route
path once the hardware has been described.

There is no addressing grammar in the repo either. How a configuration-tree node maps to a Logix
tag string (`"MyTag"`, `"Motor"."Speed"`, `"Arr"[5]`) is a design question for the DataPort, and it
gets settled against a real controller as the port is built.

What the `libplctag` dependency makes of all this, with its pseudo-tags, its buffers and its session
sharing, is one layer up in [`../libplctag/`](../libplctag/README.md). The folder is modelled on the
Siemens
[`s7-protocol/`](../../../../../Siemens/S7/docs/Siemens.S7.Documentation/s7-protocol/README.md)
documentation set.

## The one fact to carry into every other document

CIP is little-endian on the wire. Every multi-byte value, from the encapsulation header through the
message-router fields to the tag data, is transmitted least-significant-byte-first. Because .NET
also runs little-endian, scalar CIP values need no byte swapping. That is the opposite of big-endian
PLC protocols such as Siemens S7comm and Modbus/TCP, and it is the single most consequential
difference from them, which is why each document repeats it.
