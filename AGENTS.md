# AGENTS.md

Allen-Bradley DataPorts for .NET 10, speaking CIP over EtherNet/IP. Two addons are planned:
**Logix** (ControlLogix / CompactLogix, symbolic tag addressing) and **Legacy**
(MicroLogix / Micro800, file-based addressing). Only Logix has a project so far.

## Current state — read this first

The repo is **bootstrapped, not implemented**. `src/AllenBradley.Logix` compiles and deliberately does nothing: it holds
the addon's identity and nothing else. Device nodes, the YAML manifest, the communication configuration and any client
code arrive with the walking-skeleton slice.

What *does* exist is a connectivity spike — `tests/AllenBradley.Logix.Tests/Integration/` — which talks to a real L32E
over the Link Manager tunnel using `libplctag`. It proves the protocol works; it is not the addon. `libplctag` is a
dependency of the **test** project only. Committing `src/` to a driver is the walking skeleton's decision, not a settled
one.

**The unit suite is a placeholder.** There is no addon behaviour to test yet, but MTP exits non-zero on a run that
discovers nothing, so `PlaceholderTests` holds a single tautology to keep
`dotnet test` green. It asserts nothing about the product. Delete it as soon as there is something real to assert.

## Quick Reference

| Task               | Command                                                                                                                             |
|--------------------|-------------------------------------------------------------------------------------------------------------------------------------|
| Build              | `dotnet build allen-bradley.slnx -c Debug`                                                                                          |
| Test (unit)        | `dotnet test` — the unit suite is the default, and never touches the PLC                                                            |
| Test (integration) | `dotnet test -p:test-suite=integration` — **requires the L32E**                                                                     |
| Test (all)         | `dotnet test -p:test-suite=all`                                                                                                     |
| Test (class)       | `dotnet test --project tests/AllenBradley.Logix.Tests/AllenBradley.Logix.Tests.csproj --filter-class "<fully.qualified.ClassName>"` |
| Format             | `dotnet format allen-bradley.slnx`                                                                                                  |
| Clean build        | `dotnet clean allen-bradley.slnx && dotnet build allen-bradley.slnx -c Debug`                                                       |
| Lint MD            | `markdownlint-cli2 "**/*.md"`                                                                                                       |
| Verify             | `dotnet build allen-bradley.slnx -c Debug && dotnet test && dotnet format allen-bradley.slnx --verify-no-changes`                   |

## Testing platform — the thing that trips people up

**MTP + xUnit v3.** The classic VSTest `--filter` syntax does **not** work here. Suites are selected through the
`test-suite` MSBuild property, defined in `tests/Directory.Build.props`:

- Unit tests carry **no trait**. Integration tests carry `[Trait("Category", "Integration")]`.
- Unset runs the unit suite, so a bare `dotnet test` is always PLC-safe.
- An unknown `test-suite` value **fails the build** rather than silently running hardware I/O.
- `--filter-query` clashes with the injected suite filter; use the simple-style filters (`--filter-class`,
  `--filter-trait`, `--filter-not-trait`) instead.

This was not a free choice. `ViciOne.Suite.DataPort.Extensions.Testing` depends on
`xunit.v3.extensibility.core`, so xUnit v2 was never available — and MTP was picked over the xUnit v3 VSTest adapter to
stay aligned with the S7 repo. Integration tests need the L32E reachable;
see [docs/AllenBradley.Documentation/context/TEST-DEVICE-SETUP.md](docs/AllenBradley.Documentation/context/TEST-DEVICE-SETUP.md).

**Under MTP the test assembly *is* the process**, so its exit code is the suite's exit code. Anything that corrupts
process shutdown fails the suite even when every test passed. This is not theoretical:
the spike's `PlcTagLister` did not dispose its libplctag `Tag` objects, so native handles were freed from finalizers
after CLR teardown and the process fail-fasted with `0xC0000602` on a green 3/3 run. **Dispose every `Tag`.** The old
VSTest host hid this.

