using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration.Templates;
using System.Collections.Immutable;
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

    private static readonly TemplateDefinition LineTemplate = new(
        new TemplateId(0x123), new TemplateName("Line"), new StructureHandle(0xABCD), new StructureSize(4), []);

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
        var definitions = DefinitionsHolding(tags: []);

        // Act
        var found = definitions.Lookup(new TagAddress("Nope"));

        // Assert
        found.Should().BeNull();
    }

    [Fact]
    public void ATemplateIsLookedUpByItsId()
    {
        // Arrange
        var definitions = DefinitionsHolding(LineTemplate);

        // Act
        var found = definitions.LookupTemplate(LineTemplate.Id);

        // Assert
        found.Should().Be(LineTemplate);
    }

    [Fact]
    public void ATemplateAbsentFromTheDefinitionsIsNotFound()
    {
        // Arrange
        var definitions = DefinitionsHolding(templates: []);

        // Act
        var found = definitions.LookupTemplate(new TemplateId(0x999));

        // Assert
        found.Should().BeNull();
    }

    private static TagDefinitions DefinitionsHolding(params TagDefinition[] tags) =>
        new(
            tags.ToDictionary(tag => tag.TagAddress, TagAddress.CaseInsensitiveComparer),
            ImmutableDictionary<TemplateId, TemplateDefinition>.Empty);

    private static TagDefinitions DefinitionsHolding(params TemplateDefinition[] templates) =>
        new(
            ImmutableDictionary<TagAddress, TagDefinition>.Empty,
            templates.ToDictionary(template => template.Id));
}
