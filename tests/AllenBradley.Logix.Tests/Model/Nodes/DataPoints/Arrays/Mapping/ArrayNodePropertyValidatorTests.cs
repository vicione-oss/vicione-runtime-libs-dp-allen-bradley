using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Integers.IntArray;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Mapping;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LinkedNodesDataFactory;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.NodePropertyFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.Nodes.DataPoints.Arrays.Mapping;

/// <summary>The element-count rule on its own, without the tag-name and poll-frequency rules beside it.</summary>
public sealed class ArrayNodePropertyValidatorTests
{
    private readonly ArrayNodePropertyValidator _validator = new();

    [Theory]
    [InlineData(1u, "a single element is still an array")]
    [InlineData(10u, "the plain case")]
    [InlineData(uint.MaxValue, "the largest count the property can carry")]
    public void APositiveElementCountIsAccepted(uint elementCount, string validBecause)
    {
        // Arrange
        var node = ArrayNodeWith(CreateElementCount(elementCount));

        // Act
        var validation = _validator.Validate(node);

        // Assert
        validation.IsValid.Should().BeTrue(validBecause);
    }

    [Fact]
    public void AnElementCountOfZeroIsRefusedOnTheElementCountProperty()
    {
        // Arrange
        var node = ArrayNodeWith(CreateElementCount(0u));

        // Act
        var validation = _validator.Validate(node);

        // Assert
        validation.IsValid.Should().BeFalse("a tag that holds nothing is not an array tag");
        validation.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(LogixArrayDataPointNode.ElementCountPropertyName);
    }

    [Fact]
    public void AMissingElementCountIsRefusedNamingTheProperty()
    {
        // Arrange
        var node = ArrayNodeWith();

        // Act
        var validation = _validator.Validate(node);

        // Assert
        validation.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(LogixArrayDataPointNode.ElementCountPropertyName);
    }

    /// <summary>Values the manifest's UInt32 property cannot carry: text, and a signed integer.</summary>
    public static TheoryData<object> ValuesThatAreNotAnUnsignedInteger => ["10", -1];

    [Theory]
    [MemberData(nameof(ValuesThatAreNotAnUnsignedInteger))]
    public void AnElementCountThatIsNotAnUnsignedIntegerIsRefusedInsteadOfThrowing(object elementCount)
    {
        // Arrange
        var node = ArrayNodeWith(CreateElementCount(elementCount));

        // Act
        var validating = _validator.Invoking(validator => validator.Validate(node));

        // Assert
        validating.Should().NotThrow().Which.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(LogixArrayDataPointNode.ElementCountPropertyName);
    }

    private static LinkedNode ArrayNodeWith(params KeyValuePair<string, Property>[] properties) =>
        CreateLinkedNode(IntArrayDataPointNode.LinkedNodeTypeId, "Readings", properties);
}
