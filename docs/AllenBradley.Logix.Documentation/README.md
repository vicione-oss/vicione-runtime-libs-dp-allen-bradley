# Allen-Bradley Logix DataPort

Documentation for the **Logix** Allen-Bradley DataPort — **symbolic tag addressing** (e.g.
`Motor.Speed`, `Arr[5]`) over **CIP / EtherNet/IP**. It targets the Logix controller family across
**all its generations** — **ControlLogix**, **CompactLogix**, **GuardLogix**, **SoftLogix**,
programmed in Studio 5000 Logix Designer — spanning both the classic type set and the extended one
the 5x80 controllers add.

**Micro800** (programmed in Connected Components Workbench) speaks the same symbolic tag protocol
but is **not in scope**: whether it lands here as a device family or in a port of its own is an open
decision, taken once Logix is done.

This implementation builds on the reusable `ViciOne.Suite.DataPort.Extensions` base classes, which
handle the generic data-port machinery (connection lifecycle, polling, write queuing, retry, value
validation, the typed-node framework). That machinery is **documented with the package**, not here.
These pages cover only what the Logix port adds on top.

> **Status.** The Logix DataPort is not built yet. This project is scaffolded; the Diátaxis folders
> below (`explanation/`, `how-to/`, `reference/`, `ADR/`) fill in as the implementation lands. Until
> then the authoritative material is the protocol background linked below.
>
> **Addressing is an open design question.** How a configuration-tree node maps to a Logix tag string
> is not specified anywhere in this repo, and no grammar should be assumed — it gets designed against
> a real controller as the port is built, and documented here under `reference/` when it is.

## New here?

- **Protocol background** — start with the
  [CIP / EtherNet/IP networking overview](../AllenBradley.Documentation/cip-protocol/cip-networking-overview.md)
  for the wire stack, session registration, the CIP object model and EPATH, and how a Logix tag read
  becomes a message-router request.
- **Data types** — the
  [CIP data types reference](../AllenBradley.Documentation/cip-protocol/cip-datatypes-reference.md)
  covers the wire formats, the Logix `STRING`/`TIMER` structures, and the symbol-type bitfield used
  when enumerating tags.
- **Test device** — a real CompactLogix L32E is available for integration testing; see
  [the test-device setup](../AllenBradley.Documentation/context/TEST-DEVICE-SETUP.md).

## Components

### Explanation

| Document | Covers |
|----------|--------|
| [`explanation/client/architecture.md`](explanation/client/architecture.md) | How the client classes collaborate — from the read/write seams down to the native `libplctag` handle (with diagram) |

### How-to *(planned)*

| Document | Covers |
|----------|--------|
| `how-to/connect-to-a-device.md` | Connecting to a Logix controller (gateway, backplane path, connection sizing) |
| `how-to/add-a-data-type.md` | Step-by-step recipe for adding a new Logix data-point type |

### Reference *(planned)*

| Document | Covers |
|----------|--------|
| `reference/datatype-support.md` | Implemented Logix types, wire/.NET mapping, per-family status, known issues |

### Decision records *(planned)*

Architecture decision records land under `ADR/` as design decisions are made.

---

Cross-port material — the protocol/PLC background, the test-device inventory, and process docs — lives
in the [AllenBradley.Documentation](../AllenBradley.Documentation/README.md) hub. These docs follow the
[Diátaxis](https://diataxis.fr/) framework; see the
[documentation principles](../AllenBradley.Documentation/documentation-principles.md) before adding to them.
