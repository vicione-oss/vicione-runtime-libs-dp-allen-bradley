# Allen-Bradley DataPorts — Documentation

Documentation for the Allen-Bradley DataPorts. It follows the [Diátaxis](https://diataxis.fr/)
framework and is modelled on the docs that ship with the `ViciOne.Suite.DataPort.Extensions`
package — it documents **only what is specific to the Allen-Bradley implementations** and the
context needed to understand them, and links out to the package for the shared machinery rather
than repeating it. See [documentation-principles.md](documentation-principles.md) for how the docs
are organised and the rules for adding to them.

The words these docs use are not chosen freely: [`CONTEXT.md`](../../CONTEXT.md) at the repo root is
the glossary, and it is the authority when Rockwell, ODVA and libplctag disagree about what to call
something — which they often do.

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
| **Logix**   | ControlLogix, CompactLogix, GuardLogix, SoftLogix       | Symbolic tags (`Motor.Speed`, `Arr[5]`) | Studio 5000 Logix Designer            | [AllenBradley.Logix.Documentation](../AllenBradley.Logix.Documentation/README.md)   |
| **Legacy**  | PLC-5, SLC 500, MicroLogix                              | File / data-table (`N7:0`, `T4:0.PRE`) over PCCC | RSLogix 5 · RSLogix 500        | [AllenBradley.Legacy.Documentation](../AllenBradley.Legacy.Documentation/README.md) |

Both ports speak **CIP over EtherNet/IP** on the wire; the Legacy port tunnels **PCCC** inside it. The
protocol is documented once, below, and shared by both ports.

**Micro800** belongs to neither yet. It addresses tags symbolically rather than by data file, so it is
not a Legacy controller, but it has no program scope and no tag listing, so it is not a drop-in Logix
device either. Whether it lands in the Logix port as a device family or in a port of its own is an open
decision, taken once Logix is done. The controller lines and what separates them are covered in
[controller families and routing](cip-protocol/controller-families-and-routing.md).

## Cross-port material (this project)

### CIP / EtherNet/IP protocol background — `cip-protocol/`

PLC and protocol knowledge needed to understand the implementations. These docs are
**client-agnostic** — they describe CIP and its EtherNet/IP adaptation as administered by
[ODVA](https://www.odva.org/), not how this codebase implements it.

| Document | Description |
|----------|-------------|
| [cip-protocol/](cip-protocol/README.md) | Index of all CIP background docs |
| [cip-protocol/cip-networking-overview.md](cip-protocol/cip-networking-overview.md) | Protocol landscape, encapsulation wire stack (TCP 44818 / UDP 2222), session registration, the CIP object model and EPATH, connected vs. unconnected messaging, Forward Open and backplane routing, PCCC tunneling, security |
| [cip-protocol/cip-datatypes-reference.md](cip-protocol/cip-datatypes-reference.md) | Source-of-truth wire formats for every CIP type: type codes, little-endian encoding, byte layout, ranges, and .NET equivalents |
| [cip-protocol/symbolic-tag-data-types.md](cip-protocol/symbolic-tag-data-types.md) | The types the tag-addressed families expose (Logix, Micro800), the Logix `STRING`/`TIMER` structures, BOOL packing, the symbol-type bitfield, and the `@tags` listing entry |
| [cip-protocol/pccc-data-file-types.md](cip-protocol/pccc-data-file-types.md) | The data-file types of the file-addressed families (MicroLogix, SLC 500, PLC-5): file letters, element layouts, and what these families lack |
| [cip-protocol/controller-families-and-routing.md](cip-protocol/controller-families-and-routing.md) | The controller lines and their two form factors, chassis / slot / backplane, the backplane as a CIP network, building a route path hop by hop, and the conventional path per family |

### libplctag behaviour — `libPlcTag/`

How the [libplctag](https://github.com/libplctag/libplctag) native library and its .NET wrapper
behave — the facts about our dependency that shape the client design, sitting **above** the
client-agnostic protocol docs and **below** the client ADRs that consume them.

| Document | Description |
|----------|-------------|
| [libPlcTag/](libPlcTag/README.md) | Index of libplctag behaviour docs |
| [libPlcTag/the-shared-session.md](libPlcTag/the-shared-session.md) | The one CIP session shared across handles to a controller, and request packing end to end: whether requests pack (`allow_packing` by PLC type), when and how large a pack gets (self-clocking thread, no linger timer, throughput tracks requests-in-flight), what you can steer (exclude or segregate, never compose), and why the reusable unit is the warm handle |
| [libPlcTag/the-port-in-the-gateway-attribute.md](libPlcTag/the-port-in-the-gateway-attribute.md) | Why there is no port attribute — the gateway string carries `host:port`, split in the native core, defaulting to 44818 (502 for Modbus TCP) — and where `ConnectionEndpoint` and `TcpPort` are joined back together |
| [libPlcTag/concurrent-operations-on-a-handle.md](libPlcTag/concurrent-operations-on-a-handle.md) | Why two overlapping operations on one handle mispair and cascade, and the raw-buffer race below the native BUSY guard |
| [libPlcTag/tag-disposal-and-shutdown.md](libPlcTag/tag-disposal-and-shutdown.md) | Why every `Tag` must be disposed deterministically — finalized native handles fail-fast the process with `0xC0000602` |
| [libPlcTag/what-the-tag-buffer-holds.md](libPlcTag/what-the-tag-buffer-holds.md) | What `GetBuffer` returns: payload only, with protocol framing stripped and the controller's own layout — count words, padding, BOOL packing, wire byte order — left intact |
| [libPlcTag/writing-into-the-tag-buffer.md](libPlcTag/writing-into-the-tag-buffer.md) | What `SetBuffer` does with an array that is not the handle's width: a longer one is refused with `PLCTAG_ERR_OUT_OF_BOUNDS` before anything is sent, a shorter one fills from the start and the whole handle goes out |
| [libPlcTag/reading-a-udt-definition.md](libPlcTag/reading-a-udt-definition.md) | Why UDT metadata takes both `@tags` and `@udt/<id>`, what the `@udt/` buffer actually contains, and how nested UDTs are walked |

### Implementation context — `context/`

Background material specific to this implementation: the test devices available for integration
testing.

| Document | Description |
|----------|-------------|
| [context/TEST-DEVICE-SETUP.md](context/TEST-DEVICE-SETUP.md) | The Allen-Bradley CompactLogix L32E used for integration testing: GateManager access, IP/backplane path, available tags, and how to run the tests |

## Developer process

| Document | Description |
|----------|-------------|
| [process/](process/README.md) | How a dataport gets built, phase by phase. Phase 1, [Bootstrap the project](process/bootstrap-the-project.md), is written |
| [documentation-principles.md](documentation-principles.md) | How these docs are organised; Diátaxis + the no-duplication rule |
| [modelling-conventions.md](modelling-conventions.md) | The types we define are `readonly record struct`s or `enum`s, never bare primitives |
| [CONTEXT.md](../../CONTEXT.md) | The ubiquitous language — Rockwell's and ODVA's words for the tag model and for reaching a controller, and the ones we avoid |

The repo-wide build conventions — test platform, package feeds, central package management — are
documented in [`AGENTS.md`](../../AGENTS.md) at the repo root.

## Contributing to documentation

When adding a document, place it in the matching Diátaxis folder of the relevant port project
(`explanation/`, `how-to/`, `reference/`, or `ADR/`), or in this hub if it is cross-port, add it to
that project's README index, and **do not duplicate the extensions docs** — link to them instead.
Files appear in the IDE automatically: each documentation project surfaces its own folder tree, so a
new file shows up on reload without editing the `.csproj`.
