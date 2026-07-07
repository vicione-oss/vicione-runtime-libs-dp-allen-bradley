# CIP / EtherNet/IP Protocol Background

PLC and protocol knowledge needed to understand the Allen-Bradley implementations. These
documents are **client-agnostic** — they describe the Common Industrial Protocol (CIP) and
its EtherNet/IP adaptation as administered by [ODVA](https://www.odva.org/), not how this
codebase implements it. Each document says so explicitly at the top.

| Document | Description |
|----------|-------------|
| [cip-networking-overview.md](cip-networking-overview.md) | Protocol landscape (CIP / EtherNet/IP / PCCC), the encapsulation wire stack (TCP 44818 / UDP 2222), session registration, the CIP object model and EPATH, connected vs. unconnected messaging, Forward Open and backplane routing, message-router request/reply format, security considerations |
| [cip-datatypes-reference.md](cip-datatypes-reference.md) | Source-of-truth wire formats for every CIP type: type codes, encoding (little-endian), byte layout, ranges, .NET equivalents, the Logix `STRING`/`TIMER` structures, the Logix symbol-type bitfield, and byte-offset examples |
| [cip-datatype-support-matrix.md](cip-datatype-support-matrix.md) | Which types exist per controller family (Logix, Micro800, legacy MicroLogix / SLC-500 / PLC-5); per-family programming tools, addressing model, and native protocol |

## How this maps to the addressing grammars

The **addressing grammars** (how a tree node maps to a tag string or data-file address) live
alongside the dataport definitions, not here:

| Grammar | Location |
|---------|----------|
| Logix symbolic addressing (`"MyTag"`, `"Motor"."Speed"`, `"Arr"[5]`) | [`../../../dataport-definition/Allen-Bradley Logix Adressierung.md`](../../../dataport-definition/Allen-Bradley%20Logix%20Adressierung.md) |
| Legacy PLC-5 / SLC / MicroLogix file addressing (`N7:0`, `T4:0.PRE`) and Micro800 | [`../../../dataport-definition/Allen-Bradley Legacy Adressierung.md`](../../../dataport-definition/Allen-Bradley%20Legacy%20Adressierung.md) |

This `cip-protocol/` folder covers the layer **below** addressing: what CIP and EtherNet/IP
are on the wire, and how the types those addresses point at are encoded. It is modelled on the
Siemens [`s7-protocol/`](../../../../../Siemens/S7/docs/Siemens.S7.Documentation/s7-protocol/README.md)
documentation set.

## The one fact to carry into every other document

**CIP is little-endian on the wire.** Every multi-byte value — the encapsulation header, the
message-router fields, and all tag data — is transmitted least-significant-byte-first. Because
.NET also runs little-endian, scalar CIP values need **no byte swapping** (the opposite of
big-endian PLC protocols such as Siemens S7comm and Modbus/TCP). This is called out again in
each document because it is the single most consequential difference from those protocols.
