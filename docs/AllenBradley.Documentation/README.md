# Allen-Bradley DataPorts Documentation

What a developer needs to know about Allen-Bradley controllers and the protocol they speak before
reading the code of a port. The port-specific documentation, meaning what the Logix port implements
and the decisions behind it, lives next door in
[AllenBradley.Logix.Documentation](../AllenBradley.Logix.Documentation/README.md) and
[AllenBradley.Legacy.Documentation](../AllenBradley.Legacy.Documentation/README.md).

The words these docs use are not chosen freely. [`CONTEXT.md`](../../CONTEXT.md) at the repo root is
the glossary. When Rockwell, ODVA and libplctag disagree about what to call something, and they often
do, the glossary decides.

## The thirty-second version

Allen-Bradley is Rockwell Automation's controller brand. Its controllers talk CIP, the Common
Industrial Protocol, an open standard from ODVA. EtherNet/IP is CIP carried over TCP/IP, and the
"IP" stands for Industrial Protocol rather than Internet Protocol. CIP itself has no idea what a tag
is. Everything that makes a Logix controller browsable and its tags readable by name is a Rockwell
extension on top of the standard.

Rockwell's controllers address data in one of two ways, and the two ports follow that split:

| Port        | Controllers                                       | Addressing                                       | Programmed in                  | Status              |
|-------------|---------------------------------------------------|--------------------------------------------------|--------------------------------|---------------------|
| **Logix**   | ControlLogix, CompactLogix, GuardLogix, SoftLogix | Symbolic tags (`Motor.Speed`, `Arr[5]`)          | Studio 5000 Logix Designer     | Built, slice by slice |
| **Legacy**  | PLC-5, SLC 500, MicroLogix                        | Data files (`N7:0`, `T4:0.PRE`) over PCCC        | RSLogix 5 · RSLogix 500        | Planned, not started |

Both ports speak CIP over EtherNet/IP on the wire. The Legacy port will tunnel PCCC, Rockwell's
older command set, inside it. Micro800 is not a legacy controller. It addresses tags symbolically,
but it has no program scope, a different type set and no browsing in the Logix sense, so it fits
neither port as they are, and whether it is ever supported is an open decision.

Inside the Logix line there is one more split the code leans on everywhere. The generation, 5X70
versus 5X80, decides which data types a controller can declare at all.

## New here? Read in this order

1. [`CONTEXT.md`](../../CONTEXT.md), the vocabulary. Skim it now and come back to it often.
2. [Controller families](controllers/controller-families.md): which controllers exist, how they
   are shaped, which tool programs them, and which protocol mechanism reaches their data.
3. [Logix generations](controllers/logix-generations.md): what 5X70 and 5X80 mean and why the
   port cares.
4. [CIP / EtherNet/IP networking overview](protocol/cip/networking-overview.md): the wire stack,
   sessions, the object model, services, and how a request is routed to a controller.
5. [Chassis and route paths](controllers/chassis-and-route-paths.md): why a connection needs a
   `1,0` next to its IP address.
6. [Tag services](protocol/allen-bradley-extension/tag-services.md): how a tag is read and written
   by name.
7. [Symbolic tag data types](protocol/allen-bradley-extension/symbolic-tag-data-types.md): which
   types a Logix tag can have, and how `STRING`, `TIMER` and `BOOL` arrays are laid out. It links
   to the CIP reference for the byte encoding of each type.
8. [Tag browsing](protocol/allen-bradley-extension/tag-browsing.md): how a client lists the tags a
   controller has and reads the definition of a structure.
9. [libplctag behaviour](libplctag/README.md): the library the port drives the wire through, and
   the facts about it that shaped the client.
10. [The test bench](test-bench/test-device-setup.md): the two controllers, how to reach them, and
    how to run the integration suite.

Then move on to the [Logix port](../AllenBradley.Logix.Documentation/README.md). Its reference
section says which of the types above the port implements today.

## The map

The folders are layered from the bottom up. Each one describes a layer the next one builds on, and a
lower folder never mentions a higher one except to point at it. The `protocol/` and `libplctag/`
folders have an index of their own. The others are small enough to open.

| Folder | Layer | What it holds |
|--------|-------|---------------|
| [`controllers/`](controllers/) | The hardware | The product lines and form factors, the 5X70/5X80 generation split, chassis and slot, and the route path that names them |
| [`protocol/cip/`](protocol/README.md) | The ODVA standard | The wire stack, the object model and services, the data-type encodings. Nothing here names a Rockwell object |
| [`protocol/allen-bradley-extension/`](protocol/README.md) | What Rockwell adds | The tag services, the Symbol and Template objects, controller and program scope, the types the tag-addressed and file-addressed families expose, and the PCCC tunnel |
| [`libplctag/`](libplctag/README.md) | The library | What libplctag does that the spec does not dictate: session sharing and packing, the buffer contract, disposal, browsing pseudo-tags |
| [`test-bench/`](test-bench/) | Our devices | The borrowed L32E and our own L306ER: access, addresses, tags, and the environment variables the suites read |
| [`conventions/`](conventions/) | How we write | The documentation principles these pages obey, and the modelling rule for the types we define |

The repo-wide build conventions (test platform, package feeds, central package management) are in
[`AGENTS.md`](../../AGENTS.md) at the repo root.

## Contributing to documentation

Cross-port material goes in the folder of its layer here. Port-specific material goes in the matching
Diátaxis folder (`explanation/`, `how-to/`, `reference/`, or `ADR/`) of the port project. Add the
document to the index of the README it lives under, and do not duplicate the extensions docs. Link
to them instead. The rules are in `conventions/`. Files appear in the IDE automatically, because each
documentation project surfaces its own folder tree, so a new file shows up on reload without editing
the `.csproj`.
