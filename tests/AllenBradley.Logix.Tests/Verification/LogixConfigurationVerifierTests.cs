using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Lifetime;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Verification;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Verification;

/// <summary>
/// The configuration diff. <see cref="LogixConfigurationVerifier.GetMismatches"/> is the whole of the
/// verification rule, so it is exercised directly per mismatch class, and <c>VerifyAsync</c> is checked
/// once end to end against a fake tag manager — the verifier projects its tags, it no longer
/// browses its own schema.
/// </summary>
public class LogixConfigurationVerifierTests
{
    private static TagDefinition Declaration(
        bool isStruct = false,
        AllenBradleyDataType? dataType = AllenBradleyDataType.Dint,
        int dimensionCount = 0,
        int? maxLength = null) =>
        new(
            new TagName("Tag"),
            isStruct ? LogixTypeKind.Structure : LogixTypeKind.Atomic,
            dataType,
            maxLength is { } length ? new StringMaxLength(length) : null,
            new DimensionCount(dimensionCount),
            new ElementCount(1));

    // What the controller reports for a built-in STRING: a structure of .DATA[82] behind its .LEN.
    private static TagDefinition StringDeclaration(int maxLength = 82) =>
        Declaration(isStruct: true, dataType: AllenBradleyDataType.String, maxLength: maxLength);

    private static ResolvedDataPoint Resolved(ILogixDataPoint dataPoint, TagDefinition? device) =>
        new(dataPoint, device);

    [Fact]
    public void GetMismatches_WhenTypeAndShapeMatch_ReportsNothing()
    {
        var resolved = Resolved(CreateDInt("Motor.Speed"), Declaration(dataType: AllenBradleyDataType.Dint));

        LogixConfigurationVerifier.GetMismatches(resolved).Should().BeEmpty();
    }

    [Fact]
    public void GetMismatches_WhenTheTagIsAbsent_ReportsNotFound()
    {
        var resolved = Resolved(CreateDInt("Ghost"), device: null);

        LogixConfigurationVerifier.GetMismatches(resolved)
            .Should().ContainSingle()
            .Which.Description.Should().Contain("Ghost").And.Contain("not found");
    }

    [Fact]
    public void GetMismatches_WhenTheAtomicTypeDiffers_ReportsTheMismatch()
    {
        // DINT configured, REAL on the controller.
        var resolved = Resolved(CreateDInt("Motor.Speed"), Declaration(dataType: AllenBradleyDataType.Real));

        LogixConfigurationVerifier.GetMismatches(resolved)
            .Should().ContainSingle()
            .Which.Description.Should().Contain("Dint").And.Contain("Real");
    }

    [Fact]
    public void GetMismatches_WhenTheControllerTagIsAStructure_ReportsAShapeMismatch()
    {
        var resolved = Resolved(CreateDInt("Motor"), StringDeclaration());

        LogixConfigurationVerifier.GetMismatches(resolved)
            .Should().ContainSingle()
            .Which.Description.Should().Contain("structure");
    }

    [Fact]
    public void GetMismatches_WhenTheControllerTagIsAnArray_ReportsAShapeMismatch()
    {
        var resolved = Resolved(CreateDInt("Counts"), Declaration(dataType: AllenBradleyDataType.Dint, dimensionCount: 1));

        LogixConfigurationVerifier.GetMismatches(resolved)
            .Should().ContainSingle()
            .Which.Description.Should().Contain("array");
    }

    [Fact]
    public async Task VerifyAsync_ReturnsOnlyTheMisconfiguredDataPoints()
    {
        var tagManager = new FakeTagManager
        {
            ["Good"] = Declaration(dataType: AllenBradleyDataType.Dint),
            ["WrongType"] = Declaration(dataType: AllenBradleyDataType.Real),
            // "Missing" is deliberately absent — the manager stamps null metadata on its tag.
        };
        var verifier = new LogixConfigurationVerifier(tagManager);

        var result = await verifier.VerifyAsync(
            [
                CreateDInt("Good"),
                CreateDInt("WrongType"),
                CreateDInt("Missing"),
            ],
            TestContext.Current.CancellationToken);

        result.Select(m => m.DataPoint.TagName.Value).Should().BeEquivalentTo("WrongType", "Missing");
    }

    // Stands in for CachingLogixTagManager: it maps a tag name to the metadata the controller would
    // report and hands back a tag carrying it (null for an absent tag).
    private sealed class FakeTagManager : ILogixTagManager
    {
        private readonly Dictionary<TagName, TagDefinition> _declarations =
            new(TagName.CaseInsensitiveComparer);

        public TagDefinition this[string tagName]
        {
            set => _declarations[new TagName(tagName)] = value;
        }

        public Task LoadTagDefinitionsAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public ILogixTag TagFor(ILogixDataPoint dataPoint) =>
            new ProjectionTag(
                dataPoint,
                _declarations.TryGetValue(dataPoint.TagName, out var declaration) ? declaration : null);
    }

    // Only the projected getters matter to the verifier; it never reads or writes the tag.
    private sealed class ProjectionTag(ILogixDataPoint dataPoint, TagDefinition? metadata)
        : ILogixTag
    {
        public ILogixDataPoint DataPoint => dataPoint;

        public TagDefinition? Metadata => metadata;

        public Task<LogixTagReadResult> ReadAsync(CancellationToken cancellationToken) =>
            throw new NotSupportedException("Verification projects metadata; it does not read the tag.");

        public Task<LogixTagWriteResult> WriteAsync(byte[] buffer, CancellationToken cancellationToken) =>
            throw new NotSupportedException("Verification projects metadata; it does not write the tag.");

        public void Dispose()
        {
        }
    }
}
