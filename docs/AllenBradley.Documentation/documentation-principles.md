# Documentation Principles

These docs follow the [Diátaxis](https://diataxis.fr/) framework — a systematic approach to technical
documentation organised around what a reader needs, not around what the product does. They are
modelled on the documentation that ships with the `ViciOne.Suite.DataPort.Extensions` package, and
they deliberately **stop where that package's docs begin** (see
[Don't duplicate the extensions docs](#dont-duplicate-the-extensions-docs)).

---

## The four types

Diátaxis identifies four distinct documentation needs, each requiring a different kind of writing:

- **Tutorials** — *learning-oriented.* A hands-on lesson for a newcomer. This project has none yet.
- **How-to guides** — *task-oriented.* A recipe for an already-competent reader to accomplish a
  specific goal (e.g. *add a data type*, *connect to a device*). It omits teaching and background.
- **Reference** — *information-oriented.* Precise, complete description consulted rather than read
  (e.g. the *data type support matrix*). Its structure mirrors the product. No instruction, no opinion.
- **Explanation** — *understanding-oriented.* Discusses *why* — design rationale, trade-offs, the
  shape of the architecture. It illuminates rather than instructs.

|                         | Action (practical) | Cognition (theoretical) |
|-------------------------|--------------------|-------------------------|
| **Acquisition (study)** | Tutorials          | Explanation             |
| **Application (work)**  | How-to guides      | Reference               |

---

## How we apply it

The documentation is split across sibling projects that mirror the eventual `src/`:

| Project                                                                          | Holds                                                                       |
|----------------------------------------------------------------------------------|-----------------------------------------------------------------------------|
| **AllenBradley.Documentation** (this project)                                    | Cross-port material: the hub, these principles, `cip-protocol/`, `context/`, and process docs |
| [**AllenBradley.Logix.Documentation**](../AllenBradley.Logix.Documentation/README.md)  | The Logix port — symbolic tag addressing (ControlLogix, CompactLogix, GuardLogix, SoftLogix) |
| [**AllenBradley.Legacy.Documentation**](../AllenBradley.Legacy.Documentation/README.md) | The Legacy port — file/data-table addressing over PCCC (PLC-5, SLC 500, MicroLogix) |

Each port project organises its content into Diátaxis folders:

| Folder         | Type         | Answers                                              |
|----------------|--------------|------------------------------------------------------|
| `explanation/` | Explanation  | *Why* is the port built this way?                    |
| `how-to/`      | How-to guide | *How* do I connect, add a type, troubleshoot, tune?  |
| `reference/`   | Reference    | *What exactly* does this port support?               |
| `ADR/`         | Reference    | What was decided, and why — architecture decision records |

When adding content, identify which question it answers and place it in the matching folder. If no
existing file fits, create one. Cross-port material belongs in this project under one of its top-level
folders:

- [`cip-protocol/`](cip-protocol/) — **client-agnostic** CIP / EtherNet/IP background: wire formats,
  the object model, networking, per-family type differences. Nothing here is specific to this codebase.
- [`libPlcTag/`](libPlcTag/) — **library-specific** behaviour of the `libplctag` dependency: what it
  does that the CIP spec does not dictate (request packing and its timing, connection sharing,
  concurrency and disposal quirks). Facts about the dependency, consumed by the client ADRs — one
  layer above `cip-protocol/`, one below the decisions in `ADR/`.
- [`context/`](context/) — the test-device inventory and other material needed to run and understand
  the implementation.

---

## Don't duplicate the extensions docs

The single most important rule. The generic data-port machinery — the incoming/outgoing base classes,
the connection lifecycle, polling, the write queue, retry and resilience, the typed-node framework,
value conversion and validation, the YAML consistency tests — belongs to the
`ViciOne.Suite.DataPort.Extensions` package and is **documented with the package**. Do not re-explain
any of it here.

When an Allen-Bradley page needs to refer to one of those mechanisms, name it and link out to the
DataPort.Extensions package docs rather than describing how it works. An Allen-Bradley page should
cover only:

- what is **specific to the Allen-Bradley implementation** — the CIP/EtherNet-IP client, tag vs.
  data-file addressing, the CIP data types and their wire layouts, symbol/UDT discovery, PCCC
  tunneling for the legacy families — and
- the **context** required to understand it (the protocol and PLC background in `cip-protocol/`).

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
markdown docs — duplicated implementation details go stale silently. How-to guides are deliberately
light on code for the same reason: they point at the source files that are the single source of truth
and explain the *shape* of the change.

---

## Keeping docs in sync

A doc change that matches a source change belongs in the same merge request. Add or remove a supported
data type and you update the port's `reference/` datatype-support doc; change an architectural decision
and you add or revise an `ADR/`. Markdown is linted with `markdownlint-cli2 "**/*.md"`; the ruleset is
in the repo-root [`.markdownlint.json`](../../.markdownlint.json).
