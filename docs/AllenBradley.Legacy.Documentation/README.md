# Allen-Bradley Legacy DataPort

Documentation for the Legacy Allen-Bradley DataPort, which does file / data-table addressing
(`N7:0`, `T4:0.PRE`, `I:1.0/7`) over PCCC tunneled inside EtherNet/IP. It targets the classic
controller families: PLC-5, programmed in RSLogix 5, and SLC 500 and MicroLogix, programmed in
RSLogix 500.

Unlike the Logix port, these controllers have no symbolic tag database on the wire. An address names
a data file (`N` integer, `B` binary, `F` float, `T` timer, `C` counter, `ST` string, and so on), a
file number, and an element, with optional field and bit suffixes. The request is carried as a PCCC
command wrapped in a CIP unconnected-send. The protocol background has the encapsulation details.

This implementation builds on the reusable `ViciOne.Suite.DataPort.Extensions` base classes, which
handle the generic data-port machinery: connection lifecycle, polling, write queuing, retry, value
validation, the typed-node framework. That machinery is documented with the package, not here. These
pages cover only what the Legacy port adds on top.

> **Status.** Planned, not started. The port will be a second project in this repo, beside the
> Logix port, and the two are versioned and published together. The content below is protocol
> background only, and there is no addressing grammar in the repo yet. The Diátaxis folders
> (`explanation/`, `how-to/`, `reference/`, `ADR/`) fill in as the port is built.

## Which controllers are in scope

The port covers the families that store their data in data files and are addressed through PCCC:

| Family | Addressing | In scope |
|--------|------------|----------|
| **PLC-5**, including the Ethernet `/xxE` models | `N7:0`, octal I/O (`I:012/07`) | Yes |
| **SLC 500** (5/01 to 5/05) | `N7:0`, slot-based I/O (`I:1/3`) | Yes |
| **MicroLogix** (1000, 1100, 1200, 1400, 1500) | Same as SLC 500 | Yes |
| **PLC-2** | Octal word addresses (`010/07`), no data files | No |
| **PLC-3**, **PLC-5/250** | Older, rare | No |

PLC-5 and SLC 500 share the file notation but not the I/O notation, so how an address is checked
depends on the family.

The PLC-5/xxE, the SLC 5/05 and the MicroLogix 1100 and 1400 have Ethernet of their own, and they
still speak PCCC with data files. Every other model is reached over DF1 serial, DH+ or DH-485. The
port can reach those only through a bridge, because libplctag speaks nothing but EtherNet/IP.

Two things look close and are not part of this port. Micro800 (810 to 870) is tag-based. Reading by
tag name works, but it is a separate and simpler platform, and browsing and structure handling do not
follow the Logix model. And a Logix controller can answer a legacy address such as `N7:0` when it is
configured to map PLC/SLC messages. That is a compatibility option for old HMIs and devices, and
supporting it is not a requirement.

## New here?

[PCCC](../AllenBradley.Documentation/protocol/allen-bradley-extension/pccc.md) explains how PCCC is
tunneled over EtherNet/IP and how a data-file read is framed on the wire.
[PCCC data-file types](../AllenBradley.Documentation/protocol/allen-bradley-extension/pccc-data-file-types.md)
lists which types each legacy family exposes, how the file letters map to CIP and .NET types, and
how Timer, Counter, and string elements are laid out.

Most of these families have no Ethernet of their own. How a request reaches them through a bridge is
in [chassis and route paths](../AllenBradley.Documentation/controllers/chassis-and-route-paths.md#reaching-a-legacy-controller-through-a-bridge).

The integration test rig currently targets a Logix controller. See
[the test-device setup](../AllenBradley.Documentation/test-bench/test-device-setup.md) for the
connection model. A legacy device would be reached the same way, through the Link Manager tunnel.

## Components

### Explanation *(planned)*

| Document | Covers |
|----------|--------|
| `explanation/architecture.md` | How the CIP/PCCC client, typed nodes, and the incoming/outgoing ports fit together |

### How-to *(planned)*

| Document | Covers |
|----------|--------|
| `how-to/connect-to-a-device.md` | Connecting to a PLC-5 / SLC / MicroLogix (gateway, routing) |
| `how-to/add-a-data-type.md` | Step-by-step recipe for adding a new data-file type |

### Reference *(planned)*

| Document | Covers |
|----------|--------|
| `reference/datatype-support.md` | Implemented PCCC file types, wire/.NET mapping, per-family status, known issues |

### Decision records *(planned)*

Architecture decision records land under `ADR/` as design decisions are made.

---

Cross-port material (the controllers, the protocol background, libplctag behaviour and the test
bench) lives in the [AllenBradley.Documentation](../AllenBradley.Documentation/README.md) hub. These
docs follow the [Diátaxis](https://diataxis.fr/) framework. Read the
[documentation principles](../AllenBradley.Documentation/conventions/documentation-principles.md)
before adding to them.
