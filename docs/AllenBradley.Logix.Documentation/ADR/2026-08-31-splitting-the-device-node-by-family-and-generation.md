# Splitting the Device Node by Family and Generation

## Context and Problem Statement

The device node in `allen-bradley-logix.yaml` tells the dataport which controller it connects to. The C# types are in
`Model/Nodes/Device/`. This decision was made under
issue #9: Split the device node by controller family and generation.

The walking skeleton had one node, `Device`, with a `ControllerType` property: ControlLogix or CompactLogix. This
property changed nothing. libplctag has one PLC type for all Logix controllers, so the factory used
`PlcType.ControlLogix` for both.

Two differences between controllers are important, and the model had neither:

1. **The route path depends on the family.** A ControlLogix is in a 1756 chassis, and the controller can be in any
   slot. Thus, its route path must be configured. A CompactLogix has a virtual backplane with the controller at slot 0.
   Its route path is always `1,0`.
2. **The atomic types depend on the generation.** 5X70 controllers and older have `BOOL`, `SINT`, `INT`, `DINT`,
   `LINT` and `REAL`. 5X80 controllers add the unsigned integers and `LREAL`
   ([symbolic tag data types](../../AllenBradley.Documentation/protocol/allen-bradley-extension/symbolic-tag-data-types.md#what-logix-exposes)).
   A 5X70 configuration could use a 5X80 type, and nothing stopped it.

Do we model the family and the generation as properties of one node, or as one node type for each answer?

## Considered Options

1. **One `Device` node** with the family and the generation as properties.
2. **One node for each family,** with the generation as a property.
3. **One node for each family and generation.** This gives four node ids and one `DeviceNode` class in C#.
4. **One node for each catalog number,** for example `Device1756L71`.

## Decision Outcome

Chosen option: **Option 3**. A node type can change its property list and its child list, but a property can change
neither. The two differences need exactly this.

- The manifest has four device nodes: `DeviceControlLogix5X70`, `DeviceControlLogix5X80`, `DeviceCompactLogix5X70`
  and `DeviceCompactLogix5X80`. `s7-absolute.yaml` uses the same pattern.
- C# has one `DeviceNode`. The mapper gets the family and the generation from the `DesignId`. It rejects an unknown
  design id.
- The ControlLogix nodes have a route path property. The CompactLogix nodes do not, and the mapper sets `1,0` for them.
- The tag containers are split by generation. Only the 5X80 containers offer the 5X80 types. `DeviceNode` accepts only
  a container of its own generation.
- Thus, the editor cannot build a configuration that the dataport refuses later.
- `ControllerType` is removed. The family is not part of the client pool key (`LogixClientInformation`). For CIP, a
  ControlLogix and a CompactLogix at the same endpoint and route path are the same controller. With the family in the
  key, two ports on one controller with different node types would open two sessions.

### Consequences

- Good: The integrator selects the controller one time, with the node type. The editor asks no further questions about
  it.
- Good: C# has one device class, as in the S7 dataport.
- Bad: The node id is a configuration contract, and the `Device` node does not exist anymore. The dataport is not
  released, so no stored configuration is affected. After a release, a change like this needs a migration.
- Bad: Each tag container has one `MappingId` for each generation. The S7 dataport shares one `MappingId` for this
  case. Here, `CanBeAdded` of the container must know the generation. At that time the container has no parent yet,
  and its `MappingId` is the only thing that identifies it.
- Bad: Each container node type needs its own mapper class, because the YAML consistency test finds a mapper only by
  its `TargetLinkedNodeTypeId`.

## Why Not the Other Options

### Option 1: One `Device` node with properties

The editor would offer a route path to a CompactLogix, which has only one correct value. It would offer `LREAL` to a
5X70, which does not have this type. Rules at run time would refuse both, but only after the configuration is built.

### Option 2: One node for each family

It solves the route path. The types stay a rule at run time, the same as in Option 1.

### Option 4: One node for each catalog number

It needs many node ids, and one new id for each new part from Rockwell. The ids carry no information that the family
and the generation do not already carry.

## More Information

- Explanation: [Node model](../explanation/model/node-model.md)
- Background: [Controller families](../../AllenBradley.Documentation/controllers/controller-families.md) ·
  [Logix generations](../../AllenBradley.Documentation/controllers/logix-generations.md) ·
  [Chassis and route paths](../../AllenBradley.Documentation/controllers/chassis-and-route-paths.md)
- Terms: [CONTEXT.md](../../../CONTEXT.md), **controller family**, **controller generation** and **controller kind**
- Related decisions: [Maximizing throughput with one shared connection](2026-07-16-maximizing-throughput-with-one-shared-connection.md),
  the reason why the family is not part of the client pool key
