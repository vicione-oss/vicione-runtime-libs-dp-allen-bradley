using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Symbols.SymbolTypes;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Symbols.Templates;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.TemplateDefinitionTestDataFactory;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.TemplateTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client.Tags.Symbols.Templates;

public sealed class TemplateDecoderTests
{
    private const ushort LineTemplateId = 0x123;

    [Fact]
    public void TheStringTemplateDecodesToItsNameSizeHandleAndMembers()
    {
        // Arrange
        var stringTemplate = StringTemplate();
        var template = Template(stringTemplate);

        // Act
        var decoded = TemplateDecoder.Decode(template);

        // Assert
        var expected = new TemplateDefinition(
            new TemplateId(StringTemplateId),
            new TemplateName("STRING"),
            new StructureHandle(stringTemplate.Handle),
            new StructureSize(88),
            [
                AtomicMember("LEN", AllenBradleyDataType.Dint),
                ArrayMemberOf(AtomicMember("DATA", AllenBradleyDataType.Sint), 82) with { Offset = new MemberOffset(4) },
            ]);
        decoded.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void ATemplateNameIsCutAtTheEncodingSuffixTheControllerAppends()
    {
        // Arrange
        var template = Template(new TemplateEntry(LineTemplateId, "MotorState;n", []));

        // Act
        var decoded = TemplateDecoder.Decode(template);

        // Assert
        decoded.Name.Should().Be(new TemplateName("MotorState"));
    }

    [Fact]
    public void AStructureMemberNamesItsOwnTemplateAndNoAtomicType()
    {
        // Arrange
        var template = Template(new TemplateEntry(LineTemplateId, "Line",
            [new MemberEntry("Ramp", StructureMemberType) { Offset = 8 }]));

        // Act
        var decoded = TemplateDecoder.Decode(template);

        // Assert
        var expected = StructureMember("Ramp", new TemplateId(0x456)) with { Offset = new MemberOffset(8) };
        decoded.Members.Should().ContainSingle().Which.Should().Be(expected);
    }

    [Fact]
    public void AnAtomicMemberNamesNoTemplate()
    {
        // Arrange
        var template = Template(new TemplateEntry(LineTemplateId, "Line", [new MemberEntry("Speed", DintMemberType)]));

        // Act
        var decoded = TemplateDecoder.Decode(template);

        // Assert
        decoded.Members.Should().ContainSingle().Which.TagDefinition.TemplateId.Should().BeNull();
    }

    [Fact]
    public void ABoolMemberCarriesTheBitOfItsHostByteItIs()
    {
        // Arrange
        var template = Template(new TemplateEntry(LineTemplateId, "Line",
            [new MemberEntry("Running", BoolMemberType) { Info = 3, Offset = 12 }]));

        // Act
        var decoded = TemplateDecoder.Decode(template);

        // Assert
        var expected = AtomicMember("Running", AllenBradleyDataType.Bool) with
        {
            Offset = new MemberOffset(12),
            BitPosition = new BitPosition(3),
        };
        decoded.Members.Should().ContainSingle().Which.Should().Be(expected);
    }

    [Fact]
    public void AMemberThatIsNotABoolCarriesNoBitPosition()
    {
        // Arrange
        // A non-zero info word on a DINT is whatever the controller left there, not a bit.
        var template = Template(new TemplateEntry(LineTemplateId, "Line",
            [new MemberEntry("Speed", DintMemberType) { Info = 3 }]));

        // Act
        var decoded = TemplateDecoder.Decode(template);

        // Assert
        decoded.Members.Should().ContainSingle().Which.BitPosition.Should().BeNull();
    }

    [Fact]
    public void AnArrayMemberCarriesItsRankAndDeclaredElementCount()
    {
        // Arrange
        var template = Template(new TemplateEntry(LineTemplateId, "Line",
            [new MemberEntry("Readings", ArrayMemberType | DintMemberType) { Info = 10 }]));

        // Act
        var decoded = TemplateDecoder.Decode(template);

        // Assert
        var expected = ArrayMemberOf(AtomicMember("Readings", AllenBradleyDataType.Dint), 10);
        decoded.Members.Should().ContainSingle().Which.Should().Be(expected);
    }

    [Fact]
    public void ABoolArrayMemberCountsTheBitsItsWordsHold()
    {
        // Arrange
        var template = Template(new TemplateEntry(LineTemplateId, "Line",
            [new MemberEntry("Flags", ArrayMemberType | DwordMemberType) { Info = 2 }]));

        // Act
        var decoded = TemplateDecoder.Decode(template);

        // Assert
        var expected = ArrayMemberOf(AtomicMember("Flags", AllenBradleyDataType.Bool), 64);
        decoded.Members.Should().ContainSingle().Which.Should().Be(expected);
    }

    [Fact]
    public void MembersKeepTheOrderTheTemplateListsThem()
    {
        // Arrange
        var template = Template(new TemplateEntry(LineTemplateId, "Line",
        [
            new MemberEntry("Third", DintMemberType) { Offset = 8 },
            new MemberEntry("First", DintMemberType) { Offset = 0 },
            new MemberEntry("Second", DintMemberType) { Offset = 4 },
        ]));

        // Act
        var decoded = TemplateDecoder.Decode(template);

        // Assert
        decoded.Members.Select(member => member.Name.Value).Should().Equal("Third", "First", "Second");
    }

    [Fact]
    public void ATemplateShorterThanItsHeaderIsRefused()
    {
        // Arrange
        var template = Template(StringTemplate())[..10];

        // Act
        var decoding = () => TemplateDecoder.Decode(template);

        // Assert
        decoding.Should().Throw<FormatException>();
    }

    [Fact]
    public void ATemplateEndingBeforeItsMemberDescriptorsIsRefused()
    {
        // Arrange
        var template = Template(StringTemplate())[..20];

        // Act
        var decoding = () => TemplateDecoder.Decode(template);

        // Assert
        decoding.Should().Throw<FormatException>();
    }

    [Fact]
    public void ATemplateWhoseLastNameIsNotTerminatedIsRefused()
    {
        // Arrange
        var template = Template(StringTemplate())[..^1];

        // Act
        var decoding = () => TemplateDecoder.Decode(template);

        // Assert
        decoding.Should().Throw<FormatException>();
    }
}
