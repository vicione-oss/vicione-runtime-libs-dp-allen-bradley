using static ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.TestData.LegacyDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.Model.DataPort.DataPoints.Strings;

public sealed class AsciiDataPointTests
{
    [Theory]
    [InlineData("", "an empty value clears the element")]
    [InlineData("H", "one character fits")]
    [InlineData("Hi", "two characters fill the element")]
    [InlineData("\r\n", "control characters are ASCII too")]
    public void AnAsciiValueThatFitsTheElementIsInRange(string text, string inRangeBecause)
    {
        // Arrange
        var value = A9Element(0).CreateLegacyValue(text);

        // Act
        var isInValueRange = value.IsInValueRange();

        // Assert
        isInValueRange.Should().BeTrue(inRangeBecause);
    }

    [Fact]
    public void AValueOf3CharactersIsOutOfRange()
    {
        // Arrange
        var value = A9Element(0).CreateLegacyValue("abc");

        // Act
        var isInValueRange = value.IsInValueRange();

        // Assert
        isInValueRange.Should().BeFalse();
    }

    [Theory]
    [InlineData("ü", "a Latin-1 letter is not ASCII")]
    [InlineData("€", "a character outside Latin-1 is not ASCII either")]
    public void AValueWithACharacterOutsideAsciiIsOutOfRange(string text, string outOfRangeBecause)
    {
        // Arrange
        var value = A9Element(0).CreateLegacyValue(text);

        // Act
        var isInValueRange = value.IsInValueRange();

        // Assert
        isInValueRange.Should().BeFalse(outOfRangeBecause);
    }
}
