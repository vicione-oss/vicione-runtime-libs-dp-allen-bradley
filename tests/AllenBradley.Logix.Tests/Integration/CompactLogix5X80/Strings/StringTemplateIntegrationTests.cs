using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Access.LibPlcTag;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Symbols;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80.Infrastructure;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80.Strings;

/// <summary>
/// Every Logix controller has a <c>STRING</c> template. Its members pin the real <c>@udt/&lt;id&gt;</c> byte
/// layout that the synthetic buffers imitate.
/// </summary>
public abstract class StringTemplateIntegrationTests(
    TestControllerFixture fixture,
    ITestOutputHelper output,
    TagScope scope)
    : CompactLogix5X80IntegrationTestBase(fixture, output, scope)
{
    [Fact]
    public async Task AStringHoldsEightyTwoCharacters()
    {
        // Arrange
        // The client exposes no lookup, so the test repeats the browse that ran at connect.
        var loader = new SymbolTableLoader(new LogixTagAccessFactory(TestControllerConfiguration.ClientInformation));

        // Act
        var symbolTable = await loader.LoadAsync(TestContext.Current.CancellationToken);

        // Assert
        var expected = new DeclaredType(
            new TagAddress(Tags.String),
            AllenBradleyDataType.String,
            TagAddresses.StringCapacity,
            DimensionCount.Scalar,
            ElementCount.Scalar);
        symbolTable.GetDeclaredTypeAtPath(TagPath.Parse(Tags.String)).Should().Be(expected);
    }

    [Fact]
    public async Task LenIsAdInt()
    {
        // Arrange
        var loader = new SymbolTableLoader(new LogixTagAccessFactory(TestControllerConfiguration.ClientInformation));

        // Act
        var symbolTable = await loader.LoadAsync(TestContext.Current.CancellationToken);

        // Assert
        var expected = new DeclaredType(
            new TagAddress($"{Tags.String}.LEN"),
            AllenBradleyDataType.Dint,
            MaxLength: null,
            DimensionCount.Scalar,
            ElementCount.Scalar);
        symbolTable.GetDeclaredTypeAtPath(TagPath.Parse($"{Tags.String}.LEN")).Should().Be(expected);
    }

    [Fact]
    public async Task DataIsEightyTwoSInts()
    {
        // Arrange
        var loader = new SymbolTableLoader(new LogixTagAccessFactory(TestControllerConfiguration.ClientInformation));

        // Act
        var symbolTable = await loader.LoadAsync(TestContext.Current.CancellationToken);

        // Assert
        var expected = new DeclaredType(
            new TagAddress($"{Tags.String}.DATA"),
            AllenBradleyDataType.Sint,
            MaxLength: null,
            DimensionCount.OneDimensional,
            new ElementCount(82));
        symbolTable.GetDeclaredTypeAtPath(TagPath.Parse($"{Tags.String}.DATA")).Should().Be(expected);
    }

    public sealed class ControllerScope(TestControllerFixture fixture, ITestOutputHelper output)
        : StringTemplateIntegrationTests(fixture, output, TagScope.Controller);

    public sealed class ProgramScope(TestControllerFixture fixture, ITestOutputHelper output)
        : StringTemplateIntegrationTests(fixture, output, TagScope.Program);
}
