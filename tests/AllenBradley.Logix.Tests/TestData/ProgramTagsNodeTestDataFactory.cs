using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.Scope.ProgramTags;
using static System.Guid;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData;

internal static class ProgramTagsNodeTestDataFactory
{
    /// <summary>
    /// A program-scope container named <paramref name="programName"/>, hanging directly off the device.
    /// <paramref name="containerDesignId"/> decides the generation it holds its tags to, and defaults to
    /// the 5X70 one, matching <see cref="LogixCommunicationTestDataFactory.DeviceDesignId"/>.
    /// </summary>
    internal static Node CreateProgramTagsNode(string programName, string? containerDesignId = null) =>
        new()
        {
            DesignId = containerDesignId ?? ProgramTagsNode.Logix5X70LinkedNodeTypeId,
            Name = programName,
            Id = NewGuid(),
            Properties = new Dictionary<string, Property>
            {
                { ProgramTagsNode.ProgramNamePropertyName, new Property { Value = programName } },
            },
        };
}
