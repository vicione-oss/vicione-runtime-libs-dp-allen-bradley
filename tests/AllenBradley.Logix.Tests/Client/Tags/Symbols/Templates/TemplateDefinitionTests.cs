using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Symbols.SymbolTypes;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Symbols.Templates;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.TemplateDefinitionTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client.Tags.Symbols.Templates;

public sealed class TemplateDefinitionTests
{
    private static readonly TemplateMember Len = AtomicMember("LEN", AllenBradleyDataType.Dint);

    private static readonly TemplateMember Data =
        ArrayMemberOf(AtomicMember("DATA", AllenBradleyDataType.Sint), 82) with { Offset = new MemberOffset(4) };

    private static readonly TemplateDefinition StringTemplate = Template(new TemplateId(0xFCE), "STRING", Len, Data);

    [Fact]
    public void AMemberIsFoundWithoutRegardToCase()
    {
        // Arrange
        var lowerCaseName = new UdtMemberName("len");

        // Act
        var found = StringTemplate.FindMember(lowerCaseName);

        // Assert
        found.Should().Be(Len);
    }

    [Fact]
    public void AMemberTheTemplateDoesNotHaveIsNotFound()
    {
        // Arrange
        var unknownName = new UdtMemberName("SIZE");

        // Act
        var found = StringTemplate.FindMember(unknownName);

        // Assert
        found.Should().BeNull();
    }

    [Fact]
    public void AStringTemplateHoldsAsManyCharactersAsItsDataMemberHasElements()
    {
        // Arrange

        // Act
        var capacity = StringTemplate.StringCapacity;

        // Assert
        capacity.Should().Be(new StringMaxLength((int)Data.TagDefinition.ElementCount.Value));
    }

    [Fact]
    public void ATemplateWithoutASintArrayNamedDataHoldsNoCharacters()
    {
        // Arrange
        var dintData = Data with { TagDefinition = Data.TagDefinition with { DataType = AllenBradleyDataType.Dint } };
        var template = StringTemplate with { Members = [Len, dintData] };

        // Act
        var capacity = template.StringCapacity;

        // Assert
        capacity.Should().BeNull();
    }
}
