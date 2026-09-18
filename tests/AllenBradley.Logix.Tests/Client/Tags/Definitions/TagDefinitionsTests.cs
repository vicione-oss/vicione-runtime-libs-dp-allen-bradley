using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Definitions;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration.Templates;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.TagDefinitionTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client.Tags.Definitions;

public sealed class TagDefinitionsTests
{
    private static readonly TemplateId RampTemplateId = new(0x456);
    private static readonly TemplateId StringTemplateId = new(0xFCE);

    private static readonly TagDefinition Speed =
        DefaultAtomicTagDefinition() with { TagAddress = new TagAddress("Speed") };

    private static readonly TagDefinition Readings =
        DefaultIntArrayTagDefinition() with { TagAddress = new TagAddress("Readings") };

    private static readonly TagDefinition Motor =
        DefaultStringTagDefinition() with { TagAddress = new TagAddress("Motor") };

    private static readonly TemplateDefinition MotorTemplate = Template(DefaultTemplateId, "MotorType",
        Atomic("Speed", AllenBradleyDataType.Dint),
        Structure("Ramp", RampTemplateId),
        ArrayOf(Atomic("Readings", AllenBradleyDataType.Int), 10),
        Structure("Name", StringTemplateId),
        ArrayOf(Structure("History", RampTemplateId), 4));

    private static readonly TemplateDefinition RampTemplate = Template(RampTemplateId, "RampType",
        Atomic("Target", AllenBradleyDataType.Real));

    private static readonly TemplateDefinition StringTemplate = Template(StringTemplateId, "STRING",
        Atomic("LEN", AllenBradleyDataType.Dint),
        ArrayOf(Atomic("DATA", AllenBradleyDataType.Sint), 82));

    private static readonly TagDefinition SpeedOfMotor = new(
        new TagAddress("Motor.Speed"), AllenBradleyDataType.Dint, TemplateId: null, MaxLength: null,
        DimensionCount.Scalar, ElementCount.Scalar);

    private static readonly TagDefinition ReadingsOfMotor = new(
        new TagAddress("Motor.Readings"), AllenBradleyDataType.Int, TemplateId: null, MaxLength: null,
        DimensionCount.OneDimensional, new ElementCount(10));

    private readonly TagDefinitions _definitions = new(
        new[] { Speed, Readings, Motor }.ToDictionary(tag => tag.TagAddress, TagAddress.CaseInsensitiveComparer),
        new[] { MotorTemplate, RampTemplate, StringTemplate }.ToDictionary(template => template.Id));

    [Fact]
    public void ATagIsLookedUpWithoutRegardToCase()
    {
        // Arrange

        // Act
        var found = _definitions.Lookup(TagPath.Parse("SPEED"));

        // Assert
        found.Should().Be(Speed);
    }

    [Fact]
    public void ATagAbsentFromTheDefinitionsIsNotFound()
    {
        // Arrange

        // Act
        var found = _definitions.Lookup(TagPath.Parse("Nope"));

        // Assert
        found.Should().BeNull();
    }

    [Fact]
    public void AnElementIsDeclaredAsItsArray()
    {
        // Arrange

        // Act
        var found = _definitions.Lookup(TagPath.Parse("Readings[3]"));

        // Assert
        found.Should().Be(Readings);
    }

    [Fact]
    public void AMemberIsDeclaredAsItsTemplateDescribesIt()
    {
        // Arrange

        // Act
        var found = _definitions.Lookup(TagPath.Parse("Motor.Speed"));

        // Assert
        found.Should().Be(SpeedOfMotor);
    }

    [Fact]
    public void AMemberIsLookedUpWithoutRegardToCaseAndAddressedAsTheControllerSpellsIt()
    {
        // Arrange

        // Act
        var found = _definitions.Lookup(TagPath.Parse("MOTOR.speed"));

        // Assert
        found.Should().Be(SpeedOfMotor);
    }

    [Fact]
    public void AMemberOfANestedStructureIsReachedThroughEachTemplateOnTheWay()
    {
        // Arrange

        // Act
        var found = _definitions.Lookup(TagPath.Parse("Motor.Ramp.Target"));

        // Assert
        var expected = new TagDefinition(
            new TagAddress("Motor.Ramp.Target"), AllenBradleyDataType.Real, TemplateId: null, MaxLength: null,
            DimensionCount.Scalar, ElementCount.Scalar);
        found.Should().Be(expected);
    }

