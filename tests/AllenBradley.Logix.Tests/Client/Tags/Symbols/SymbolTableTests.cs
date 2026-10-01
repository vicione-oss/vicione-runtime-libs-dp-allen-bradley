using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Symbols;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Symbols.SymbolTypes;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Symbols.TagsListing;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Symbols.Templates;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.TagDefinitionTestDataFactory;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.DeclaredTypeTestDataFactory;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.TemplateDefinitionTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client.Tags.Symbols;

/// <summary>
/// The walk from a path to what the controller declares at its end: a tag, a member reached through the
/// templates, or one element of an array. What comes out carries the address the path renders to.
/// </summary>
public sealed class SymbolTableTests
{
    private static readonly TemplateId RampTemplateId = new(0x456);
    private static readonly TemplateId StringTemplateId = new(0xFCE);
    private static readonly TemplateId TimerTemplateId = new(0xF83);
    private static readonly TemplateId CounterTemplateId = new(0xF82);

    private static readonly TagDefinition StringTagDefinition =
        DefaultStructureTagDefinition() with { TemplateId = StringTemplateId };

    private static readonly TemplateDefinition StringTemplate = Template(StringTemplateId, "STRING",
        AtomicMember("LEN", AllenBradleyDataType.Dint),
        ArrayMemberOf(AtomicMember("DATA", AllenBradleyDataType.Sint), 82));

    private static readonly TagDefinition TimerTagDefinition =
        DefaultStructureTagDefinition() with { TemplateId = TimerTemplateId };

    private static readonly TemplateDefinition TimerTemplate = Template(TimerTemplateId, "TIMER",
        AtomicMember("PRE", AllenBradleyDataType.Dint),
        AtomicMember("ACC", AllenBradleyDataType.Dint),
        AtomicMember("DN", AllenBradleyDataType.Bool));

    private static readonly TagDefinition CounterTagDefinition =
        DefaultStructureTagDefinition() with { TemplateId = CounterTemplateId };

    private static readonly TemplateDefinition CounterTemplate = Template(CounterTemplateId, "COUNTER",
        AtomicMember("PRE", AllenBradleyDataType.Dint),
        AtomicMember("ACC", AllenBradleyDataType.Dint),
        AtomicMember("DN", AllenBradleyDataType.Bool));

    [Fact]
    public void ATagIsLookedUpWithoutRegardToCase()
    {
        // Arrange
        var speedTag = new ListedTag(new TagAddress("Speed"), DefaultAtomicTagDefinition());
        var symbolTable = new SymbolTable([speedTag], []);
        var path = TagPath.Parse("SPEED");

        // Act
        var found = symbolTable.GetDeclaredTypeAtPath(path);

        // Assert
        found.Should().Be(DefaultAtomicDeclaredType() with { TagAddress = new TagAddress("SPEED") });
    }

    [Fact]
    public void ATagAbsentFromTheDefinitionsIsNotFound()
    {
        // Arrange
        var speedTag = new ListedTag(new TagAddress("Speed"), DefaultAtomicTagDefinition());
        var symbolTable = new SymbolTable([speedTag], []);
        var path = TagPath.Parse("Nope");

        // Act
        var found = symbolTable.GetDeclaredTypeAtPath(path);

        // Assert
        found.Should().BeNull();
    }

    [Fact]
    public void AnArrayTagIsDeclaredWithItsElementCount()
    {
        // Arrange
        var arrayTag = new ListedTag(new TagAddress("IntValues"), DefaultIntArrayTagDefinition());
        var symbolTable = new SymbolTable([arrayTag], []);
        var path = TagPath.Parse("IntValues");

        // Act
        var found = symbolTable.GetDeclaredTypeAtPath(path);

        // Assert
        found.Should().Be(DefaultIntArrayDeclaredType() with { TagAddress = new TagAddress("IntValues") });
    }

    [Fact]
    public void AnElementIsDeclaredAScalarOfItsArraysTypeAtItsOwnAddress()
    {
        // Arrange
        var arrayTag = new ListedTag(new TagAddress("IntValues"), DefaultIntArrayTagDefinition());
        var symbolTable = new SymbolTable([arrayTag], []);
        var path = TagPath.Parse("IntValues[3]");

        // Act
        var found = symbolTable.GetDeclaredTypeAtPath(path);

        // Assert
        found.Should().Be(IntScalarAt("IntValues[3]"));
    }

    [Fact]
    public void AnElementPastTheDeclaredCountIsNotFound()
    {
        // Arrange
        var arrayTag = new ListedTag(new TagAddress("IntValues"), DefaultIntArrayTagDefinition());
        var symbolTable = new SymbolTable([arrayTag], []);
        var path = TagPath.Parse("IntValues[10]");

        // Act
        var found = symbolTable.GetDeclaredTypeAtPath(path);

        // Assert
        found.Should().BeNull();
    }

