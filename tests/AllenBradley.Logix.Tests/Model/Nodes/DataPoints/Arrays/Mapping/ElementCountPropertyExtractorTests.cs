using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Integers.IntArray;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Mapping;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LinkedNodesDataFactory;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.NodePropertyFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.Nodes.DataPoints.Arrays.Mapping;

public sealed class ElementCountPropertyExtractorTests
{
    [Fact]
    public void TheElementCountIsReadOffTheElementCountProperty()
    {
        // Arrange
        const uint declaredElementCount = 10;
        var node = CreateLinkedNode(
            IntArrayDataPointNode.LinkedNodeTypeId, "Readings", CreateElementCount(declaredElementCount));

        // Act
        var elementCount = ElementCountPropertyExtractor.GetElementCount(node);

        // Assert
        elementCount.Should().Be(new ElementCount(declaredElementCount));
    }
}
