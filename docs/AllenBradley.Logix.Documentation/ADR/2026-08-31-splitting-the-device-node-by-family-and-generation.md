# Splitting the Device Node by Family and Generation

## Context and Problem Statement

This decision covers the device node in `allen-bradley-logix.yaml` and the
`Model/Nodes/Device/` types behind it. It settles how an integrator tells the dataport which controller
they are pointing it at. It was made under
[issue #9: Split the device node by controller family and generation](https://gitlab.com/vicione-oss/addons/allen-bradley/cip/-/work_items/9).

The walking skeleton shipped one device node, `Device`, with a `ControllerType` property offering
ControlLogix and CompactLogix. That property changed no behaviour: the access factory mapped both of
its members onto `PlcType.ControlLogix`, because libplctag has one PLC type for the whole Logix-5000
line. It was a required question whose answer was never read.

Two things about a controller do matter, and neither was modelled.

The **route path** differs by family. ControlLogix is the 1756 chassis line, so the CPU sits in
whichever slot whoever assembled the chassis put it in, and the path has to be configured. A
CompactLogix clips onto a DIN rail whose virtual backplane places the controller at slot 0, so its
path is always `1,0`. Both nodes asked for a path, and a CompactLogix has no answer but the default —
a question with one legal answer, and an invitation to get it wrong.

The **atomic type vocabulary** differs by generation. The 5X70 controllers and everything before them
have `BOOL`, `SINT`, `INT`, `DINT`, `LINT` and `REAL`. The 5X80 controllers add the unsigned integers
and `LREAL`. See
[symbolic-tag-data-types.md §2](../../AllenBradley.Documentation/protocol/allen-bradley-extension/symbolic-tag-data-types.md#2-what-logix-exposes).
Nothing stopped a 5X70 from being configured with a type it has not got, and the failure would have
surfaced as a tag the controller could not resolve.

So: does the dataport ask these as properties on one node, or declare a node type per answer?

## Considered Options

- **Option 1: One `Device` node, family and generation as properties.** The shape the skeleton had,
  extended with a second dropdown. One node id, one property list, one child list.
- **Option 2: One node per family, generation as a property.** The path question is solved by the node
  type; the type vocabulary stays a property the mapper reads.
- **Option 3: One node per family and generation.** Four node ids —
  `DeviceControlLogix5X70`, `DeviceControlLogix5X80`, `DeviceCompactLogix5X70`,
  `DeviceCompactLogix5X80` — one `DeviceNode` in C#, told apart by `DesignId`.
- **Option 4: One node per catalog number.** `Device1756L71`, `Device1769L32E`, and so on.

## Decision Outcome

Chosen: **Option 3.**

The manifest declares four device nodes and the C# side keeps one `DeviceNode`, carrying a
`LogixControllerFamily` and a `LogixGeneration` that `DeviceNodeMapper` resolves from
`communication.DesignId`. This is the shape `s7-absolute.yaml` already uses for its seven
`DeviceNNNN` ids, so a dataport developer moving between the two repos finds the same thing.

What decides it is that a node type can vary its **property list** and its **child list**, and a
property cannot vary either. That is exactly what the two differences need:

- The ControlLogix nodes declare `Path`; the CompactLogix nodes do not, and the mapper supplies
  `Path.VirtualBackplane`. `LogixCommunicationValidator` holds a ControlLogix to declaring one, and a
  CompactLogix to nothing.
- `ControllerTags` splits into `ControllerTags5X70` and `ControllerTags5X80`. Only the child list
  differs: the 5X80 container offers `LReal`, the 5X70 one does not.

Under Option 1 both of those would have had to be runtime rules on a tree the editor still offers in
full, which means an integrator can build a configuration the dataport then refuses. Under Option 3 the
editor cannot offer it in the first place.

`ControllerType` is deleted rather than renamed. The family is now the node type, and it deliberately
does **not** reach `LogixClientInformation`: that record is the client pool's key, so a field on it
that no connection depends on would open a second session whenever two ports on one controller were
configured under different node types. A ControlLogix and a CompactLogix at the same gateway and path
are the same controller as far as CIP is concerned, and they compare equal.

Option 4 is rejected. It multiplies node ids without adding information — nothing behind CIP
distinguishes a 5570 from a 5580 beyond the two axes above — and it would need a new node for every
part number Rockwell ships.

### Consequences

The node id is a configuration contract, so this is the expensive half of the decision. `Device` no
longer exists, and a configuration referencing it maps to nothing. `DeviceNodeMapper` rejects an
unrecognised design id outright rather than guessing, because a device node type with no family and
no generation has nothing to fall back on. The dataport is unreleased, so no stored configuration is
affected today; after a release this rename would need a migration.

`DeviceNode.TypeOf` and the manifest's device nodes are the same set, and nothing enforces that but
a test. Adding a fifth device node means adding an arm there too.

The two tag containers carry **two** `MappingId`s rather than sharing one, which is a deviation from
the `Plc1200`/`Plc1500` pattern S7 uses for the same situation. It is what puts the generation on the
mapped `ControllerTagsNode`, so the container can gate its own children in `CanBeAdded`.

Sharing one `MappingId` was tried first and does not work. A `LinkedNode` carries the `MappingId` as
its `DesignId` and nothing else that identifies the manifest node, so a shared id leaves the container
unable to tell which of the two it came from. Reaching upward instead is no better: the engine
completes a container — maps it, attaches its data points — before it attaches the container to
anything, so `ParentConfigurationNode` is still null while `CanBeAdded` runs. Both were established by
probe, not by reading.

The cost is a second mapper class. One mapper could claim both ids by overriding `IsTargetMapperFor`,
and that works at run time, but the YAML consistency test resolves a node's mapper by
`TargetLinkedNodeTypeId` alone and reports the second node as unmapped. So
`ControllerTagsNodeMapper` is an abstract base carrying the generation, with a four-line subclass per
node type. `ControllerTagsNode.Generation` is the one branch-node member that is not a declared
property, and it is excluded in `LogixYamlConsistencyTests` for that reason.

### Enforcement

The YAML consistency test holds the manifest and the node model to each other. Beyond it, compliance
is a code-review check on `DeviceNode`: a new device node type is a manifest node **and** an arm in
`TypeOf`, and anything the family or generation decides is read off the `DeviceNode`, never
configured a second time as a property.

Two node types carrying the generation means a configuration can disagree with itself, and the
container is the half that is believed: it gates its tags on its own node type without ever seeing
the device. `DeviceNode.CanBeAdded(IConfigurationNode)` is where the two meet, so it refuses a
container whose generation is not the device's — the pairing is not one the editor can build, which
is exactly what is already true of the `LREAL` the container turns away.

## Pros and Cons of the Options

### Option 1: One `Device` node, family and generation as properties (rejected)

#### Pros

One node id, so nothing to migrate and nothing to keep in step with a switch in C#. It is also the
only option under which the editor tree is the same whatever controller is configured, which is
simpler to reason about if the differences did not matter.

#### Cons

The differences do matter, and a property cannot express either of them. The editor would offer
`Path` to a CompactLogix that has one legal answer, and `LREAL` to a 5X70 that has no such type —
both refused later, by rules that fire after the configuration is built rather than while it is being
built. It also asks two questions whose answers the integrator has already given by choosing the
controller they are configuring.

### Option 2: One node per family, generation as a property (rejected)

#### Pros

It solves the path question, which is the more visible of the two, at half the node ids.

#### Cons

It leaves the type vocabulary as a runtime rule for the same reason Option 1 does, and it splits the
model along one axis while leaving the other as a property — which is the harder shape to explain of
the three.

### Option 3: One node per family and generation (chosen)

#### Pros

Both differences become editor-time facts: a CompactLogix is not asked for a path, and a 5X70 is not
offered an `LREAL`. The integrator answers once, by picking the node that names their controller. The
C# side stays one `DeviceNode` with one switch, and the shape matches the S7 repo's.

#### Cons

Four node ids to keep in step with one switch, and a manifest whose device section is four nodes
rather than one — mitigated by YAML anchors, since the four share two property shapes. It also
renames the only device node there was.

### Option 4: One node per catalog number (rejected)

#### Cons

Dozens of node ids carrying no information the two axes do not already carry, and a new one for every
part number Rockwell ships. The catalog number is narrower than anything the dataport acts on.

## More Information

- [`controller-families-and-routing.md`](../../AllenBradley.Documentation/protocol/allen-bradley-extension/controller-families-and-routing.md)
  — the two form factors, the lines, and why a chassis controller's path is not guessable
- [`symbolic-tag-data-types.md`](../../AllenBradley.Documentation/protocol/allen-bradley-extension/symbolic-tag-data-types.md)
  — the two generations' type sets
- [Maximizing throughput with one shared connection](2026-07-16-maximizing-throughput-with-one-shared-connection.md)
  — why the family stays off `LogixClientInformation`
- [`CONTEXT.md`](../../../CONTEXT.md) — **controller family** and **controller generation**
- [Issue #9: Split the device node by controller family and generation](https://gitlab.com/vicione-oss/addons/allen-bradley/cip/-/work_items/9)