    [Fact]
    public void AnElementOfATagDeclaredScalarIsNotFound()
    {
        // Arrange
        var speedTag = new ListedTag(new TagAddress("Speed"), DefaultAtomicTagDefinition());
        var symbolTable = new SymbolTable([speedTag], []);
        var path = TagPath.Parse("Speed[0]");

        // Act
        var found = symbolTable.GetDeclaredTypeAtPath(path);

        // Assert
        found.Should().BeNull();
    }

    [Fact]
    public void AMemberIsDeclaredAsItsTemplateDescribesItAtItsOwnAddress()
    {
        // Arrange
        var motorTag = new ListedTag(new TagAddress("Motor"), DefaultStructureTagDefinition());
        var motorTemplate = MotorTemplateWith(AtomicMember("Speed", AllenBradleyDataType.Dint));
        var symbolTable = new SymbolTable([motorTag], [motorTemplate]);
        var path = TagPath.Parse("Motor.Speed");

        // Act
        var found = symbolTable.GetDeclaredTypeAtPath(path);

        // Assert
        found.Should().Be(DefaultAtomicDeclaredType() with { TagAddress = new TagAddress("Motor.Speed") });
    }

    [Fact]
    public void AMemberIsLookedUpWithoutRegardToCase()
    {
        // Arrange
        var motorTag = new ListedTag(new TagAddress("Motor"), DefaultStructureTagDefinition());
        var motorTemplate = MotorTemplateWith(AtomicMember("Speed", AllenBradleyDataType.Dint));
        var symbolTable = new SymbolTable([motorTag], [motorTemplate]);
        var path = TagPath.Parse("MOTOR.speed");

        // Act
        var found = symbolTable.GetDeclaredTypeAtPath(path);

        // Assert
        found.Should().Be(DefaultAtomicDeclaredType() with { TagAddress = new TagAddress("MOTOR.speed") });
    }

    [Fact]
    public void AMemberOfANestedStructureIsReachedThroughEachTemplateOnTheWay()
    {
        // Arrange
        var motorTag = new ListedTag(new TagAddress("Motor"), DefaultStructureTagDefinition());
        var motorTemplate = MotorTemplateWith(StructureMember("Ramp", RampTemplateId));
        var rampTemplate = Template(RampTemplateId, "RampType", AtomicMember("Target", AllenBradleyDataType.Real));
        var symbolTable = new SymbolTable([motorTag], [motorTemplate, rampTemplate]);
        var path = TagPath.Parse("Motor.Ramp.Target");

        // Act
        var found = symbolTable.GetDeclaredTypeAtPath(path);

        // Assert
        var expected = DefaultAtomicDeclaredType() with
        {
            TagAddress = new TagAddress("Motor.Ramp.Target"),
            DataType = AllenBradleyDataType.Real,
        };
        found.Should().Be(expected);
    }

    [Fact]
    public void AnArrayMemberIsDeclaredWithItsElementCount()
    {
        // Arrange
        var motorTag = new ListedTag(new TagAddress("Motor"), DefaultStructureTagDefinition());
        var motorTemplate = MotorTemplateWith(ArrayMemberOf(AtomicMember("Values", AllenBradleyDataType.Int), 10));
        var symbolTable = new SymbolTable([motorTag], [motorTemplate]);
        var path = TagPath.Parse("Motor.Values");

        // Act
        var found = symbolTable.GetDeclaredTypeAtPath(path);

        // Assert
        found.Should().Be(DefaultIntArrayDeclaredType() with { TagAddress = new TagAddress("Motor.Values") });
    }

    [Fact]
    public void AnElementOfAnArrayMemberIsDeclaredAScalarOfThatMembersType()
    {
        // Arrange
        var motorTag = new ListedTag(new TagAddress("Motor"), DefaultStructureTagDefinition());
        var motorTemplate = MotorTemplateWith(ArrayMemberOf(AtomicMember("Values", AllenBradleyDataType.Int), 10));
        var symbolTable = new SymbolTable([motorTag], [motorTemplate]);
        var path = TagPath.Parse("Motor.Values[3]");

        // Act
        var found = symbolTable.GetDeclaredTypeAtPath(path);

        // Assert
        found.Should().Be(IntScalarAt("Motor.Values[3]"));
    }

