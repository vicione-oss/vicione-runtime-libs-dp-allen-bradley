using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Symbols.SymbolTypes;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Symbols.Templates;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.TemplateTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client.Tags.Symbols.Templates;

/// <summary>
/// Both structs reinterpret libplctag's bytes, so nothing checks their layout at compile time and a
/// reordered or repacked field would fail silently.
/// </summary>
public sealed class TemplateHeaderTests
{
    // Distinct values, so a swapped pair of fields shows up rather than passing by coincidence.
    private static readonly TemplateEntry Distinct = new(0x0A0B, "Line", [new MemberEntry("Speed", 0x1C1D) { Info = 0x1A1B, Offset = 0x2A2B2C2D }])
    {
        MemberDescriptionSize = 0x0102_0304,
        StructureSize = 0x0506_0708,
        Handle = 0x0C0D,
    };

    [Fact]
    public void TheHeaderIsTheSizeThePackedLayoutGivesIt()
    {
        // Arrange

        // Act
        var size = TemplateHeader.Size;

        // Assert
        // Without Pack = 1 the 4-byte fields are aligned and this reports 16.
        size.Should().Be(14);
    }

    [Fact]
    public void TheMemberDescriptorIsTheSizeThePackedLayoutGivesIt()
    {
        // Arrange

        // Act
        var size = TemplateMemberDescriptor.Size;

        // Assert
        size.Should().Be(8);
    }

    [Fact]
    public void EveryFieldOfTheHeaderIsReadFromItsOwnOffset()
    {
        // Arrange
        var template = Template(Distinct);

        // Act
        var header = TemplateHeader.ReadFrom(template);

        // Assert
        // Field by field because the struct offers no constructor to build a whole expectation from.
        header.TemplateId.Should().Be(Distinct.Id);
        header.MemberDescriptionSize.Should().Be(Distinct.MemberDescriptionSize);
        header.StructureSize.Should().Be(Distinct.StructureSize);
        header.MemberCount.Should().Be((ushort)Distinct.Members.Count);
        header.StructureHandle.Should().Be(Distinct.Handle);
    }

    [Fact]
    public void EveryFieldOfAMemberDescriptorIsReadFromItsOwnOffset()
    {
        // Arrange
        var firstDescriptor = Template(Distinct).AsSpan(TemplateHeader.Size);

        // Act
        var descriptor = TemplateMemberDescriptor.ReadFrom(firstDescriptor);

        // Assert
        var member = Distinct.Members[0];
        descriptor.Info.Should().Be(member.Info);
        descriptor.SymbolType.Should().Be(new SymbolType(member.SymbolType));
        descriptor.Offset.Should().Be(member.Offset);
    }
}
