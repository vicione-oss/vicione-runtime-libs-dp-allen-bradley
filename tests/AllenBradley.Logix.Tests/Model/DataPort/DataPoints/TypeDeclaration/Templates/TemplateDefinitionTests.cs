using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration.Templates;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.DataPort.DataPoints.TypeDeclaration.Templates;

public sealed class TemplateDefinitionTests
{
    private static readonly TemplateMember Len = new(
        new UdtMemberName("LEN"), new MemberOffset(0), AllenBradleyDataType.Dint, TemplateId: null,
        DimensionCount.Scalar, ElementCount.Scalar, BitPosition: null);

    private static readonly TemplateMember Data = new(
        new UdtMemberName("DATA"), new MemberOffset(4), AllenBradleyDataType.Sint, TemplateId: null,
        DimensionCount.OneDimensional, new ElementCount(82), BitPosition: null);

    private static readonly TemplateDefinition StringTemplate = new(
        new TemplateId(0xFCE), new TemplateName("STRING"), new StructureHandle(0xABCD), new StructureSize(88),
        [Len, Data]);

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
        capacity.Should().Be(new StringMaxLength((int)Data.ElementCount.Value));
    }

    [Fact]
    public void ATemplateWithoutASintArrayNamedDataHoldsNoCharacters()
    {
        // Arrange
        var template = StringTemplate with { Members = [Len, Data with { DataType = AllenBradleyDataType.Dint }] };

        // Act
        var capacity = template.StringCapacity;

        // Assert
        capacity.Should().BeNull();
    }
}