    [Fact]
    public void AStringTagIsDeclaredAStringOfItsTemplatesCapacity()
    {
        // Arrange
        var labelTag = new ListedTag(new TagAddress("Label"), StringTagDefinition);
        var symbolTable = new SymbolTable([labelTag], [StringTemplate]);
        var path = TagPath.Parse("Label");

        // Act
        var found = symbolTable.GetDeclaredTypeAtPath(path);

        // Assert
        found.Should().Be(DefaultStringDeclaredType() with { TagAddress = new TagAddress("Label") });
    }

    [Fact]
    public void AStringMemberIsDeclaredAStringOfItsTemplatesCapacity()
    {
        // Arrange
        var motorTag = new ListedTag(new TagAddress("Motor"), DefaultStructureTagDefinition());
        var motorTemplate = MotorTemplateWith(StructureMember("Name", StringTemplateId));
        var symbolTable = new SymbolTable([motorTag], [motorTemplate, StringTemplate]);
        var path = TagPath.Parse("Motor.Name");

        // Act
        var found = symbolTable.GetDeclaredTypeAtPath(path);

        // Assert
        found.Should().Be(DefaultStringDeclaredType() with { TagAddress = new TagAddress("Motor.Name") });
    }

    [Fact]
    public void AStructureTagThatIsNoStringTimerOrCounterIsDeclaredAStructure()
    {
        // Arrange
        var motorTag = new ListedTag(new TagAddress("Motor"), DefaultStructureTagDefinition());
        var motorTemplate = MotorTemplateWith(AtomicMember("Speed", AllenBradleyDataType.Dint));
        var symbolTable = new SymbolTable([motorTag], [motorTemplate]);
        var path = TagPath.Parse("Motor");

        // Act
        var found = symbolTable.GetDeclaredTypeAtPath(path);

        // Assert
        found.Should().Be(StructureAt("Motor"));
    }

    [Fact]
    public void ATimerTagIsDeclaredATimer()
    {
        // Arrange
        var delayTag = new ListedTag(new TagAddress("Delay"), TimerTagDefinition);
        var symbolTable = new SymbolTable([delayTag], [TimerTemplate]);
        var path = TagPath.Parse("Delay");

        // Act
        var found = symbolTable.GetDeclaredTypeAtPath(path);

        // Assert
        found.Should().Be(TimerAt("Delay"));
    }

    [Fact]
    public void ATimerMemberIsDeclaredATimer()
    {
        // Arrange
        var motorTag = new ListedTag(new TagAddress("Motor"), DefaultStructureTagDefinition());
        var motorTemplate = MotorTemplateWith(StructureMember("StartDelay", TimerTemplateId));
        var symbolTable = new SymbolTable([motorTag], [motorTemplate, TimerTemplate]);
        var path = TagPath.Parse("Motor.StartDelay");

        // Act
        var found = symbolTable.GetDeclaredTypeAtPath(path);

        // Assert
        found.Should().Be(TimerAt("Motor.StartDelay"));
    }

    [Fact]
    public void ACounterTagIsDeclaredACounter()
    {
        // Arrange
        var partsTag = new ListedTag(new TagAddress("Parts"), CounterTagDefinition);
        var symbolTable = new SymbolTable([partsTag], [CounterTemplate]);
        var path = TagPath.Parse("Parts");

        // Act
        var found = symbolTable.GetDeclaredTypeAtPath(path);

        // Assert
        found.Should().Be(CounterAt("Parts"));
    }

    [Fact]
    public void ACounterMemberIsDeclaredACounter()
    {
        // Arrange
        var motorTag = new ListedTag(new TagAddress("Motor"), DefaultStructureTagDefinition());
        var motorTemplate = MotorTemplateWith(StructureMember("Starts", CounterTemplateId));
        var symbolTable = new SymbolTable([motorTag], [motorTemplate, CounterTemplate]);
        var path = TagPath.Parse("Motor.Starts");

        // Act
        var found = symbolTable.GetDeclaredTypeAtPath(path);

        // Assert
        found.Should().Be(CounterAt("Motor.Starts"));
    }

    [Fact]
    public void AMemberOfATimerIsDeclaredAsTheTimerTemplateDescribesIt()
    {
        // Arrange
        var delayTag = new ListedTag(new TagAddress("Delay"), TimerTagDefinition);
        var symbolTable = new SymbolTable([delayTag], [TimerTemplate]);
        var path = TagPath.Parse("Delay.PRE");

        // Act
        var found = symbolTable.GetDeclaredTypeAtPath(path);

        // Assert
        found.Should().Be(DefaultAtomicDeclaredType() with { TagAddress = new TagAddress("Delay.PRE") });
    }

