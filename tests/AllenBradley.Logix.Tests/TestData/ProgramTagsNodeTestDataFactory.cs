using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.ProgramTags;
using static System.Guid;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData;

/// <summary>Builds the program-scope container a configured program tag hangs off.</summary>
internal static class ProgramTagsNodeTestDataFactory
{
    /// <summary>
    /// A program-scope container named <paramref name="programName"/>, hanging directly off the device.
    /// </summary>
    /// <param name="programName">The program whose tags the container holds.</param>
    /// <param name="containerDesignId">
    /// The container's node type, which decides the generation it holds its tags to. Defaults to the 5X70
    /// one, matching <see cref="LogixCommunicationTestDataFactory.DeviceDesignId"/>.
    /// </param>
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
