# AGENTS.md

Allen-Bradley DataPorts for .NET 10 that speak CIP over EtherNet/IP. Two dataports are planned:
**Logix** (ControlLogix / CompactLogix, symbolic tag addressing) and **Legacy**
(PLC-5 / SLC 500 / MicroLogix, data-file addressing over PCCC). Micro800 is symbolic and belongs to neither.

## Current state — read this first

`src/AllenBradley.Logix` is a working dataport, built slice by slice. It reads and writes tags against a real
controller. It contains the YAML manifest and the typed node model mapped from it, the incoming and outgoing ports,
the `LogixClient` with its symbol table and tag attributes, per-type converters, and verification of a configuration
against the controller's symbol table. The client pool, the batches and the byte layout of the elementary types come
from the shared client project (below).

Before you add a data type or a node, read these two pages:

1. [`reference/datatype-support.md`](docs/AllenBradley.Logix.Documentation/reference/datatype-support.md) — what is
   implemented today, and the list of what is not.
2. [`explanation/model/node-model.md`](docs/AllenBradley.Logix.Documentation/explanation/model/node-model.md) — how
   the tree is modelled.

A new elementary type also needs a codec in `src/AllenBradley.Client.Shared/TypeConversion/Codecs/` if there is none
for its .NET type yet.

The **Legacy** dataport is a second project in this repo, `src/AllenBradley.Legacy`, versioned and published
together with Logix. Its scope is in the [Legacy README](docs/AllenBradley.Legacy.Documentation/README.md).

### The shared client

`src/AllenBradley.Client.Shared` holds the client code that both dataports use around libplctag: the client pool, the
tag cache, the read and write batches, the access to one handle, and the codecs for the elementary types. Read
[the shared client](docs/AllenBradley.Documentation/client/README.md) before you change it or build a client on it.

- **Where a change goes.** A fix to pooling, tag reuse, batching or handle access goes in the shared project, and it
  fixes both dataports. Data points, converters, tag attributes, the symbol table and the structured types (strings,
  timers, counters, bit arrays) stay in the dataport.
- **What may go in.** Only a type that names no Logix or Legacy type.
- **Failures are values.** The shared batches return their failures and never throw for a failed tag. Each dataport
  turns them into its own exception, as `LogixReadBatch` and `LogixWriteBatch` do with `LogixTagException`. Do not
  add a dataport exception to the shared code.
- **Internal by default.** `IConnectableClient` is the only public type, because the public client interfaces extend
  it. Keep everything else internal.
- **Packaging.** It is not a package of its own. `dotnet publish` of a dataport copies its assembly through the project
  reference, so CI and the deploy scripts need nothing extra.

The decision is in the
[ADR](docs/AllenBradley.Logix.Documentation/ADR/2026-10-12-sharing-the-libplctag-client-code-with-legacy.md).

## Quick Reference

| Task               | Command                                                                                                                             |
|--------------------|-------------------------------------------------------------------------------------------------------------------------------------|
| Build              | `dotnet build allen-bradley.slnx -c Debug`                                                                                          |
| Test (unit)        | `dotnet test` — the unit suite is the default, and never touches the PLC                                                            |
| Test (integration) | `dotnet test -p:test-suite=integration` — **needs a controller** (see run-the-integration-tests.md)                                  |
| Test (all)         | `dotnet test -p:test-suite=all`                                                                                                     |
| Test (class)       | `dotnet test --project tests/AllenBradley.Logix.Tests/AllenBradley.Logix.Tests.csproj --filter-class "<fully.qualified.ClassName>"` |
| Test (shared)      | `dotnet test --project tests/AllenBradley.Client.Shared.Tests/AllenBradley.Client.Shared.Tests.csproj` — fakes only, no PLC        |
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
- In the IDE, each test project has the launch profiles `all-tests`, `unit-tests` and `integration-tests`. Keep the
  first profile free of arguments, because `dotnet test` merges its arguments with the suite filter.

The full rules, with `dotnet run` and the filter options, are in the
[testing guide](docs/AllenBradley.Documentation/conventions/testing-guide.md).

## The integration suite

`tests/AllenBradley.Logix.Tests/Integration/` is the suite that targets actual hardware. A bare `dotnet test` never runs it.
It runs against our **5069-L306ER**, provisioned with `CompactLogix5X80/Infrastructure/Provisioning/ViciOne.L5X`. All of it is in
`CompactLogix5X80/`: a round trip per data type, against tags we provision, and a few client-stack tests that hold
the client against raw libplctag handles.

