# Run the integration tests

The Logix integration tests run against our CompactLogix 5069-L306ER located in the ViciOne-office. The controller is ours, so we provision it
to fit the suite. A test that needs a tag gets the tag on the controller, and new integration coverage belongs
there for that reason.

## How the integration tests are laid out

All integration tests are in `tests/AllenBradley.Logix.Tests/Integration/CompactLogix5X80/`. They form one
collection, `TestControllerCollection`, which never runs in parallel. The tests share one controller over one
libplctag session, and concurrent operations on that session collide.

The suite connects once. `TestControllerFixture` connects before the first test of the collection and disposes
the client after the last. All tests use the same client, so the tag cache stays filled from one test to the next,
as it does in a deployment.

## The CompactLogix 5069-L306ER

The controller is a CompactLogix 5380 with 600 KB of user memory, which this repo calls a 5X80. It runs firmware
revision 36.11 and the project in
[`ViciOne.L5X`](../../../tests/AllenBradley.Logix.Tests/Integration/CompactLogix5X80/Infrastructure/Provisioning/ViciOne.L5X).

| Slot | Catalog number | Module                                     |
|------|----------------|--------------------------------------------|
| 0    | 5069-L306ER    | CompactLogix 5380 controller, 600 KB       |
| 1    | 5069-IB16      | Compact 5000 DC input module, 16 channels  |
| 2    | 5069-OB16      | Compact 5000 DC output module, 16 channels |

The project does not configure the two I/O modules yet, and no test uses them.

The tests in `Integration/CompactLogix5X80/` target this controller. For every type the port implements, they
write a value, read it back and check the declaration that the controller reports for the tag. They cover scalars,
whole arrays, single array elements, `STRING` at two capacities, `TIMER`, `COUNTER`, and the members of two UDTs,
one nested in the other. Most tests run twice, once against the controller-scoped tags and once against the tags
of `MainProgram`.

Two suites check the client stack against raw libplctag handles on the `DINT` tag.
`LibplctagCrossCheckIntegrationTests` writes with one side and reads with the other, in both directions.
`SharedAccessConcurrencyIntegrationTests` sends concurrent reads and writes to one handle. It writes back the
current value of the tag, so the value does not change.

### The provisioned project

Two files next to the suite hold the project on the controller:

| File                 | What it is                                                                         |
|----------------------|------------------------------------------------------------------------------------|
| `ViciOne.L5X`        | The project the controller was provisioned with. Edit this file when a tag changes |
| `ViciOne_export.ACD` | The project as a Studio 5000 archive, saved after the download                     |

To provision a controller again, open `ViciOne.L5X` with **File → Open** in Studio 5000 and download the project.
Then save the project over `ViciOne_export.ACD`.

Keep `ViciOne.L5X` in step with `TagAddresses.cs`. When you add a tag, a data type or a member to one, add it to
the other. If a suite needs a tag that the controller lacks, add the tag and keep the test unchanged.

The suite takes its assumptions about the controller from two source files. `TagAddresses.cs` lists the tags.
`TestControllerConfiguration.cs` says how to reach the controller from the machine that runs the suite.

### `TagAddresses.cs`, the tags

Every tag exists once in controller scope and once in `MainProgram`, under a name that shows the scope:
`DIntControllerTag` and `Program:MainProgram.DIntProgramTag`. The name starts with the node that the
configuration editor adds for the tag. The `DInt` node reaches `DIntControllerTag`, and the `DInt[]` and
`DInt[i]` nodes reach `DIntArrayControllerTag`. The suffix also stops a name from equalling a data type name,
which Studio 5000 may refuse because Logix names are case-insensitive. UDT members carry the suffix `Tag` only
where the name would equal a type, so `UdtControllerTag.DIntTag` stands next to `UdtControllerTag.DIntArray`.

One tag per type in each scope:

