# Allen-Bradley Legacy DataPort

Documentation for the **Legacy** Allen-Bradley DataPort — **file / data-table addressing** (e.g.
`N7:0`, `T4:0.PRE`, `I:1.0/7`) over **PCCC tunneled inside EtherNet/IP**. It targets the classic
controller families: **PLC-5** (programmed in RSLogix 5) and **SLC 500** / **MicroLogix** (programmed
in RSLogix 500).

Unlike the Logix port, these controllers have no symbolic tag database on the wire: addresses name a
**data file** (`N` integer, `B` binary, `F` float, `T` timer, `C` counter, `ST` string, …), a file
number, and an element, with optional field and bit suffixes. The request is carried as a **PCCC**
command wrapped in a CIP unconnected-send; see the protocol background for the encapsulation details.

This implementation builds on the reusable `ViciOne.Suite.DataPort.Extensions` base classes, which
handle the generic data-port machinery (connection lifecycle, polling, write queuing, retry, value
validation, the typed-node framework). That machinery is **documented with the package**, not here.
These pages cover only what the Legacy port adds on top.

> **Status.** Nothing here is planned work. The roadmap builds the **Logix** port and nothing else;
> whether the legacy families ever get a DataPort — and whether it lives here, in a separate port, or
> in a separate repository — is an open decision with no owner yet. This project is scaffolded and the
> content below is **protocol background only**. There is no addressing grammar in the repo; the
> Diátaxis folders (`explanation/`, `how-to/`, `reference/`, `ADR/`) fill in only if the port is built.

## New here?

- **Protocol background** — the
  [CIP / EtherNet/IP networking overview](../AllenBradley.Documentation/cip-protocol/cip-networking-overview.md)
  explains how PCCC is tunneled over EtherNet/IP and how a data-file read is framed on the wire.
- **Data types** — the
  [CIP data-type support matrix](../AllenBradley.Documentation/cip-protocol/cip-datatype-support-matrix.md)
  lists which types each legacy family exposes and how PCCC file types map to them.
- **Test device** — the integration test rig currently targets a Logix controller; see
  [the test-device setup](../AllenBradley.Documentation/context/TEST-DEVICE-SETUP.md) for the
  connection model (a legacy device would be reached the same way, through the Link Manager tunnel).

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

Cross-port material — the protocol/PLC background, the test-device inventory, and process docs — lives
in the [AllenBradley.Documentation](../AllenBradley.Documentation/README.md) hub. These docs follow the
[Diátaxis](https://diataxis.fr/) framework; see the
[documentation principles](../AllenBradley.Documentation/documentation-principles.md) before adding to them.
