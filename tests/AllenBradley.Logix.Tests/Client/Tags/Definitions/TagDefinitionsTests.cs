using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Definitions;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client.Tags.Definitions;

/// <summary>
/// Lookup and resolution against a decoded symbol table. Logix tag names are case-insensitive, so the
/// schema must match them that way.
/// </summary>
public class TagDefinitionsTests
{
    private static TagDefinition Dint(string name) =>
        new(new TagName(name), LogixTypeKind.Atomic, AllenBradleyDataType.Dint, MaxLength: null,
            new DimensionCount(0), new ElementCount(1));

    [Fact]
    public void Lookup_IsCaseInsensitive()
    {
        // Arrange
        var schema = new TagDefinitions(
            new Dictionary<TagName, TagDefinition>(TagName.CaseInsensitiveComparer)
            {
                [new TagName("Motor.Speed")] = Dint("Motor.Speed"),
            });

        // Act
        var declaration = schema.Lookup(new TagName("motor.speed"));

        // Assert
        declaration.Should().NotBeNull();
    }

    [Fact]
    public void Lookup_AnAbsentTag_ReturnsNull()
    {
        // Arrange
        var schema = new TagDefinitions(new Dictionary<TagName, TagDefinition>());

        // Act
        var declaration = schema.Lookup(new TagName("Nope"));

        // Assert
        declaration.Should().BeNull();
    }
}
