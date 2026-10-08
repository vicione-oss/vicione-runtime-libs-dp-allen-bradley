# AGENTS.md

Allen-Bradley DataPorts for .NET 10 that speak CIP over EtherNet/IP. Two dataports are planned:
**Logix** (ControlLogix / CompactLogix, symbolic tag addressing) and **Legacy**
(MicroLogix / Micro800, file-based addressing).

## Current state — read this first

`src/AllenBradley.Logix` is a working dataport, built slice by slice. It reads and writes tags against a real
controller. It contains the YAML manifest and the typed node model mapped from it, the incoming and outgoing ports,
a pooled `LogixClient` over `libplctag`, per-type converters, and verification of a configuration against the
controller's symbol table.

Before you add a data type or a node, read these two pages:

1. [`reference/datatype-support.md`](docs/AllenBradley.Logix.Documentation/reference/datatype-support.md) — what is
   implemented today, and the list of what is not.
2. [`explanation/model/node-model.md`](docs/AllenBradley.Logix.Documentation/explanation/model/node-model.md) — how
   the tree is modelled.

The **Legacy** dataport has no project yet.

## Quick Reference

| Task               | Command                                                                                                                             |
|--------------------|-------------------------------------------------------------------------------------------------------------------------------------|
| Build              | `dotnet build allen-bradley.slnx -c Debug`                                                                                          |
| Test (unit)        | `dotnet test` — the unit suite is the default, and never touches the PLC                                                            |
| Test (integration) | `dotnet test -p:test-suite=integration` — **needs a controller** (see test-device-setup.md)                                         |
| Test (all)         | `dotnet test -p:test-suite=all`                                                                                                     |
| Test (class)       | `dotnet test --project tests/AllenBradley.Logix.Tests/AllenBradley.Logix.Tests.csproj --filter-class "<fully.qualified.ClassName>"` |
| Format             | `dotnet format allen-bradley.slnx`                                                                                                  |
| Clean build        | `dotnet clean allen-bradley.slnx && dotnet build allen-bradley.slnx -c Debug`                                                       |
| Lint MD            | `markdownlint-cli2 "**/*.md"`                                                                                                       |
| Verify             | `dotnet build allen-bradley.slnx -c Debug && dotnet test && dotnet format allen-bradley.slnx --verify-no-changes`                   |

## Testing platform — the thing that trips people up

**MTP + xUnit v3.** The classic VSTest `--filter` syntax does **not** work here. To select a suite, use the
`test-suite` MSBuild property, defined in `tests/Directory.Build.props`:

- Unit tests carry **no trait**. Integration tests carry `[Trait("Category", "Integration")]`.
- Valid values are `unit`, `integration` and `all`. Unset selects the unit suite, so a bare `dotnet test` is always
  PLC-safe.
- An unknown `test-suite` value **fails the build** instead of a silent run of hardware I/O.
- `--filter-query` clashes with the injected suite filter. Use the simple-style filters (`--filter-class`,
  `--filter-trait`, `--filter-not-trait`).

## The integration suite

`tests/AllenBradley.Logix.Tests/Integration/` is the suite that targets actual hardware. A bare `dotnet test` never runs it.
It is split in two, by device:

| Folder              | Device                                                    | What a test there is                                     |
|---------------------|-----------------------------------------------------------|----------------------------------------------------------|
| `CompactLogix5X70/` | the **L32E**, borrowed, in the ifm demo cell              | a description of tags we cannot change                   |
| `CompactLogix5X80/` | the **5069-L306ER**, our own device, not commissioned yet | a specification for a tag that must still be provisioned |

Put new integration coverage in `CompactLogix5X80/`, where a test that needs a tag can have one provisioned. Each
folder holds exactly one type that says how its device is reached — `BenchController` for the L32E, `TestController`
for the L306ER — and each reads its endpoint from an environment variable with a default. `PlcCollection` holds every
test that talks to a controller and never runs in parallel, because the tests share one libplctag session.

Read [test-device-setup.md](docs/AllenBradley.Documentation/test-bench/test-device-setup.md) before you point anything
at a device.

## Repo conventions

Adopted wholesale from the S7 repo, so that a dataport developer who moves between the two finds the same build:

- **`Directory.Build.props`** — net10.0, nullable + implicit usings, `TreatWarningsAsErrors`,
  `EnforceCodeStyleInBuild`, NuGet lock files, version read from `VERSION`
- **`Directory.Packages.props`** — central package management; **no inline `Version=` in a csproj**
- **`NuGet.Config`** — JFrog proxy for everything, JFrog ViciOne for `ViciOne.*`
- **`.editorconfig`** — S7's, verbatim
- **`NSubstitute`** for interaction tests only — where every assertion is "the caller did this to its
  collaborator", as in `LogixClientPoolTests`. Keep stateful fakes (a tag lookup, a captured write buffer)
  hand-rolled; a substitute makes those longer, not shorter
- **Model types are `readonly record struct`s or `enum`s — never bare primitives.** Every domain concept we define
  gets its own named value type; see
  [modelling-conventions.md](docs/AllenBradley.Documentation/conventions/modelling-conventions.md)

## DataPort.Extensions Packages

Shared infrastructure comes from `ViciOne.Suite.DataPort.Extensions` (and `...Extensions.Testing` for test
utilities) — base classes for incoming and outgoing dataports, resilience, validation, queuing and logging. The full
documentation ships inside the NuGet packages themselves.

## Documentation

| Topic                     | Location                                                                                                                                             |
|---------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------|
| **Ubiquitous language**   | [CONTEXT.md](CONTEXT.md) — **read before naming anything**; `allen-bradley.glossary.yml` is its Contextive twin, keep the two in step                |
| Documentation index       | [docs/AllenBradley.Documentation/README.md](docs/AllenBradley.Documentation/README.md)                                                               |
| Documentation principles  | [docs/AllenBradley.Documentation/conventions/documentation-principles.md](docs/AllenBradley.Documentation/conventions/documentation-principles.md)   |
| Modelling conventions     | [docs/AllenBradley.Documentation/conventions/modelling-conventions.md](docs/AllenBradley.Documentation/conventions/modelling-conventions.md)         |
| CIP protocol              | [docs/AllenBradley.Documentation/protocol/README.md](docs/AllenBradley.Documentation/protocol/README.md)                                             |
| libplctag behaviour       | [docs/AllenBradley.Documentation/libplctag/README.md](docs/AllenBradley.Documentation/libplctag/README.md) — the library's quirks and traps          |
| Test device setup         | [docs/AllenBradley.Documentation/test-bench/test-device-setup.md](docs/AllenBradley.Documentation/test-bench/test-device-setup.md)                   |
| Logix dataport            | [docs/AllenBradley.Logix.Documentation/README.md](docs/AllenBradley.Logix.Documentation/README.md)                                                   |
| Legacy dataport           | [docs/AllenBradley.Legacy.Documentation/README.md](docs/AllenBradley.Legacy.Documentation/README.md)                                                 |
| Data types the port has   | [docs/AllenBradley.Logix.Documentation/reference/datatype-support.md](docs/AllenBradley.Logix.Documentation/reference/datatype-support.md)           |
| Node model                | [docs/AllenBradley.Logix.Documentation/explanation/model/node-model.md](docs/AllenBradley.Logix.Documentation/explanation/model/node-model.md)       |
| Client architecture       | [docs/AllenBradley.Logix.Documentation/explanation/client/architecture.md](docs/AllenBradley.Logix.Documentation/explanation/client/architecture.md) |
| Decision records          | [docs/AllenBradley.Logix.Documentation/ADR/](docs/AllenBradley.Logix.Documentation/ADR/)                                                             |
| Tree-editor icons         | [docs/AllenBradley.Logix.Documentation/logix-icons/README.md](docs/AllenBradley.Logix.Documentation/logix-icons/README.md) — edit the SVG and run the script, never a `Content:` line |
| Deploy to a local ViciOne | [scripts/README.md](scripts/README.md)                                                                                                               |

## GitLab & Version Control

This project is hosted on **GitLab**. Always use the **`glab` CLI** for repository interactions.

| Task       | Command                                                         |
|------------|-----------------------------------------------------------------|
| View MR    | `glab mr view <id>`                                             |
| Update MR  | `glab mr update <id> --title "..." --description "..."`         |
| Create MR  | `glab mr create`                                                |
| List MRs   | `glab mr list`                                                  |
| View issue | `glab issue view <id>`                                          |