| Type        | Controller scope        | Program scope, `MainProgram` | Note                   |
|-------------|-------------------------|------------------------------|------------------------|
| `BOOL`      | `BoolControllerTag`     | `BoolProgramTag`             |                        |
| `SINT`      | `SIntControllerTag`     | `SIntProgramTag`             |                        |
| `INT`       | `IntControllerTag`      | `IntProgramTag`              |                        |
| `DINT`      | `DIntControllerTag`     | `DIntProgramTag`             |                        |
| `LINT`      | `LIntControllerTag`     | `LIntProgramTag`             |                        |
| `USINT`     | `USIntControllerTag`    | `USIntProgramTag`            |                        |
| `UINT`      | `UIntControllerTag`     | `UIntProgramTag`             |                        |
| `UDINT`     | `UDIntControllerTag`    | `UDIntProgramTag`            |                        |
| `ULINT`     | `ULIntControllerTag`    | `ULIntProgramTag`            |                        |
| `REAL`      | `RealControllerTag`     | `RealProgramTag`             |                        |
| `LREAL`     | `LRealControllerTag`    | `LRealProgramTag`            |                        |
| `STRING`    | `StringControllerTag`   | `StringProgramTag`           | 82 characters          |
| `STRING_20` | `String20ControllerTag` | `String20ProgramTag`         | 20 characters          |
| `TIMER`     | `TimerControllerTag`    | `TimerProgramTag`            | used by no instruction |
| `COUNTER`   | `CounterControllerTag`  | `CounterProgramTag`          | used by no instruction |
| `UDT_Outer` | `UdtControllerTag`      | `UdtProgramTag`              |                        |

One array per element type in each scope, each ten elements long except the `BOOL` array:

| Type        | Controller scope          | Program scope, `MainProgram` | Note                   |
|-------------|---------------------------|------------------------------|------------------------|
| `BOOL[32]`  | `BoolArrayControllerTag`  | `BoolArrayProgramTag`        |                        |
| `SINT[10]`  | `SIntArrayControllerTag`  | `SIntArrayProgramTag`        |                        |
| `INT[10]`   | `IntArrayControllerTag`   | `IntArrayProgramTag`         |                        |
| `DINT[10]`  | `DIntArrayControllerTag`  | `DIntArrayProgramTag`        |                        |
| `LINT[10]`  | `LIntArrayControllerTag`  | `LIntArrayProgramTag`        |                        |
| `USINT[10]` | `USIntArrayControllerTag` | `USIntArrayProgramTag`       |                        |
| `UINT[10]`  | `UIntArrayControllerTag`  | `UIntArrayProgramTag`        |                        |
| `UDINT[10]` | `UDIntArrayControllerTag` | `UDIntArrayProgramTag`       |                        |
| `ULINT[10]` | `ULIntArrayControllerTag` | `ULIntArrayProgramTag`       |                        |
| `REAL[10]`  | `RealArrayControllerTag`  | `RealArrayProgramTag`        |                        |
| `LREAL[10]` | `LRealArrayControllerTag` | `LRealArrayProgramTag`       |                        |
| `TIMER[10]` | `TimerArrayControllerTag` | `TimerArrayProgramTag`       | used by no instruction |

One `DINT` named `DIntTag` exists in both scopes under the same name. `ScopeIntegrationTests` uses it to check
that the program tag and the controller tag are separate tags.

The `BOOL` arrays are 32 long because Studio 5000 packs the bits into 32-bit words and offers no length that is
not a multiple of 32.

Three data types, each declared before the types that use it:

| Data type   | Members, in this order                                                                                                                                                                                                                                 |
|-------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `STRING_20` | A user string type of 20 characters                                                                                                                                                                                                                    |
| `UDT_Inner` | `BoolTag : BOOL`, `RealTag : REAL`, `IntArray : INT[10]`, `StringTag : STRING`                                                                                                                                                                         |
| `UDT_Outer` | `BoolTag : BOOL`, `BoolNeighbour : BOOL`, `DIntTag : DINT`, `StringTag : STRING`, `String20 : STRING_20`, `DIntArray : DINT[10]`, `BoolArray : BOOL[32]`, `TimerTag : TIMER`, `TimerArray : TIMER[10]`, `CounterTag : COUNTER`, `InnerUdt : UDT_Inner` |

