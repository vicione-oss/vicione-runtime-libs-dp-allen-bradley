# Testing Guide (MTP + xUnit v3)

The tests run on **Microsoft Testing Platform (MTP)** with **xUnit v3**, configured in
[`global.json`](../../../global.json). The MTP runner behaves differently from the classic VSTest
runner. Read this page before you run tests.

## Test projects

| Project                            | What it tests                                     | Integration tests against                 |
|------------------------------------|---------------------------------------------------|-------------------------------------------|
| `AllenBradley.Client.Shared.Tests` | The shared client, against fakes                  | none                                      |
| `AllenBradley.Logix.Tests`         | The Logix dataport                                | our CompactLogix 5069-L306ER              |
| `AllenBradley.Legacy.Tests`        | The Legacy dataport                               | libplctag's `ab_server`, started locally  |

## Test suites

The `test-suite` MSBuild property selects the suite. It is defined in
[`tests/Directory.Build.props`](../../../tests/Directory.Build.props), and it injects the matching
xUnit filter through `TestingPlatformCommandLineArguments`:

| Suite              | Command                                 | Injected filter                           |
|--------------------|-----------------------------------------|-------------------------------------------|
| Unit (**default**) | `dotnet test`                           | `--filter-not-trait Category=Integration` |
| Integration        | `dotnet test -p:test-suite=integration` | `--filter-trait Category=Integration`     |
| All                | `dotnet test -p:test-suite=all`         | none                                      |

Unit tests carry no trait. Integration tests carry `[Trait("Category", "Integration")]`, which they get
from the base class of their folder. If `test-suite` is not set, the unit suite runs. Any other value
fails the build with `Unknown test-suite '<value>'`. With `--no-build`, the validation target cannot
run. An unknown value then falls back to the unit filter, so a typo never talks to a controller.

