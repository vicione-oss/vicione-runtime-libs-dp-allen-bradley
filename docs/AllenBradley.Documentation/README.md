# Allen-Bradley DataPorts — Documentation

Documentation for the Allen-Bradley DataPorts. It follows the [Diátaxis](https://diataxis.fr/)
framework and is modelled on the docs that ship with the `ViciOne.Suite.DataPort.Extensions`
package — it documents **only what is specific to the Allen-Bradley implementations** and the
context needed to understand them, and links out to the package for the shared machinery rather
than repeating it. See [documentation-principles.md](documentation-principles.md) for how the docs
are organised and the rules for adding to them.

The `docs/` tree mirrors the eventual `src/` and `tests/`: each documentation project lives in its
own folder. This project, **AllenBradley.Documentation**, holds the cross-port material; port-specific
docs live in the sibling **AllenBradley.Logix.Documentation** and **AllenBradley.Legacy.Documentation**
projects, each organised into `explanation/`, `how-to/`, and `reference/`.

> **Status.** The Allen-Bradley DataPort implementation is not built yet. The protocol background
> (`cip-protocol/`), the addressing grammars, and the test-device setup are complete; the per-port
> `explanation/`/`how-to/`/`reference/` docs are scaffolded and fill in as the implementation lands.

## The two ports

Controls engineers pick a port by **controller family**, not by protocol — the same way they choose a
driver in Studio 5000, Kepware, or Ignition. The two ports split by addressing paradigm:

| Port        | Controllers                                             | Addressing                              | Programmed in                         | Start here                                                                 |
|-------------|---------------------------------------------------------|-----------------------------------------|---------------------------------------|----------------------------------------------------------------------------|
| **Logix**   | ControlLogix, CompactLogix, GuardLogix, Micro800        | Symbolic tags (`Motor.Speed`, `Arr[5]`) | Studio 5000 · Connected Components Workbench | [AllenBradley.Logix.Documentation](../AllenBradley.Logix.Documentation/README.md)   |
| **Legacy**  | PLC-5, SLC 500, MicroLogix                              | File / data-table (`N7:0`, `T4:0.PRE`) over PCCC | RSLogix 5 · RSLogix 500        | [AllenBradley.Legacy.Documentation](../AllenBradley.Legacy.Documentation/README.md) |

Both ports speak **CIP over EtherNet/IP** on the wire; the Legacy port tunnels **PCCC** inside it. The
protocol is documented once, below, and shared by both ports.

## Cross-port material (this project)

### CIP / EtherNet/IP protocol background — `cip-protocol/`

PLC and protocol knowledge needed to understand the implementations. These docs are
**client-agnostic** — they describe CIP and its EtherNet/IP adaptation as administered by
[ODVA](https://www.odva.org/), not how this codebase implements it.

| Document | Description |
|----------|-------------|
| [cip-protocol/](cip-protocol/README.md) | Index of all CIP background docs |
| [cip-protocol/cip-networking-overview.md](cip-protocol/cip-networking-overview.md) | Protocol landscape, encapsulation wire stack (TCP 44818 / UDP 2222), session registration, the CIP object model and EPATH, connected vs. unconnected messaging, Forward Open and backplane routing, PCCC tunneling, security |
| [cip-protocol/cip-datatypes-reference.md](cip-protocol/cip-datatypes-reference.md) | Source-of-truth wire formats for every CIP type: type codes, little-endian encoding, ranges, .NET equivalents, the Logix `STRING`/`TIMER` structures, and the Logix symbol-type bitfield |
| [cip-protocol/cip-datatype-support-matrix.md](cip-protocol/cip-datatype-support-matrix.md) | Which types exist per controller family (Logix, Micro800, legacy MicroLogix / SLC-500 / PLC-5) |

### Addressing grammars — `dataport-definition/`

How a tree node maps to a tag string or data-file address. These live alongside the dataport
definitions, not here — one grammar per port:

| Grammar | Location |
|---------|----------|
| Logix symbolic addressing (`"MyTag"`, `"Motor"."Speed"`, `"Arr"[5]`) | [`../../dataport-definition/Allen-Bradley Logix Adressierung.md`](../../dataport-definition/Allen-Bradley%20Logix%20Adressierung.md) |
| Legacy PLC-5 / SLC / MicroLogix file addressing (`N7:0`, `T4:0.PRE`) and Micro800 | [`../../dataport-definition/Allen-Bradley Legacy Adressierung.md`](../../dataport-definition/Allen-Bradley%20Legacy%20Adressierung.md) |

### Implementation context — `context/`

Background material specific to this implementation: the test devices available for integration
testing.

| Document | Description |
|----------|-------------|
| [context/TEST-DEVICE-SETUP.md](context/TEST-DEVICE-SETUP.md) | The Allen-Bradley CompactLogix L32E used for E2E testing: GateManager access, IP/backplane path, available tags, and how to run the tests |

## Developer process

| Document | Description |
|----------|-------------|
| [documentation-principles.md](documentation-principles.md) | How these docs are organised; Diátaxis + the no-duplication rule |

## Contributing to documentation

When adding a document, place it in the matching Diátaxis folder of the relevant port project
(`explanation/`, `how-to/`, `reference/`, or `ADR/`), or in this hub if it is cross-port, add it to
that project's README index, and **do not duplicate the extensions docs** — link to them instead.
Files appear in the IDE automatically: each documentation project surfaces its own folder tree, so a
new file shows up on reload without editing the `.csproj`.