`UDT_Outer` has one member of each kind that its template declares differently: a packed `BOOL`, an atomic value,
a string of each capacity, two arrays, a timer, a timer array, a counter and a nested UDT. More atomic members
would only repeat the tag suites. A member's type decodes the same way as a tag's, and the suite reads its value
through its own address. `BoolTag` and `BoolNeighbour` share one hidden byte, so the suite can check that writing
one bit leaves the other bit alone.

The tag list is a set of constants. Every controller that the suite runs against holds the same tags, so the list
belongs in the source and in review. Only the way to reach the controller comes from the environment.

The suites run in both scopes because the controller finds the two differently. It lists a program tag under a
program-qualified name and a controller tag under its bare name, and the configuration tree composes the two
addresses differently. Each scope has its own names, so a program-scope test that loses its `Program:MainProgram.`
prefix finds no tag and fails. With shared names, it would reach the controller tag and pass.

> **The suite writes every one of these tags.** Provision them so that nothing in the controller program depends
> on them. This costs nothing on our controller, because no other program runs on it.

No `TON`, `TOF` or `RTO` may use a `Timer…Tag` tag, any element of a `TimerArray…Tag` tag, or the `TimerTag` and
`TimerArray` members of a `Udt…Tag` tag. The suite writes their `.ACC` and `.PRE` and expects `.DN` to read
clear. That expectation holds only while no instruction runs the timer. In the same way, no `CTU` or `CTD` may
use a `Counter…Tag` tag or the `CounterTag` member.

`MissingTimer` and `MissingCounter` must not exist in either scope, and `UDT_Outer` must have no member named
`MissingMember`. The suite configures them to check that connect reports them missing.

### `TestControllerConfiguration.cs`, how the controller is reached

The way to reach the controller varies by machine, so it is configurable. The suite assumes a direct connection.
The endpoint is the controller itself, on the EtherNet/IP port, over the virtual backplane.

| Variable                   | Default         | Description                                                                  |
|----------------------------|-----------------|------------------------------------------------------------------------------|
| `CIP_5X80_GATEWAY`         | `10.100.11.234` | Controller IP address. libplctag calls the device it connects to its gateway |
| `CIP_5X80_PORT`            | `44818`         | TCP port the EtherNet/IP session is opened on                                |
| `CIP_5X80_PATH`            | `1,0`           | CIP route path to the CPU                                                    |
| `CIP_5X80_TIMEOUT_SECONDS` | `10`            | How long one tag read or write may take                                      |

The controller's address is `10.100.11.234`, its host name is `allenbradley-5069-1`, and the router of its network
is `10.100.1.1`. The router is part of the network settings of the controller. The suite connects to the
controller directly and does not use the router. The default is the IP address because it works without DNS. To
connect by name, set `CIP_5X80_GATEWAY=allenbradley-5069-1`. If the controller moves, or ends up behind a tunnel
or a bridge, `TestControllerConfiguration.cs` is the only file that changes.

Run `GeneralIntegrationTests` first against a newly provisioned controller. Its
`EveryConfiguredTagPassesVerification` resolves every configured tag against the symbol table in one pass and
reports each disagreement by name (missing, wrong type, wrong shape or wrong capacity). You get a readable list of
wrong assumptions. The round trips would give one failure per type, and every failure would say that a read came
back `Bad`.

`ATagOfTheWrongTypeIsReported` covers the other half, a tag that exists but has a different declaration than the
configuration. No round trip catches that for any type, because the read decodes the bytes as the configured
type. The unsigned integers are the hardest case. Each has the same width as its signed twin, so neither
direction gets even the buffer-length backstop that a width mismatch would give.

## Run the tests

Run only the integration tests (the controller must be reachable):

```sh
dotnet test -p:test-suite=integration
```

Run only the hardware-free tests. It is the default suite, and CI runs it:

```sh
dotnet test
```

> The classic VSTest `--filter "Category=..."` syntax does **not** work in this repo. Tests run on Microsoft
> Testing Platform with xUnit v3, and the `test-suite` MSBuild property selects the suite. See the
> [testing guide](../../AllenBradley.Documentation/conventions/testing-guide.md).
