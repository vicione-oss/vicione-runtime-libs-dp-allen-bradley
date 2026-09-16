using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.Scope.ProgramTags;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.DataPort.DataPoints;

public sealed class TagPathTests
{
    private static readonly TagPath ThirdReadingInMain =
        new(new ProgramName("Main"), new TagName("Readings"), new ElementIndex(3));

    [Fact]
    public void AControllerScopedTagAddressesItselfByName()
    {
        // Arrange
        var path = new TagPath(Program: null, new TagName("Count"), Element: null);

        // Act
        var tagAddress = path.ToTagAddress();

        // Assert
        tagAddress.Should().Be(new TagAddress("Count"));
    }

    [Fact]
    public void AProgramScopedElementAddressesItselfBehindItsProgramAndArray()
    {
        // Arrange

        // Act
        var tagAddress = ThirdReadingInMain.ToTagAddress();

        // Assert
        tagAddress.Should().Be(new TagAddress("Program:Main.Readings[3]"));
    }

    [Fact]
    public void AnElementIsDeclaredAsItsArray()
    {
        // Arrange

        // Act
        var declaredTagAddress = ThirdReadingInMain.TagDefinitionAddress;

        // Assert
        declaredTagAddress.Should().Be(new TagAddress("Program:Main.Readings"));
    }

    [Fact]
    public void ATagThatIsNoElementIsDeclaredAsItself()
    {
        // Arrange
        var path = ThirdReadingInMain with { Element = null };

        // Act
        var declaredTagAddress = path.TagDefinitionAddress;

        // Assert
        declaredTagAddress.Should().Be(path.ToTagAddress());
    }

    [Theory]
    [InlineData("Count")]
    [InlineData("Program:Main.Count")]
    [InlineData("Readings[3]")]
    [InlineData("Program:Main.Readings[3]")]
    [InlineData("Motor.Speed")]
    public void AnAddressParsesBackIntoThePathThatRendersIt(string address)
    {
        // Arrange

        // Act
        var path = TagPath.Parse(address);

        // Assert
        path.ToTagAddress().Should().Be(new TagAddress(address));
    }

    [Fact]
    public void ParsingFillsEveryPart()
    {
        // Arrange

        // Act
        var path = TagPath.Parse("Program:Main.Readings[3]");

        // Assert
        path.Should().Be(ThirdReadingInMain);
    }

    [Theory]
    [InlineData("Readings[05]", "a leading zero")]
    [InlineData("Readings[-1]", "a negative index")]
    [InlineData("Readings[1,2]", "two dimensions")]
    [InlineData("Readings[5].Value", "a member behind the subscript")]
    [InlineData("", "nothing")]
    public void AnAddressThatIsNotATagOrOneElementOfOneIsRefused(string address, string because)
    {
        // Arrange

        // Act
        var parsing = () => TagPath.Parse(address);

        // Assert
        parsing.Should().Throw<FormatException>(because);
    }
}
