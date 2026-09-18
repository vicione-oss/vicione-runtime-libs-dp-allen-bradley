using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Access.LibPlcTag;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Definitions;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80.Strings;

/// <summary>
/// The one structure whose template every Logix controller has: the members the lookup reads off the
/// <c>STRING</c> tag's template pin the real <c>@udt/&lt;id&gt;</c> byte layout the synthetic buffers imitate.
/// </summary>
public sealed class StringTemplateIntegrationTests(ITestOutputHelper output)
    : CompactLogix5X80IntegrationTestBase(output)
{
    [Fact]
    public async Task TheStringTagNamesTheBuiltInStringTemplate()
    {
        // Arrange
        // The client exposes no lookup, so the browse it ran at connect is run once more here.
        var loader = new TagDefinitionsLoader(new LogixTagAccessFactory(TestController.ClientInformation));

        // Act
        var definitions = await loader.LoadAsync(TestContext.Current.CancellationToken);

        // Assert
        definitions.Lookup(TagPath.Parse(TagAddresses.String))!.Value.TemplateId
            .Should().Be(PredefinedTemplates.String);
    }

    [Fact]
    public async Task TheLengthMemberOfTheStringTagIsADintAtTheFront()
    {
        // Arrange
        var loader = new TagDefinitionsLoader(new LogixTagAccessFactory(TestController.ClientInformation));

        // Act
        var definitions = await loader.LoadAsync(TestContext.Current.CancellationToken);

        // Assert
        var expected = new TagDefinition(
            new TagAddress($"{TagAddresses.String}.LEN"), AllenBradleyDataType.Dint, TemplateId: null,
            MaxLength: null, DimensionCount.Scalar, ElementCount.Scalar);
        definitions.Lookup(TagPath.Parse($"{TagAddresses.String}.LEN")).Should().Be(expected);
    }

    [Fact]
    public async Task TheDataMemberOfTheStringTagIsEightyTwoSints()
    {
        // Arrange
        var loader = new TagDefinitionsLoader(new LogixTagAccessFactory(TestController.ClientInformation));

        // Act
        var definitions = await loader.LoadAsync(TestContext.Current.CancellationToken);

        // Assert
        var expected = new TagDefinition(
            new TagAddress($"{TagAddresses.String}.DATA"), AllenBradleyDataType.Sint, TemplateId: null,
            MaxLength: null, DimensionCount.OneDimensional, new ElementCount(82));
        definitions.Lookup(TagPath.Parse($"{TagAddresses.String}.DATA")).Should().Be(expected);
    }
}