    [Fact]
    public void AnArrayMemberIsDeclaredWithItsElementCount()
    {
        // Arrange

        // Act
        var found = _definitions.Lookup(TagPath.Parse("Motor.Readings"));

        // Assert
        found.Should().Be(ReadingsOfMotor);
    }

    [Fact]
    public void AnElementOfAnArrayMemberIsDeclaredAsThatMember()
    {
        // Arrange

        // Act
        var found = _definitions.Lookup(TagPath.Parse("Motor.Readings[3]"));

        // Assert
        found.Should().Be(ReadingsOfMotor);
    }

    [Fact]
    public void AStructureMemberIsDeclaredAsAStringOfItsTemplatesCapacity()
    {
        // Arrange

        // Act
        var found = _definitions.Lookup(TagPath.Parse("Motor.Name"));

        // Assert
        var expected = new TagDefinition(
            new TagAddress("Motor.Name"), AllenBradleyDataType.String, StringTemplateId, new StringMaxLength(82),
            DimensionCount.Scalar, ElementCount.Scalar);
        found.Should().Be(expected);
    }

    [Fact]
    public void AStructureMemberThatIsNoStringHasNoCapacity()
    {
        // Arrange

        // Act
        var found = _definitions.Lookup(TagPath.Parse("Motor.Ramp"));

        // Assert
        var expected = new TagDefinition(
            new TagAddress("Motor.Ramp"), AllenBradleyDataType.String, RampTemplateId, MaxLength: null,
            DimensionCount.Scalar, ElementCount.Scalar);
        found.Should().Be(expected);
    }

    [Fact]
    public void AMemberTheTemplateDoesNotHaveIsNotFound()
    {
        // Arrange

        // Act
        var found = _definitions.Lookup(TagPath.Parse("Motor.Torque"));

        // Assert
        found.Should().BeNull();
    }

    [Fact]
    public void AMemberOfAnAtomicTagIsNotFound()
    {
        // Arrange

        // Act
        var found = _definitions.Lookup(TagPath.Parse("Speed.Value"));

        // Assert
        found.Should().BeNull();
    }

    [Fact]
    public void AMemberBehindAnAtomicMemberIsNotFound()
    {
        // Arrange

        // Act
        var found = _definitions.Lookup(TagPath.Parse("Motor.Speed.Value"));

        // Assert
        found.Should().BeNull();
    }

    [Fact]
    public void AMemberBehindAnArrayMemberIsNotFound()
    {
        // Arrange

        // Act
        var found = _definitions.Lookup(TagPath.Parse("Motor.History.Target"));

        // Assert
        found.Should().BeNull();
    }

    [Fact]
    public void AMemberOfAStructureWhoseTemplateTheControllerDidNotServeIsNotFound()
    {
        // Arrange
        var definitions = new TagDefinitions(
            new[] { Motor }.ToDictionary(tag => tag.TagAddress, TagAddress.CaseInsensitiveComparer),
            new Dictionary<TemplateId, TemplateDefinition>());

        // Act
        var found = definitions.Lookup(TagPath.Parse("Motor.Speed"));

        // Assert
        found.Should().BeNull();
    }

    private static TemplateDefinition Template(TemplateId id, string name, params TemplateMember[] members) =>
        new(id, new TemplateName(name), new StructureHandle(0xABCD), new StructureSize(4), members);

    private static TemplateMember Atomic(string name, AllenBradleyDataType dataType) =>
        new(new UdtMemberName(name), new MemberOffset(0), dataType, TemplateId: null,
            DimensionCount.Scalar, ElementCount.Scalar, BitPosition: null);

    private static TemplateMember Structure(string name, TemplateId templateId) =>
        Atomic(name, AllenBradleyDataType.Unknown) with { TemplateId = templateId };

    private static TemplateMember ArrayOf(TemplateMember element, uint elementCount) =>
        element with { DimensionCount = DimensionCount.OneDimensional, ElementCount = new ElementCount(elementCount) };
}
