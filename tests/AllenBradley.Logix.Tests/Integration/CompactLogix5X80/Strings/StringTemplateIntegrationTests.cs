using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration.Templates;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Access.LibPlcTag;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Definitions;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80.Strings;

/// <summary>
/// The one structure whose template every Logix controller has: what the browse reads for the
/// <c>STRING</c> tag pins the real <c>@udt/&lt;id&gt;</c> byte layout the synthetic buffers imitate.
/// </summary>
public sealed class StringTemplateIntegrationTests(ITestOutputHelper output)
    : CompactLogix5X80IntegrationTestBase(output)
{
    [Fact]
    public async Task TheStringTagNamesTheBuiltInStringTemplate()
    {
        // Arrange
        // The client exposes no template lookup, so the browse it ran at connect is run once more here.
        var loader = new TagDefinitionsLoader(new LogixTagAccessFactory(TestController.ClientInformation));

        // Act
        var definitions = await loader.LoadAsync(TestContext.Current.CancellationToken);

        // Assert
        definitions.Lookup(new TagAddress(TagAddresses.String))!.Value.TemplateId
            .Should().Be(PredefinedTemplates.String);
    }

    [Fact]
    public async Task TheBuiltInStringTemplateIsALengthWordBeforeEightyTwoCharacters()
    {
        // Arrange
        var loader = new TagDefinitionsLoader(new LogixTagAccessFactory(TestController.ClientInformation));

        // Act
        var definitions = await loader.LoadAsync(TestContext.Current.CancellationToken);

        // Assert
        // The handle is a CRC the controller computes, so it is the one field not pinned here.
        var expected = new TemplateDefinition(
            PredefinedTemplates.String,
            new TemplateName("STRING"),
            Handle: default,
            new StructureSize(88),
            [
                new TemplateMember(
                    new MemberName("LEN"), new MemberOffset(0), AllenBradleyDataType.Dint, TemplateId: null,
                    DimensionCount.Scalar, ElementCount.Scalar, BitPosition: null),
                new TemplateMember(
                    new MemberName("DATA"), new MemberOffset(4), AllenBradleyDataType.Sint, TemplateId: null,
                    DimensionCount.OneDimensional, new ElementCount(82), BitPosition: null),
            ]);
        definitions.LookupTemplate(PredefinedTemplates.String)
            .Should().BeEquivalentTo(expected, options => options.Excluding(template => template.Handle));
    }
}
