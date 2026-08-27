using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Definitions;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client.Tags.Definitions;

/// <summary>
/// Lookup and resolution against a decoded symbol table. Logix tag names are case-insensitive, so the
/// schema must match them that way.
/// </summary>
public class TagDefinitionsTests
{
    private static TagDefinition Dint(string name) =>
        new(new TagName(name), LogixTypeKind.Atomic, AllenBradleyDataType.Dint, MaxLength: null, new DimensionCount(0), new ElementCount(1));

    [Fact]
    public void Lookup_IsCaseInsensitive()
    {
        var schema = new TagDefinitions(
            new Dictionary<TagName, TagDefinition>(TagName.CaseInsensitiveComparer)
            {
                [new TagName("Motor.Speed")] = Dint("Motor.Speed"),
            });

        schema.Lookup(new TagName("motor.speed")).Should().NotBeNull();
    }

    [Fact]
    public void Lookup_AnAbsentTag_ReturnsNull()
    {
        var schema = new TagDefinitions(new Dictionary<TagName, TagDefinition>());

        schema.Lookup(new TagName("Nope")).Should().BeNull();
    }

    [Fact]
    public void Resolve_APresentTag_PairsTheDataPointWithItsDeclaration()
    {
        var schema = new TagDefinitions(
            new Dictionary<TagName, TagDefinition>(TagName.CaseInsensitiveComparer)
            {
                [new TagName("Motor.Speed")] = Dint("Motor.Speed"),
            });

        var resolved = schema.Resolve(CreateDInt("Motor.Speed"));

        resolved.TagDefinition.Should().NotBeNull();
        resolved.TagDefinition!.Value.DataType.Should().Be(AllenBradleyDataType.Dint);
    }

    [Fact]
    public void Resolve_AnAbsentTag_LeavesTheDeviceSideNull()
    {
        var schema = new TagDefinitions(new Dictionary<TagName, TagDefinition>());

        schema.Resolve(CreateDInt("Ghost")).TagDefinition.Should().BeNull();
    }
}
