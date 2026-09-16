using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Definitions;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.TagDefinitionTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client.Tags.Definitions;

public sealed class TagDefinitionsTests
{
    private static readonly TagAddress SpeedTagAddress = new("Motor.Speed");

    private static readonly TagDefinition SpeedDefinition =
        DefaultAtomicTagDefinition() with { TagAddress = SpeedTagAddress };

    [Fact]
    public void ATagNameIsLookedUpWithoutRegardToCase()
    {
        // Arrange
        var definitions = DefinitionsHolding(SpeedDefinition);

        // Act
        var found = definitions.Lookup(new TagAddress("motor.speed"));

        // Assert
        found.Should().Be(SpeedDefinition);
    }

    [Fact]
    public void ATagAbsentFromTheDefinitionsIsNotFound()
    {
        // Arrange
        var definitions = DefinitionsHolding();

        // Act
        var found = definitions.Lookup(new TagAddress("Nope"));

        // Assert
        found.Should().BeNull();
    }

    private static TagDefinitions DefinitionsHolding(params TagDefinition[] definitions) =>
        new(definitions.ToDictionary(definition => definition.TagAddress, TagAddress.CaseInsensitiveComparer));
}
