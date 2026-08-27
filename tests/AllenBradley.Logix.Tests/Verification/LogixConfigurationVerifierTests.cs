using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Verification;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Verification;

/// <summary>
/// The configuration diff. <see cref="LogixConfigurationVerifier.GetMismatches"/> is the whole of the
/// verification rule, so it is exercised directly per mismatch class, and <c>VerifyAsync</c> is checked
/// once end to end against a fake tag manager — the verifier projects its tags, it no longer
/// browses its own schema.
/// </summary>
public class LogixConfigurationVerifierTests
{
    private static LogixTypeDeclaration Declaration(
        bool isStruct = false, CipType? atomicType = CipType.Dint, int dimensionCount = 0) =>
        new(
            new TagName("Tag"),
            isStruct ? LogixTypeKind.Structure : LogixTypeKind.Atomic,
            atomicType,
            new DimensionCount(dimensionCount),
            new ElementCount(1),
            new ElementLength(4));

    private static LogixResolvedDataPoint Resolved(ILogixDataPoint dataPoint, LogixTypeDeclaration? device) =>
        new(dataPoint, device);

    [Fact]
    public void GetMismatches_WhenTypeAndShapeMatch_ReportsNothing()
    {
        var resolved = Resolved(new DIntDataPoint(new TagName("Motor.Speed")), Declaration(atomicType: CipType.Dint));

        LogixConfigurationVerifier.GetMismatches(resolved).Should().BeEmpty();
    }

    [Fact]
    public void GetMismatches_WhenTheTagIsAbsent_ReportsNotFound()
    {
        var resolved = Resolved(new DIntDataPoint(new TagName("Ghost")), device: null);

        LogixConfigurationVerifier.GetMismatches(resolved)
            .Should().ContainSingle()
            .Which.Description.Should().Contain("Ghost").And.Contain("not found");
    }

    [Fact]
    public void GetMismatches_WhenTheAtomicTypeDiffers_ReportsTheMismatch()
    {
        // DINT configured, REAL on the controller.
        var resolved = Resolved(new DIntDataPoint(new TagName("Motor.Speed")), Declaration(atomicType: CipType.Real));

        LogixConfigurationVerifier.GetMismatches(resolved)
            .Should().ContainSingle()
            .Which.Description.Should().Contain("Dint").And.Contain("Real");
    }

    [Fact]
    public void GetMismatches_WhenTheControllerTagIsAStructure_ReportsAShapeMismatch()
    {
        var resolved = Resolved(new DIntDataPoint(new TagName("Motor")), Declaration(isStruct: true, atomicType: null));

        LogixConfigurationVerifier.GetMismatches(resolved)
            .Should().ContainSingle()
            .Which.Description.Should().Contain("structure");
    }

    [Fact]
    public void GetMismatches_WhenTheControllerTagIsAnArray_ReportsAShapeMismatch()
    {
        var resolved = Resolved(new DIntDataPoint(new TagName("Counts")), Declaration(atomicType: CipType.Dint, dimensionCount: 1));

        LogixConfigurationVerifier.GetMismatches(resolved)
            .Should().ContainSingle()
            .Which.Description.Should().Contain("array");
    }

    [Fact]
    public async Task VerifyAsync_ReturnsOnlyTheMisconfiguredDataPoints()
    {
        var tagManager = new FakeTagManager
        {
            ["Good"] = Declaration(atomicType: CipType.Dint),
            ["WrongType"] = Declaration(atomicType: CipType.Real),
            // "Missing" is deliberately absent — the manager stamps null metadata on its tag.
        };
        var verifier = new LogixConfigurationVerifier(tagManager);

        var result = await verifier.VerifyAsync(
            [
                new DIntDataPoint(new TagName("Good")),
                new DIntDataPoint(new TagName("WrongType")),
                new DIntDataPoint(new TagName("Missing")),
            ],
            TestContext.Current.CancellationToken);

        result.Select(m => m.DataPoint.TagName.Value).Should().BeEquivalentTo("WrongType", "Missing");
    }

    // Stands in for CachingLogixTagManager: it maps a tag name to the metadata the controller would
    // report and hands back a tag carrying it (null for an absent tag).
    private sealed class FakeTagManager : ILogixTagManager
    {
        private readonly Dictionary<TagName, LogixTypeDeclaration> _declarations =
            new(TagName.CaseInsensitiveComparer);

        public LogixTypeDeclaration this[string tagName]
        {
            set => _declarations[new TagName(tagName)] = value;
        }

        public Task LoadSchemaAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public ILogixTag TagFor(ILogixDataPoint dataPoint) =>
            new ProjectionTag(
                dataPoint,
                _declarations.TryGetValue(dataPoint.TagName, out var declaration) ? declaration : null);
    }

    // Only the projected getters matter to the verifier; it never reads or writes the tag.
    private sealed class ProjectionTag(ILogixDataPoint dataPoint, LogixTypeDeclaration? metadata)
        : ILogixTag
    {
        public ILogixDataPoint DataPoint => dataPoint;

        public LogixTypeDeclaration? Metadata => metadata;

        public Task<LogixTagReadResult> ReadAsync(CancellationToken cancellationToken) =>
            throw new NotSupportedException("Verification projects metadata; it does not read the tag.");

        public Task<LogixTagWriteResult> WriteAsync(byte[] buffer, CancellationToken cancellationToken) =>
            throw new NotSupportedException("Verification projects metadata; it does not write the tag.");

        public void Dispose()
        {
        }
    }
}
