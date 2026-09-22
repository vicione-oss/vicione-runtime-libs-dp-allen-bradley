# About the client architecture

The Logix client sits between the two dataports and the native `libplctag` handle. These pages say why it is shaped the
way it is. They do not describe the code. The diagrams name the classes, the code says what each method does, and the
[decision records](../../ADR/) hold the decisions. What these pages add is the glue between the three: the constraint
that forced a choice, what it was weighed against, and what it costs the person who builds on it.

## Two forces we did not choose

The framework is the first one. `ViciOne.Suite.DataPort.Extensions` owns the lifecycle. It acquires a client, connects
it, verifies the configuration against the device, then polls groups of data points on one side and drains a write
queue on the other. It also decides what a read may return and what a write may throw. None of that is repeated here,
on purpose ([documentation principles](../../../AllenBradley.Documentation/documentation-principles.md)). The client
fits that contract and adds as little machinery of its own as it can.

`libplctag` is the second. It has no connection object. What it has is a handle per tag, and every handle to one
controller goes through a single shared session that packs whatever requests are in flight into one packet. A handle is
expensive the first time it is read, and it has to be disposed by hand. There is no connection status, and the socket
opens on the first read. The [library notes](../../../AllenBradley.Documentation/libPlcTag/README.md) record each of
those facts with the source line it came from. The rest of the design follows from them.

## The shape

![How the Logix client classes collaborate](diagrams/client-architecture-collaboration.svg)

One stack, entered from two seams. A read and a write each become a batch that fans out one request per tag and
collects the outcomes. Under the batches sits one tag manager per controller. It owns the symbol table loaded at
connect and one warm handle per data point. A pool gives both dataports the same client, so a controller sees one
session whichever direction is talking to it. The verifier uses the same tags the polls use rather than a second path
to the controller.

## The pages

Each part of the design has a page of its own, and the source folders are cut the same way. A page names the
constraint, shows the scene, weighs the alternatives, and says what the choice costs.

| Page                                                 | What it is about                                                                                                                | Folder                    |
|------------------------------------------------------|---------------------------------------------------------------------------------------------------------------------------------|---------------------------|
| [Reading and writing](reading-and-writing.md)        | How a poll and a write travel through the client, and why the two directions fail differently                                   | `Client/`                 |
| [Values and their types](values-and-their-types.md) | Why a value is made only by its data point, and why converters know nothing about the tag                                       | `Client/TypeConversion/`  |
| [Connecting](connecting.md)                          | Why a connect loads the symbol table, what owns the connection state, and why two ports share one session                       | `Client/Pool/`            |
| [Tags and handles](tags-and-handles.md)              | Why there is one warm handle per data point, borrowed and never owned, and why one operation at a time is enough                 | `Client/Tags/Lifetime/`, `Client/Tags/Access/` |
| [The symbol table](symbol-table.md)                  | What the client reads at connect, why all of it, and the one question the table answers                                         | `Client/Tags/Symbols/`    |
| [Verification](verification.md)                      | Why the configuration is compared against the controller once, at connect, and by both ports                                    | `Verification/`           |

The diagrams live in [`diagrams/`](diagrams/) beside these pages, each one an Excalidraw source plus the SVG exported
from it. How to edit one is in the
[documentation principles](../../../AllenBradley.Documentation/documentation-principles.md#diagrams).
