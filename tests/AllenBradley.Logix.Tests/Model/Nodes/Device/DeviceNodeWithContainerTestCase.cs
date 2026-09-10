using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using Xunit.Sdk;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.Nodes.Device;

/// <summary>A controller kind paired with the generation of a scope container configured under it.</summary>
public sealed record DeviceNodeWithContainerTestCase : IXunitSerializable
{
    /// <summary>Only xUnit calls this, and fills the instance through <see cref="Deserialize"/>.</summary>
    public DeviceNodeWithContainerTestCase()
    {
    }

    public DeviceNodeWithContainerTestCase(LogixControllerKind deviceNodeKind, LogixGeneration containerGenerationToAdd)
    {
        DeviceNodeKind = deviceNodeKind;
        ContainerGenerationToAdd = containerGenerationToAdd;
    }

    public LogixControllerKind DeviceNodeKind { get; private set; }

    public LogixGeneration ContainerGenerationToAdd { get; private set; }

    /// <summary>A controller kind is two enums, so it travels flattened and is put back together.</summary>
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

    public override string ToString() =>
        $"{DeviceNodeKind.Family} {DeviceNodeKind.Generation}, {ContainerGenerationToAdd} container";
}
