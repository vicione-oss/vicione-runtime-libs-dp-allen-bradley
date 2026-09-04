using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using Xunit.Sdk;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.Nodes.Device;

/// <summary>
/// A controller kind and the generation of a scope container configured under it: the one pairing a
/// suite about what a device admits varies. It serializes itself, so the runner names a row by the
/// controller and the container rather than by an argument list it cannot round-trip.
/// </summary>
public sealed record DeviceNodeWithContainerTestCase : IXunitSerializable
{
    /// <summary>Only xUnit calls this, and fills the instance through <see cref="Deserialize"/>.</summary>
    public DeviceNodeWithContainerTestCase()
    {
    }

    /// <param name="deviceNodeKind">The controller the container is configured under.</param>
    /// <param name="containerGenerationToAdd">The generation of the container to be added.</param>
    public DeviceNodeWithContainerTestCase(LogixControllerKind deviceNodeKind, LogixGeneration containerGenerationToAdd)
    {
        DeviceNodeKind = deviceNodeKind;
        ContainerGenerationToAdd = containerGenerationToAdd;
    }

    /// <summary>The controller the container is configured under.</summary>
    public LogixControllerKind DeviceNodeKind { get; private set; }

    /// <summary>The generation the container holds its tags to.</summary>
    public LogixGeneration ContainerGenerationToAdd { get; private set; }

    /// <summary>
    /// A controller kind is two enums rather than one, so it travels as its family and its generation
    /// and is put back together on the way in.
    /// </summary>
    public void Serialize(IXunitSerializationInfo info)
    {
        info.AddValue(nameof(LogixControllerKind.Family), DeviceNodeKind.Family);
        info.AddValue(nameof(LogixControllerKind.Generation), DeviceNodeKind.Generation);
        info.AddValue(nameof(ContainerGenerationToAdd), ContainerGenerationToAdd);
    }

    /// <inheritdoc cref="Serialize" />
    public void Deserialize(IXunitSerializationInfo info)
    {
        DeviceNodeKind = new LogixControllerKind(
            info.GetValue<LogixControllerFamily>(nameof(LogixControllerKind.Family)),
            info.GetValue<LogixGeneration>(nameof(LogixControllerKind.Generation)));
        ContainerGenerationToAdd = info.GetValue<LogixGeneration>(nameof(ContainerGenerationToAdd));
    }

    /// <summary>What the runner shows for a row.</summary>
    public override string ToString() =>
        $"{DeviceNodeKind.Family} {DeviceNodeKind.Generation}, {ContainerGenerationToAdd} container";
}