## Repo conventions

Adopted wholesale from the S7 repo, so that an addon developer moving between the two finds the same build:

- **`Directory.Build.props`** — net10.0, nullable + implicit usings, `TreatWarningsAsErrors`,
  `EnforceCodeStyleInBuild`, NuGet lock files, version read from `VERSION`
- **`Directory.Packages.props`** — central package management; **no inline `Version=` in a csproj**
- **`NuGet.Config`** — JFrog proxy for everything, JFrog ViciOne for `ViciOne.*`
- **`.editorconfig`** — S7's, verbatim
- **`AwesomeAssertions`**, not FluentAssertions (which conflicts, and was dropped over the licence change)
- **Model types are `readonly record struct`s or `enum`s — never bare primitives.** Every domain concept we define gets
  its own named value type; see
  [modelling-conventions.md](docs/AllenBradley.Documentation/modelling-conventions.md)

**Trap:** `Directory.Build.props` switches a project to MTP on the condition
`MSBuildProjectName.EndsWith('Tests') And MSBuildProjectDirectory.Contains('tests')`. Any new test project picks that up
automatically — which is usually what you want, but it means naming a non-test project `*Tests` under `tests/` will
quietly reconfigure it.

`src/` and `tests/` project names are load-bearing: `Directory.Build.props` grants
`InternalsVisibleTo` to `$(MSBuildProjectName).Tests`, so `AllenBradley.Logix` is tested by
`AllenBradley.Logix.Tests` and the wiring is implicit. `metadata.json` in each addon carries the same id and is
templated by the pipeline.

## DataPort.Extensions Packages

Shared infrastructure comes from `ViciOne.Suite.DataPort.Extensions` (and `...Extensions.Testing`
for test utilities) — base classes for incoming/outgoing dataports, resilience, validation, queuing and logging.
Comprehensive documentation ships inside the NuGet packages themselves.

## Documentation

| Topic                      | Location                                                                                                                     |
|----------------------------|------------------------------------------------------------------------------------------------------------------------------|
| Documentation index        | [docs/AllenBradley.Documentation/README.md](docs/AllenBradley.Documentation/README.md)                                       |
| Documentation principles   | [docs/AllenBradley.Documentation/documentation-principles.md](docs/AllenBradley.Documentation/documentation-principles.md)   |
| Modelling conventions      | [docs/AllenBradley.Documentation/modelling-conventions.md](docs/AllenBradley.Documentation/modelling-conventions.md)         |
| CIP protocol               | [docs/AllenBradley.Documentation/cip-protocol/README.md](docs/AllenBradley.Documentation/cip-protocol/README.md)             |
| Test device setup          | [docs/AllenBradley.Documentation/context/TEST-DEVICE-SETUP.md](docs/AllenBradley.Documentation/context/TEST-DEVICE-SETUP.md) |
| Logix addon                | [docs/AllenBradley.Logix.Documentation/README.md](docs/AllenBradley.Logix.Documentation/README.md)                           |
| Legacy addon               | [docs/AllenBradley.Legacy.Documentation/README.md](docs/AllenBradley.Legacy.Documentation/README.md)                         |
| Addressing / dataport defs | `dataport-definition/`                                                                                                       |

## GitLab & Version Control

This project is hosted on **GitLab**. Always use the **`glab` CLI** for repository interactions.

Some read commands (e.g. `glab mr view`) fail with 401 unless you pass
`--repo vicione-oss/addons/allen-bradley/cip`. Write commands (`mr update`, `mr create`) work without it. When in doubt,
add `--repo`.

| Task       | Command                                                         |
|------------|-----------------------------------------------------------------|
| View MR    | `glab mr view <id> --repo vicione-oss/addons/allen-bradley/cip` |
| Update MR  | `glab mr update <id> --title "..." --description "..."`         |
| Create MR  | `glab mr create`                                                |
| List MRs   | `glab mr list`                                                  |
| View issue | `glab issue view <id>`                                          |
