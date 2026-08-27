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
    private static LogixTypeDeclaration Dint(string name) =>
        new(new TagName(name), LogixTypeKind.Atomic, CipType.Dint, new DimensionCount(0), new ElementCount(1), new ElementLength(4));

    [Fact]
    public void Lookup_IsCaseInsensitive()
    {
        var schema = new TagDefinitions(
            new Dictionary<TagName, LogixTypeDeclaration>(TagName.CaseInsensitiveComparer)
            {
                [new TagName("Motor.Speed")] = Dint("Motor.Speed"),
            });

        schema.Lookup(new TagName("motor.speed")).Should().NotBeNull();
    }

    [Fact]
    public void Lookup_AnAbsentTag_ReturnsNull()
    {
        var schema = new TagDefinitions(new Dictionary<TagName, LogixTypeDeclaration>());

        schema.Lookup(new TagName("Nope")).Should().BeNull();
    }

    [Fact]
    public void Resolve_APresentTag_PairsTheDataPointWithItsDeclaration()
    {
        var schema = new TagDefinitions(
            new Dictionary<TagName, LogixTypeDeclaration>(TagName.CaseInsensitiveComparer)
            {
                [new TagName("Motor.Speed")] = Dint("Motor.Speed"),
            });

        var resolved = schema.Resolve(new DIntDataPoint(new TagName("Motor.Speed")));

        resolved.Device.Should().NotBeNull();
        resolved.Device!.Value.AtomicType.Should().Be(CipType.Dint);
    }

    [Fact]
    public void Resolve_AnAbsentTag_LeavesTheDeviceSideNull()
    {
        var schema = new TagDefinitions(new Dictionary<TagName, LogixTypeDeclaration>());

        schema.Resolve(new DIntDataPoint(new TagName("Ghost"))).Device.Should().BeNull();
    }
}