In the IDE, the same suites are launch profiles of each test project, in
`Properties/launchSettings.json`. See the note about launch profiles under
[Key implications](#key-implications).

The injected filter **merges** with the filter that you give on the command line:

- The simple-style filters (`--filter-class`, `--filter-method`, `--filter-namespace`,
  `--filter-trait`) combine with it. `dotnet test --filter-class Foo` runs the unit tests of that class.
- `--filter-query` does **not** combine. xUnit v3 does not accept a mix of query-style and simple-style
  filters, and the run fails with zero tests. To use `--filter-query`, set `-p:test-suite=all`. This
  removes the injected filter.

## Key implications

- **No `--` separator.** Give the filter arguments directly to `dotnet test`. Write `--filter-class`,
  not `-- --filter-class`.
- **A project path needs `--project`.** `dotnet test <path/to.csproj>` fails with _"Specifying a
  project for 'dotnet test' should be via '--project'."_ Use `dotnet test --project <path/to.csproj>`
  or `--solution allen-bradley.slnx`.
- **No VSTest `--filter` syntax.** xUnit v3 in MTP mode does **not** accept
  `--filter "FullyQualifiedName~Foo"`. Use the xUnit filter options below.
- **Zero tests give exit code 8.** `AllenBradley.Client.Shared.Tests` has no integration tests. Its
  project file adds `--ignore-exit-code 8`, so that an integration run of the solution does not fail.
  If you add a test project without integration tests, add the same argument.
- **Launch profiles are for the IDE only.** Each test project has a committed
  `Properties/launchSettings.json` with the profiles `all-tests`, `unit-tests` and
  `integration-tests`. Use them to run a suite from Rider or Visual Studio.
  - Keep the **first profile free of arguments**. `dotnet test` merges the `commandLineArgs` of the
    first profile with the `test-suite` filter. A first profile with arguments breaks the suite
    selection, and `-p:test-suite=integration` then finds zero tests.
  - `dotnet run -lp <profile>` ignores `commandLineArgs`. On the command line, select a suite with
    `-p:test-suite=...` only.
- **`dotnet run` obeys `test-suite` too.** A test project is an executable, so
  `dotnet run --project tests/<Project>` starts the MTP runner. The SDK forwards
  `TestingPlatformCommandLineArguments`, so the injected filter applies as with `dotnet test`.
  - A bare `dotnet run` runs the **unit** suite. Integration tests need `-p:test-suite=integration`.
  - Put more filters after `--`:
    `dotnet run --project tests/<Project> -p:test-suite=integration -- --filter-class Foo`.
  - Do not give only a `--filter-...` after `--` without the property. The not-trait filter stays,
    and an integration target then gives zero tests.
  - Only a direct start of the built assembly bypasses the suite selection, and it finds all tests:
    `dotnet tests/<Project>/bin/Debug/net10.0/<Project>.dll`.

## Filter options

Use one style in one command. The query style and the simple style do not mix.

| Filter               | Example                                                  |
|----------------------|----------------------------------------------------------|
| `--filter-query`     | `--filter-query "/[Category!=Integration]"`              |
| `--filter-class`     | `--filter-class "Fully.Qualified.ClassName"`             |
| `--filter-method`    | `--filter-method "Fully.Qualified.ClassName.MethodName"` |
| `--filter-namespace` | `--filter-namespace "Some.Namespace"`                    |
| `--filter-trait`     | `--filter-trait "Category=Integration"`                  |
| `--filter-not-class` | `--filter-not-class "Fully.Qualified.ClassName"`         |

## Common commands

| Task                    | Command                                                                                                                                      |
|-------------------------|----------------------------------------------------------------------------------------------------------------------------------------------|
| Unit tests (default)    | `dotnet test`                                                                                                                                |
| Integration tests       | `dotnet test -p:test-suite=integration`                                                                                                      |
| All tests               | `dotnet test -p:test-suite=all`                                                                                                              |
| One test class          | `dotnet test --project tests/<Project>/<Project>.csproj --filter-class "ViciOne.Suite.DataPort.AllenBradley.<Port>.Tests.<Namespace>.<ClassName>"` |
| One test method         | `dotnet test --project tests/<Project>/<Project>.csproj --filter-method "ViciOne.Suite.DataPort.AllenBradley.<Port>.Tests.<Namespace>.<ClassName>.<MethodName>"` |
| One integration test    | the same as above, with `-p:test-suite=integration` or `-p:test-suite=all`                                                                   |
| Discovery only          | add `--list-tests` to a command above. It shows the tests that match, and it does not run them                                               |

To run one integration test, use one of the two runners. `--filter-method` merges with the injected
suite filter:

```bash
# dotnet test
dotnet test --project tests/AllenBradley.Legacy.Tests/AllenBradley.Legacy.Tests.csproj \
  -p:test-suite=integration \
  --filter-method "ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.Integration.DataFileRoundTripIntegrationTests.AnIntegerWrittenToAnIntegerFileIsReadBack"

# dotnet run — the same filter, after the -- separator
dotnet run --project tests/AllenBradley.Legacy.Tests/AllenBradley.Legacy.Tests.csproj \
  -p:test-suite=integration -- \
  --filter-method "ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.Integration.DataFileRoundTripIntegrationTests.AnIntegerWrittenToAnIntegerFileIsReadBack"
```

## Integration test requirements

- **Logix.** The tests talk to our CompactLogix 5069-L306ER. Read
  [Run the integration tests](../../AllenBradley.Logix.Documentation/how-to/run-the-integration-tests.md)
  for the address, the provisioned tags and the environment variables. The tests are in
  `TestControllerCollection`, which never runs in parallel, because they share one libplctag session. The
  `TestControllerFixture` connects before the first test and disposes the client after the last test.
- **Legacy.** The tests talk to libplctag's `ab_server`, which emulates an SLC 500. The binaries for
  `linux-x64` and `win-x64` are in `tests/AllenBradley.Legacy.Tests/Integration/ab_server/binaries/`. The
  `AbServer` fixture starts the server on a free loopback port and stops it after the last test, so
  no device and no network are necessary. The tests are in `AbServerCollection`, which never runs in
  parallel, because they write to the same data files.

## Test conventions

- Frameworks: xUnit v3, AwesomeAssertions (`.Should()` style) and NSubstitute.
- Use NSubstitute for interaction tests only, as in `ClientPoolTests`. Write stateful fakes by hand,
  for example a tag lookup or a captured write buffer.
- Dispose each client and each libplctag handle in the test. A handle that is left to its finalizer
  stops the test process.
