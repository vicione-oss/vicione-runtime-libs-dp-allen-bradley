# Documentation Principles

These docs follow the [Diátaxis](https://diataxis.fr/) framework, a systematic approach to technical
documentation organised around what a reader needs rather than around what the product does. They
are modelled on the documentation that ships with the `ViciOne.Suite.DataPort.Extensions` package,
and they deliberately stop where that package's docs begin (see
[Don't duplicate the extensions docs](#dont-duplicate-the-extensions-docs)).

---

## The four types

Diátaxis identifies four distinct documentation needs, each requiring a different kind of writing.

A tutorial is learning-oriented: a hands-on lesson for a newcomer. This project has none yet. A
how-to guide is task-oriented: a recipe for an already-competent reader to accomplish a specific
goal, such as adding a data type or connecting to a device. It omits teaching and background. A
reference is information-oriented: a precise, complete description that gets consulted rather than
read, such as the data type support matrix. Its structure mirrors the product, and it contains no
instruction and no opinion. An explanation is understanding-oriented. It discusses why, meaning
design rationale, trade-offs and the shape of the architecture, and it illuminates rather than
instructs.

|                         | Action (practical) | Cognition (theoretical) |
|-------------------------|--------------------|-------------------------|
| **Acquisition (study)** | Tutorials          | Explanation             |
| **Application (work)**  | How-to guides      | Reference               |

---

## How we apply it

The documentation is split across sibling projects that mirror the eventual `src/`:

| Project                                                                          | Holds                                                                       |
|----------------------------------------------------------------------------------|-----------------------------------------------------------------------------|
| **AllenBradley.Documentation** (this project)                                    | Cross-port material: the hub, `controllers/`, `protocol/`, `libplctag/`, `test-bench/`, and these conventions |
| [**AllenBradley.Logix.Documentation**](../../AllenBradley.Logix.Documentation/README.md)  | The Logix port: symbolic tag addressing (ControlLogix, CompactLogix, GuardLogix, SoftLogix) |
| [**AllenBradley.Legacy.Documentation**](../../AllenBradley.Legacy.Documentation/README.md) | The Legacy port: file/data-table addressing over PCCC (PLC-5, SLC 500, MicroLogix) |

Each port project organises its content into Diátaxis folders:

| Folder         | Type         | Answers                                              |
|----------------|--------------|------------------------------------------------------|
| `explanation/` | Explanation  | *Why* is the port built this way?                    |
| `how-to/`      | How-to guide | *How* do I connect, add a type, troubleshoot, tune?  |
| `reference/`   | Reference    | *What exactly* does this port support?               |
| `ADR/`         | Reference    | What was decided, and why: architecture decision records |

When adding content, identify which question it answers and place it in the matching folder. If no
existing file fits, create one. A diagram lives in a `diagrams/` folder beside the page that embeds it,
as an `.excalidraw` source and the SVG exported from it; there is no shared diagram folder. Cross-port
material belongs in this project, in the folder of the
layer it describes. The folders are layered from the bottom up, and a lower layer never mentions a
higher one except to point at it.

[`controllers/`](../controllers/) is the hardware: the product lines and their form factors, the
5X70/5X80 generation split, chassis and slot, and the route path that names them. A page here may
cite a CIP encoding, but the encoding itself lives in `protocol/`.

[`protocol/`](../protocol/) is client-agnostic CIP / EtherNet/IP background: wire formats, the
object model, the tag services, per-family type vocabularies. Nothing here is specific to this
codebase. It is split by who defines the mechanism. `protocol/cip/` is the ODVA standard and never
names a Rockwell object. `protocol/allen-bradley-extension/` is what Rockwell adds, with the
standard pieces it reuses marked *(std)*.

[`libplctag/`](../libplctag/) is the library-specific behaviour of the `libplctag` dependency, meaning
what it does that the CIP spec does not dictate: request packing and its timing, connection sharing,
concurrency and disposal quirks. These are facts about the dependency, consumed by the client ADRs.
The folder sits one layer above `protocol/` and one below the decisions in `ADR/`.

[`test-bench/`](../test-bench/) holds the controllers available for integration testing, how to reach
them, and what each suite assumes about them. [`conventions/`](./) holds these principles and the
modelling rule for the types we define.

---

## Don't duplicate the extensions docs

This is the single most important rule. The generic data-port machinery belongs to the
`ViciOne.Suite.DataPort.Extensions` package and is documented with the package. That covers the
incoming/outgoing base classes, the connection lifecycle, polling, the write queue, retry and
resilience, the typed-node framework, value conversion and validation, and the YAML consistency
tests. Do not re-explain any of it here.

When an Allen-Bradley page needs to refer to one of those mechanisms, name it and link out to the
DataPort.Extensions package docs rather than describing how it works. An Allen-Bradley page should
cover only what is specific to the Allen-Bradley implementation (the CIP/EtherNet-IP client, tag
versus data-file addressing, the CIP data types and their wire layouts, symbol/UDT discovery, PCCC
tunneling for the legacy families) and the context required to understand it, which is the protocol
and PLC background in `protocol/`.

If a sentence would be equally true of an OPC-UA or MQTT data-port, it almost certainly belongs in the
extensions docs, not here.

---

## What belongs where

The markdown docs and the source code serve different readers. Keep them from overlapping:

| Belongs in markdown docs                                | Belongs in XML doc comments / IntelliSense / the code |
|---------------------------------------------------------|-------------------------------------------------------|
| Why an abstraction exists                               | What a method or property does                        |
| How the pieces fit together                             | Parameter meaning and allowed values                  |
| Which parts are CIP-specific vs. base-class machinery   | Return value semantics                                |
| Behavioral contracts and ordering guarantees            | Exception conditions                                  |
| Design rationale for non-obvious decisions              | Type constraints                                      |
| Common pitfalls, known bugs, and workarounds            | Overload differences                                  |

If a detail is already expressed by the source and surfaced by IntelliSense, it does not belong in the
markdown docs. Duplicated implementation details go stale silently. How-to guides are deliberately
light on code for the same reason. They point at the source files that are the single source of truth
and explain the shape of the change.

---

## Diagrams

A diagram is drawn in Excalidraw. Its `.excalidraw` source and the SVG exported from it sit together in
the `diagrams/` folder beside the page that embeds the SVG, and both are committed. A diagram names
classes and methods, so a rename in the code is a rename in the diagram too. To change one:

1. Open the `.excalidraw` file in [excalidraw.com](https://excalidraw.com) or the Excalidraw extension
   for VS Code and edit it.
2. Export it as SVG with the background on and the scene embedding off, and save it over the SVG of the
   same name. The export embeds the font, so the SVG renders the same everywhere.
3. Save the `.excalidraw` source next to it and commit both.

---

## Keeping docs in sync

A doc change that matches a source change belongs in the same merge request. Add or remove a supported
data type and you update the port's `reference/` datatype-support doc. Change an architectural
decision and you add or revise an `ADR/`. Markdown is linted with `markdownlint-cli2 "**/*.md"`, and
the ruleset is in the repo-root [`.markdownlint.json`](../../../.markdownlint.json).
