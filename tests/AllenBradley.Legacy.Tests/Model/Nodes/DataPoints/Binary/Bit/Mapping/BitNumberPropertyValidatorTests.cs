using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataPoints.Binary.Bit;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataPoints.Binary.Bit.Mapping;
using static ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.TestData.BitNodeTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.Model.Nodes.DataPoints.Binary.Bit.Mapping;

public sealed class BitNumberPropertyValidatorTests
{
    private readonly BitNumberPropertyValidator _validator = new();

    [Theory]
    [InlineData((ushort)0, "the least significant bit of an element")]
    [InlineData((ushort)15, "the most significant bit of an element")]
    public void ABitNumberFromZeroToFifteenIsAccepted(ushort bitNumber, string validBecause)
    {
        // Arrange
        var node = BitWith(BitNumberProperty(bitNumber));

        // Act
        var validation = _validator.Validate(node);

        // Assert
        validation.IsValid.Should().BeTrue(validBecause);
    }

    [Fact]
    public void ABitNumberPastTheLastBitOfAnElementIsRefusedOnTheBitNumberProperty()
    {
        // Arrange
        var node = BitWith(BitNumberProperty(16));

        // Act
        var validation = _validator.Validate(node);

        // Assert
        validation.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(BitNode.BitNumberPropertyName);
    }

    [Fact]
    public void AMissingBitNumberIsRefusedNamingTheProperty()
    {
        // Arrange
        var node = BitWith();

        // Act
        var validation = _validator.Validate(node);

        // Assert
        validation.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(BitNode.BitNumberPropertyName);
    }
}
