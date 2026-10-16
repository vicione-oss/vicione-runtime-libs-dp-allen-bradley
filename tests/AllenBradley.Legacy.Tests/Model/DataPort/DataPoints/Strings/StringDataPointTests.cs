using static ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.TestData.LegacyDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.Model.DataPort.DataPoints.Strings;

public sealed class StringDataPointTests
{
    [Theory]
    [InlineData("", "an empty string clears the element")]
    [InlineData("Batch 4711", "a short ASCII string fits")]
    [InlineData("\r\n\t", "control characters are ASCII too")]
    public void AnAsciiValueThatFitsTheElementIsInRange(string text, string inRangeBecause)
    {
        // Arrange
        var value = ST9Element(1).CreateLegacyValue(text);

        // Act
        var isInValueRange = value.IsInValueRange();

        // Assert
        isInValueRange.Should().BeTrue(inRangeBecause);
    }

    [Fact]
    public void AValueOf82CharactersIsInRange()
    {
        // Arrange
        var value = ST9Element(1).CreateLegacyValue(new string('x', 82));

        // Act
        var isInValueRange = value.IsInValueRange();

        // Assert
        isInValueRange.Should().BeTrue();
    }

    [Fact]
    public void AValueOf83CharactersIsOutOfRange()
    {
        // Arrange
        var value = ST9Element(1).CreateLegacyValue(new string('x', 83));

        // Act
        var isInValueRange = value.IsInValueRange();

        // Assert
        isInValueRange.Should().BeFalse();
    }

    [Theory]
    [InlineData("Grüße", "a Latin-1 letter is not ASCII")]
    [InlineData("€", "a character outside Latin-1 is not ASCII either")]
    public void AValueWithACharacterOutsideAsciiIsOutOfRange(string text, string outOfRangeBecause)
    {
        // Arrange
        var value = ST9Element(1).CreateLegacyValue(text);

        // Act
        var isInValueRange = value.IsInValueRange();

        // Assert
        isInValueRange.Should().BeFalse(outOfRangeBecause);
    }
}
