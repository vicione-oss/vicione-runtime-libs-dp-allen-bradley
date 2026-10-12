using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataFiles;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataFiles.IntegerFile;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.TestData;

/// <summary>Builds a configured integer file node as the engine delivers it, before it is mapped.</summary>
internal static class IntegerFileNodeTestDataFactory
{
    internal const ushort DefaultIntegerFileNumber = 7;

    /// <summary>An integer file numbered <see cref="DefaultIntegerFileNumber"/>.</summary>
    internal static Node IntegerFile() =>
        new()
        {
            DesignId = IntegerFileNode.LinkedNodeTypeId,
            Name = IntegerFileNode.LinkedNodeTypeId,
            Id = Guid.NewGuid(),
            Properties = new Dictionary<string, Property>
            {
                [DataFileNode.FileNumberPropertyName] = new() { Value = DefaultIntegerFileNumber },
            },
        };
}
