# Allen-Bradley Logix DataPort

Documentation for the **Logix** Allen-Bradley DataPort — **symbolic tag addressing** (e.g.
`Motor.Speed`, `Arr[5]`) over **CIP / EtherNet/IP**. It targets the tag-based controller families:
**ControlLogix**, **CompactLogix**, **GuardLogix** (programmed in Studio 5000 Logix Designer) and
**Micro800** (programmed in Connected Components Workbench).

This implementation builds on the reusable `ViciOne.Suite.DataPort.Extensions` base classes, which
handle the generic data-port machinery (connection lifecycle, polling, write queuing, retry, value
validation, the typed-node framework). That machinery is **documented with the package**, not here.
These pages cover only what the Logix port adds on top.

> **Status.** The Logix DataPort is not built yet. This project is scaffolded; the Diátaxis folders
> below (`explanation/`, `how-to/`, `reference/`, `ADR/`) fill in as the implementation lands. Until
> then the authoritative material is the addressing grammar and the protocol background linked below.

## New here?

- **Addressing** — how a tree node maps to a Logix tag string is specified in
  [`Allen-Bradley Logix Adressierung.md`](../../dataport-definition/Allen-Bradley%20Logix%20Adressierung.md)
  (EBNF grammar, structs/AOIs, arrays, bit suffixes). Micro800's tag grammar is documented in the
  Micro800 section of the
  [Legacy addressing doc](../../dataport-definition/Allen-Bradley%20Legacy%20Adressierung.md).
- **Protocol background** — start with the
  [CIP / EtherNet/IP networking overview](../AllenBradley.Documentation/cip-protocol/cip-networking-overview.md)
  for the wire stack, session registration, the CIP object model and EPATH, and how a Logix tag read
  becomes a message-router request.
- **Data types** — the
  [CIP data types reference](../AllenBradley.Documentation/cip-protocol/cip-datatypes-reference.md)
  covers the wire formats, the Logix `STRING`/`TIMER` structures, and the symbol-type bitfield used
  when enumerating tags.
- **Test device** — a real CompactLogix L32E is available for E2E testing; see
  [the test-device setup](../AllenBradley.Documentation/context/TEST-DEVICE-SETUP.md).

## Components

### Explanation *(planned)*

| Document | Covers |
|----------|--------|
| `explanation/architecture.md` | How the CIP client, typed nodes, and the incoming/outgoing ports fit together |

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
