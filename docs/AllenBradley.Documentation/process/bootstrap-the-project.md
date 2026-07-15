# Bootstrap the project

How to take an addon repo from "documentation and a spike" to "a solution that builds and a test
command that works". This is the first phase of the process, and it is deliberately narrow. You are
not implementing the dataport here. You are making it possible to implement one.

The guide is written from the Allen-Bradley bootstrap (issue #3) and assumes the S7 repo as the
template. If you are bootstrapping a third addon, the same steps apply and S7 is still the template.

## Before you start

You need two things that are easy to get wrong.

**Know which feed serves the ViciOne packages.** A developer laptop usually has extra NuGet sources
registered globally, so a restore can succeed on your machine and fail on a clean clone. Copy S7's
`NuGet.Config` into the repo root rather than relying on whatever is in your user profile. It clears
inherited sources and maps `ViciOne.*` to the JFrog ViciOne feed explicitly.

**Check what the test utilities drag in.** Run this before you decide anything about the test stack:

```bash
sed -n '/<dependencies>/,/<\/dependencies>/p' \
  ~/.nuget/packages/vicione.suite.dataport.extensions.testing/<version>/*.nuspec
```

On Allen-Bradley this is the step that overturned the plan. The issue had specified xUnit v2 with the
classic VSTest `--filter` syntax, but `ViciOne.Suite.DataPort.Extensions.Testing` depends on
`xunit.v3.extensibility.core`, and it has done so in every published version. The test stack was
therefore decided by a package dependency, not by us. Better to find that out in five minutes than
after writing the projects.

## 1. Settle the conventions first

Do this while there is one project to migrate rather than six. Copy from S7:

| File | What it does |
|------|--------------|
| `NuGet.Config` | Feeds and package-source mapping. Verbatim |
| `.editorconfig` | Code style. Verbatim |
| `Directory.Build.props` | Target framework, nullable, warnings-as-errors, versioning, the MTP switch |
| `Directory.Packages.props` | Central package management. Port every inline `Version=` into it |
| `global.json` | SDK pin, and the MTP runner selection |
| `VERSION`, `VERSION_CORE` | Read by `Directory.Build.props` and by the packaging pipeline |

Two edits are needed as you copy, and both are easy to miss.

Strip the S7-specific metadata from `Directory.Build.props`: `Product`, `PackageProjectUrl`, and the
`InternalsVisibleTo` entries for projects that do not exist in your repo (S7 has a `Benchmarks`
project; you probably do not).

Then look hard at this condition, which is the sharpest trap in the file:

```xml
<PropertyGroup Condition="$(MSBuildProjectName.EndsWith('Tests')) And $(MSBuildProjectDirectory.Contains('tests'))">
  <UseMicrosoftTestingPlatformRunner>true</UseMicrosoftTestingPlatformRunner>
```

It matches any project named `*Tests` under `tests/`, which almost certainly includes the spike
project you already have. Copying S7's conventions therefore reconfigures your existing test
project's runner as a side effect. That is usually what you want, but it should be a decision, not a
surprise, and the symptom if you miss it is a confusing filter failure rather than a build error.

Write the outcome down where a newcomer will actually hit it, which is `AGENTS.md` at the repo root,
not a document they have to go looking for.

## 2. Add the test-suite switch

`tests/Directory.Build.props` is what makes `dotnet test` mean something. Port it from S7 and adapt
the trait name if yours differs. It gives you three suites:

```bash
dotnet test                            # unit suite, the default, never touches the PLC
dotnet test -p:test-suite=integration  # requires the device
dotnet test -p:test-suite=all
```

The property that earns its keep is the validation target. An unrecognised value fails the build
instead of falling through to a filter that runs everything, so a typo cannot start hammering a live
controller.

Drop S7's `ProjectReference` to `ViciOne.Testing.Analyzers` unless you have that project too.

## 3. Create the addon project

`src/<Addon>` is a class library that compiles and does nothing. Give it the right identity and stop
there:

- `RootNamespace` and `AssemblyName` of `ViciOne.Suite.DataPort.<Addon>`
- `PackageReference` to `ViciOne.Suite.DataPort` and `ViciOne.Suite.DataPort.Extensions`
- a `metadata.json` carrying the addon id, with `__VERSION__` placeholders the pipeline substitutes

Resist the pull to add the protocol client here. On Allen-Bradley, `libplctag` stayed a dependency of
the test project, because choosing the driver is the walking skeleton's decision and putting it in
`src/` now would quietly pre-empt it.

Build it on its own before going further. If the feed or the central package versions are wrong, you
want to learn that from a two-file project.

## 4. Fold the spike into the real test project

The spike is already the test project you want, so rename it rather than creating a second one. On
Allen-Bradley, `tests/ConnectivityTests` became `tests/AllenBradley.Logix.Tests`.

The name is not cosmetic. `Directory.Build.props` grants `InternalsVisibleTo` to
`$(MSBuildProjectName).Tests`, so naming the test project after the addon project wires up internals
visibility with no further configuration.

Use `git mv` so the history survives, then:

1. Move the hardware tests into an `Integration/` folder and namespace, along with any helpers that
   exist only to serve them.
2. Retag them with the trait your `test-suite` switch filters on.
3. Swap the packages: out go `xunit`, `xunit.runner.visualstudio` and `Microsoft.NET.Test.Sdk`, in
   comes `xunit.v3.mtp-v2`.
4. Add a `ProjectReference` to the addon project.

The v2 to v3 source migration is smaller than it sounds. `[Fact]`, `[Trait]`, `Assert` and the
assertion library all carry over untouched. The one break on Allen-Bradley was `Xunit.Abstractions`,
which no longer exists; `ITestOutputHelper` now lives in `Xunit`.

Expect `TreatWarningsAsErrors` to be the noisy part. Spike code predates the conventions you have
just adopted, so it has never been compiled under them. Allen-Bradley's spike produced ten nullable
violations. Fix the code rather than relaxing the settings, and if a warning turns out to be wrong
about the world, say so in a comment where you suppress it. Two of those ten were a libplctag
interface that is nullable-oblivious and really does expect `null`, which is a `null!` and a
one-line comment, not a reason to turn the check off.

## 5. Put at least one test in the unit suite

The unit suite cannot be empty. MTP exits non-zero when a run discovers no tests, so an empty suite
fails CI on a build that is not actually broken. That is a hard constraint, not a stylistic one.

It collides with the other half of this phase. The addon compiles and does nothing, so there is, by
construction, nothing in it worth asserting. Allen-Bradley tried the obvious way out first, with
tests that checked the assembly was named after the addon id and that `metadata.json` agreed. They
passed, and they were dropped, because they tested the build system rather than the product and were
not worth the maintenance.

So the unit suite holds a single placeholder that asserts a tautology, clearly named and commented as
such. That is an honest description of where the project is: the suite exists, the plumbing works,
and there is nothing to test yet. It is preferable to inventing tests that look meaningful and are
not, and preferable to switching off MTP's empty-run guards, which would also hide a filter that has
quietly stopped matching anything.

Delete the placeholder in the next phase, when there is real behaviour to assert against. If it
survives past that point, something has gone wrong.

## 6. Verify, and mean it

Add both projects to the `.slnx`, then run the checks rather than assuming them:

```bash
dotnet build <solution>.slnx -c Debug     # succeeds from the repo root
dotnet test                               # green, and reports more than zero tests
dotnet test -p:test-suite=integration     # discovers the spike
dotnet test -p:test-suite=typo            # fails the build
```

Verify the clean-clone claim properly. Every restore on your machine hits the local package cache, so
a green build proves the build works *for you*, not that the feed serves it. Point the restore at an
empty cache and watch the packages actually come down:

```bash
NUGET_PACKAGES=/tmp/emptycache dotnet restore <solution>.slnx --force
```

**Run the integration suite against the real device before you call the phase done.** It is tempting
to skip, because the tests are slow and need the tunnel, and skipping it would have hidden a genuine
bug on Allen-Bradley. All three tests passed and the process still exited non-zero: `PlcTagLister`
created libplctag `Tag` objects and never disposed them, so native handles were released from
finalizers after the CLR had torn down, and the process fail-fasted on exit with `0xC0000602`. Under
the old VSTest host that was invisible. Under MTP the test assembly *is* the process, so its exit
code is the suite's exit code, and CI would have gone red on a fully passing run. Dispose every
`Tag`.

## What "done" looks like

The build works from a clean clone, both suites are green, the hardware tests are still there and
still excluded by default, and the conventions decision is written down. The addon itself still does
nothing, and the unit suite is a placeholder. Both of those are correct at this stage. Making the
addon do something is the walking skeleton, and that is what replaces the placeholder.
