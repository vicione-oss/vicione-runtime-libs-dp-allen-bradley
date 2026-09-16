using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Mapping;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Integers.DInt;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LinkedNodesDataFactory;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.NodePropertyFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.Nodes.DataPoints.Mapping;

public sealed class TagNamePropertyExtractorTests
{
    [Fact]
    public void TheTagNameIsReadOffTheTagNamePropertyAndNotOffTheNodeName()
    {
        // Arrange
        const string configuredTagName = "MotorSpeed";
        var node = CreateLinkedNode(DIntNode.LinkedNodeTypeId, "Motor", CreateTagName(configuredTagName));

        // Act
        var tagName = TagNamePropertyExtractor.GetTagName(node);

        // Assert
        tagName.Should().Be(new TagName(configuredTagName));
    }
}