If a new test needs a tag, add the tag to `TagAddresses.cs` and `ViciOne.L5X` together. `TestControllerConfiguration` is the only
type that says how the controller is reached. It reads the endpoint from environment variables with defaults. All
tests are in `TestControllerCollection`, which never runs in parallel, because the tests share one libplctag session.
The suite connects one time for the whole collection, through the `TestControllerFixture`.

Read [run-the-integration-tests.md](docs/AllenBradley.Logix.Documentation/how-to/run-the-integration-tests.md) before
you point anything at a device.

## Repo conventions

Adopted wholesale from the S7 repo, so that a dataport developer who moves between the two finds the same build:

- **`Directory.Build.props`** — net10.0, nullable + implicit usings, `TreatWarningsAsErrors`,
  `EnforceCodeStyleInBuild`, NuGet lock files, version read from `VERSION`
- **`Directory.Packages.props`** — central package management; **no inline `Version=` in a csproj**
- **`NuGet.Config`** — JFrog proxy for everything, JFrog ViciOne for `ViciOne.*`
- **`.editorconfig`** — S7's, verbatim
- **`NSubstitute`** for interaction tests only — where every assertion is "the caller did this to its
  collaborator", as in `ClientPoolTests`. Keep stateful fakes (a tag lookup, a captured write buffer)
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
| Testing guide             | [docs/AllenBradley.Documentation/conventions/testing-guide.md](docs/AllenBradley.Documentation/conventions/testing-guide.md) — suites, filters, launch profiles |
| Integration tests         | [docs/AllenBradley.Logix.Documentation/how-to/run-the-integration-tests.md](docs/AllenBradley.Logix.Documentation/how-to/run-the-integration-tests.md) |
| Logix dataport            | [docs/AllenBradley.Logix.Documentation/README.md](docs/AllenBradley.Logix.Documentation/README.md)                                                   |
| Legacy dataport           | [docs/AllenBradley.Legacy.Documentation/README.md](docs/AllenBradley.Legacy.Documentation/README.md)                                                 |
| Data types the port has   | [docs/AllenBradley.Logix.Documentation/reference/datatype-support.md](docs/AllenBradley.Logix.Documentation/reference/datatype-support.md)           |
| Node model                | [docs/AllenBradley.Logix.Documentation/explanation/model/node-model.md](docs/AllenBradley.Logix.Documentation/explanation/model/node-model.md)       |
| Client architecture       | [docs/AllenBradley.Logix.Documentation/explanation/client/architecture.md](docs/AllenBradley.Logix.Documentation/explanation/client/architecture.md) |
| Shared client             | [docs/AllenBradley.Documentation/client/README.md](docs/AllenBradley.Documentation/client/README.md) — what both dataports share, and what each adds |
| Decision records          | [docs/AllenBradley.Logix.Documentation/ADR/](docs/AllenBradley.Logix.Documentation/ADR/)                                                             |
| Tree-editor icons         | [docs/AllenBradley.Logix.Documentation/logix-icons/README.md](docs/AllenBradley.Logix.Documentation/logix-icons/README.md) — edit the SVG and run the script, never a `Content:` line |
| Deploy to a local ViciOne | [scripts/README.md](scripts/README.md)                                                                                                               |

## GitLab & Version Control

This project is hosted on **GitLab**. Always use the **`glab` CLI** for repository interactions.

After a write, read the result back to confirm it landed.

Write every issue in the structure of [`.gitlab/issue_templates/Default.md`](.gitlab/issue_templates/Default.md)
and every MR description in the structure of
[`.gitlab/merge_request_templates/Default.md`](.gitlab/merge_request_templates/Default.md). The web UI fills them
in, but `glab issue create --description` and `glab mr create --description` do not, so read the template first and
follow its section comments. Leave out the comments themselves. The issue says what should happen; the MR says what
did happen and does not repeat the issue.

Write every MR description in the structure of
[`.gitlab/merge_request_templates/Default.md`](.gitlab/merge_request_templates/Default.md). The web UI fills it in,
but `glab mr create --description` does not, so read the template first and follow its section comments. Leave out
the comments themselves.

| Task       | Command                                                         |
|------------|-----------------------------------------------------------------|
| View MR    | `glab mr view <id>`                                             |
| Update MR  | `glab mr update <id> --title "..." --description "..."`         |
| Create MR  | `glab mr create`                                                |
| List MRs   | `glab mr list`                                                  |
| View issue | `glab issue view <id>`                                          |
