# Protocol Background

PLC and protocol knowledge needed to understand the Allen-Bradley implementations. These
documents are **client-agnostic** — they describe what is on the wire, not how this codebase
implements it. Each document says so explicitly at the top.

The folder is split by **who defines the mechanism**:

| Folder | Defined by | Holds |
|--------|-----------|-------|
| [`cip/`](cip/) | [ODVA](https://www.odva.org/), in the *CIP Networks Library* | The standard every EtherNet/IP device shares: the wire stack, the object model and services, the data-type encodings |
| [`allen-bradley-extension/`](allen-bradley-extension/) | Rockwell Automation | What Rockwell builds on top of that standard: the Symbol and Template objects, the tag services, the PCCC tunnel, the controller lines and their route paths |

Nothing in `cip/` mentions a Rockwell object or a Rockwell controller except to point into the
extension folder. Everything in `allen-bradley-extension/` marks the standard pieces it reuses with
*(std)* and links back into `cip/`. Reading `cip/` alone should leave you able to talk to any
EtherNet/IP device; reading the extension is what makes a Logix tag readable.

## `cip/` — the standard

| Document | Description |
|----------|-------------|
| [cip-networking-overview.md](cip/cip-networking-overview.md) | The encapsulation wire stack (TCP 44818 / UDP 2222), session registration, CPF, the object model and its class-id ranges, services and their code ranges, EPATH, what discovery standard CIP does and does not have, connected vs. unconnected messaging, Forward Open and route paths, the message-router request/reply format, security |
| [cip-datatypes-reference.md](cip/cip-datatypes-reference.md) | Source-of-truth wire formats for every CIP type: type codes, encoding (little-endian), byte layout, ranges, .NET equivalents, and byte-offset examples |

## `allen-bradley-extension/` — what Rockwell adds

| Document | Description |
|----------|-------------|
| [symbolic-tag-data-types.md](allen-bradley-extension/symbolic-tag-data-types.md) | Which types the **tag-addressed** families expose (Logix classic and 5X80, Micro800), the Logix `STRING`/`TIMER`/`COUNTER` structures, BOOL packing, the tag services and the structure reply, and the Symbol and Template objects with the services that list and describe every tag |
| [pccc-data-file-types.md](allen-bradley-extension/pccc-data-file-types.md) | Which types the **file-addressed** families expose (MicroLogix, SLC 500, PLC-5): file letters and element sizes, Timer/Counter/Control layouts, the `ST` string element, what these families lack, and the PCCC tunnel that carries their commands inside CIP |
| [tag-scoping.md](allen-bradley-extension/tag-scoping.md) | Where a Logix tag lives: controller scope and program scope, what the `Program:` prefix adds to an address, shadowing, which tags the firmware keeps in controller scope, and why a full listing is one pass per scope |
| [controller-families-and-routing.md](allen-bradley-extension/controller-families-and-routing.md) | The controller lines and their two form factors, their programming tools and access services, what chassis / slot / backplane mean, why the backplane is a CIP network, how a route path is built hop by hop, and the conventional path per family |

## What this folder is not

There is no addressing grammar in the repo — how a configuration-tree node maps to a Logix tag
string (`"MyTag"`, `"Motor"."Speed"`, `"Arr"[5]`) is a design question for the DataPort, and it
will be settled against a real controller as the port is built.

This `protocol/` folder covers the layer **below** addressing: what CIP and EtherNet/IP are on the
wire, what Rockwell adds, and how the types those addresses point at are encoded. What the
`libplctag` dependency makes of all this — its pseudo-tags, its buffers, its session sharing — is
one layer up, in [`../libPlcTag/`](../libPlcTag/README.md). The folder is modelled on the Siemens
[`s7-protocol/`](../../../../../Siemens/S7/docs/Siemens.S7.Documentation/s7-protocol/README.md)
documentation set.

## The one fact to carry into every other document

**CIP is little-endian on the wire.** Every multi-byte value — the encapsulation header, the
message-router fields, and all tag data — is transmitted least-significant-byte-first. Because
.NET also runs little-endian, scalar CIP values need **no byte swapping** (the opposite of
big-endian PLC protocols such as Siemens S7comm and Modbus/TCP). This is called out again in
each document because it is the single most consequential difference from those protocols.