    [Fact]
    public void AnArrayOfStringsIsDeclaredAStringArrayOfItsTemplatesCapacity()
    {
        // Arrange
        var labelsTag = new ListedTag(
            new TagAddress("Labels"),
            StringTagDefinition with
            {
                DimensionCount = DimensionCount.OneDimensional,
                ElementCount = new ElementCount(5),
            });
        var symbolTable = new SymbolTable([labelsTag], [StringTemplate]);
        var path = TagPath.Parse("Labels");

        // Act
        var found = symbolTable.GetDeclaredTypeAtPath(path);

        // Assert
        var expected = DefaultStringDeclaredType() with
        {
            TagAddress = new TagAddress("Labels"),
            DimensionCount = DimensionCount.OneDimensional,
            ElementCount = new ElementCount(5),
        };
        found.Should().Be(expected);
    }

    [Fact]
    public void AStructureTagWhoseTemplateTheControllerDidNotServeIsDeclaredAStructure()
    {
        // Arrange
        var labelTag = new ListedTag(new TagAddress("Label"), StringTagDefinition);
        var symbolTable = new SymbolTable([labelTag], []);
        var path = TagPath.Parse("Label");

        // Act
        var found = symbolTable.GetDeclaredTypeAtPath(path);

        // Assert
        found.Should().Be(StructureAt("Label"));
    }

    [Fact]
    public void AMemberTheTemplateDoesNotHaveIsNotFound()
    {
        // Arrange
        var motorTag = new ListedTag(new TagAddress("Motor"), DefaultStructureTagDefinition());
        var motorTemplate = MotorTemplateWith(AtomicMember("Speed", AllenBradleyDataType.Dint));
        var symbolTable = new SymbolTable([motorTag], [motorTemplate]);
        var path = TagPath.Parse("Motor.Torque");

        // Act
        var found = symbolTable.GetDeclaredTypeAtPath(path);

        // Assert
        found.Should().BeNull();
    }

    [Fact]
    public void AMemberOfAnAtomicTagIsNotFound()
    {
        // Arrange
        var speedTag = new ListedTag(new TagAddress("Speed"), DefaultAtomicTagDefinition());
        var symbolTable = new SymbolTable([speedTag], []);
        var path = TagPath.Parse("Speed.Value");

        // Act
        var found = symbolTable.GetDeclaredTypeAtPath(path);

        // Assert
        found.Should().BeNull();
    }

    [Fact]
    public void AMemberBehindAnAtomicMemberIsNotFound()
    {
        // Arrange
        var motorTag = new ListedTag(new TagAddress("Motor"), DefaultStructureTagDefinition());
        var motorTemplate = MotorTemplateWith(AtomicMember("Speed", AllenBradleyDataType.Dint));
        var symbolTable = new SymbolTable([motorTag], [motorTemplate]);
        var path = TagPath.Parse("Motor.Speed.Value");

        // Act
        var found = symbolTable.GetDeclaredTypeAtPath(path);

        // Assert
        found.Should().BeNull();
    }

    [Fact]
    public void AMemberBehindAnArrayMemberIsNotFound()
    {
        // Arrange
        var motorTag = new ListedTag(new TagAddress("Motor"), DefaultStructureTagDefinition());
        var motorTemplate = MotorTemplateWith(ArrayMemberOf(StructureMember("History", RampTemplateId), 4));
        var symbolTable = new SymbolTable([motorTag], [motorTemplate]);
        var path = TagPath.Parse("Motor.History.Target");

        // Act
        var found = symbolTable.GetDeclaredTypeAtPath(path);

        // Assert
        found.Should().BeNull();
    }

    [Fact]
    public void AMemberOfAStructureWhoseTemplateTheControllerDidNotServeIsNotFound()
    {
        // Arrange
        var motorTag = new ListedTag(new TagAddress("Motor"), DefaultStructureTagDefinition());
        var symbolTable = new SymbolTable([motorTag], []);
        var path = TagPath.Parse("Motor.Speed");

        // Act
        var found = symbolTable.GetDeclaredTypeAtPath(path);

        // Assert
        found.Should().BeNull();
    }

    private static TemplateDefinition MotorTemplateWith(params TemplateMember[] members) =>
        Template(DefaultTemplateId, "MotorType", members);

    private static DeclaredType IntScalarAt(string address) =>
        DefaultAtomicDeclaredType() with { TagAddress = new TagAddress(address), DataType = AllenBradleyDataType.Int };

    private static DeclaredType StructureAt(string address) =>
        new(new TagAddress(address), AllenBradleyDataType.Structure, MaxLength: null, Scalar, OneElement);

    private static DeclaredType TimerAt(string address) =>
        new(new TagAddress(address), AllenBradleyDataType.Timer, MaxLength: null, Scalar, OneElement);

    private static DeclaredType CounterAt(string address) =>
        new(new TagAddress(address), AllenBradleyDataType.Counter, MaxLength: null, Scalar, OneElement);
}
