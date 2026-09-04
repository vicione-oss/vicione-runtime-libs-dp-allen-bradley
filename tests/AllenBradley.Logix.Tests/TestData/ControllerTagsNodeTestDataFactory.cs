using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.ControllerTags;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Device;
using static System.Guid;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData;

/// <summary>Builds the controller-scope container a configured controller tag hangs off.</summary>
internal static class ControllerTagsNodeTestDataFactory
{
    /// <summary>
    /// The controller-scope container of the generation <paramref name="deviceDesignId"/> names — the
    /// pairing the editor can build, and the only one the device accepts.
    /// </summary>
    internal static Node CreateControllerTagsNodeFor(
        string deviceDesignId = DeviceNode.ControlLogix5X70DesignId) =>
        new()
        {
            DesignId = ControllerTagsNode.LinkedNodeTypeIdFor(
                DeviceNode.KindOf(deviceDesignId)!.Value.Generation),
            Name = "Controller",
            Id = NewGuid(),
        };
}
