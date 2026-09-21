using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Access.LibPlcTag;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Symbols;
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
    public async Task TheStringTagIsDeclaredAStringOfItsTemplatesCapacity()
    {
        // Arrange
        // The client exposes no lookup, so the browse it ran at connect is run once more here.
        var loader = new SymbolTableLoader(new LogixTagAccessFactory(TestController.ClientInformation));

        // Act
        var symbolTable = await loader.LoadAsync(TestContext.Current.CancellationToken);

        // Assert
        var expected = new DeclaredType(
            new TagAddress(TagAddresses.String),
            AllenBradleyDataType.String,
            TagAddresses.StringCapacity,
            DimensionCount.Scalar,
            ElementCount.Scalar);
        symbolTable.GetDeclaredTypeAtPath(TagPath.Parse(TagAddresses.String)).Should().Be(expected);
    }

    [Fact]
    public async Task TheLengthMemberOfTheStringTagIsADintAtTheFront()
    {
        // Arrange
        var loader = new SymbolTableLoader(new LogixTagAccessFactory(TestController.ClientInformation));

        // Act
        var symbolTable = await loader.LoadAsync(TestContext.Current.CancellationToken);

        // Assert
        var expected = new DeclaredType(
            new TagAddress($"{TagAddresses.String}.LEN"),
            AllenBradleyDataType.Dint,
            MaxLength: null,
            DimensionCount.Scalar,
            ElementCount.Scalar);
        symbolTable.GetDeclaredTypeAtPath(TagPath.Parse($"{TagAddresses.String}.LEN")).Should().Be(expected);
    }

    [Fact]
    public async Task TheDataMemberOfTheStringTagIsEightyTwoSints()
    {
        // Arrange
        var loader = new SymbolTableLoader(new LogixTagAccessFactory(TestController.ClientInformation));

        // Act
        var symbolTable = await loader.LoadAsync(TestContext.Current.CancellationToken);

        // Assert
        var expected = new DeclaredType(
            new TagAddress($"{TagAddresses.String}.DATA"),
            AllenBradleyDataType.Sint,
            MaxLength: null,
            DimensionCount.OneDimensional,
            new ElementCount(82));
        symbolTable.GetDeclaredTypeAtPath(TagPath.Parse($"{TagAddresses.String}.DATA")).Should().Be(expected);
    }
}
