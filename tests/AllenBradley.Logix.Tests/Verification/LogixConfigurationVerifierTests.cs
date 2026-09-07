using ViciOne.Suite.DataPort.AllenBradley.Logix.Client;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Booleans;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Strings;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Mapping;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Device;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Mapper;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Verification;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixCommunicationTestDataFactory;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.ProgramTagsNodeTestDataFactory;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.ScalarNodeTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Verification;

/// <summary>
/// The configuration diff. <see cref="LogixConfigurationVerifier.GetMismatches"/> is the whole of the
/// verification rule, so it is exercised directly per mismatch class, and <c>Verify</c> — the seam the
/// dataport base class calls — is checked once end to end against a fake client: the verifier resolves
/// through <see cref="ILogixClient.ResolveDataPoints"/>, it neither browses nor holds a tag manager.
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
        // Arrange
        var resolved = Resolved(new DIntDataPoint(new TagName("Motor.Speed"), LogixDataPointTestDataFactory.DefaultPollFrequency, NoChannels), Declaration(dataType: AllenBradleyDataType.Dint));

        // Act
        var mismatches = LogixConfigurationVerifier.GetMismatches(resolved);

        // Assert
        mismatches.Should().BeEmpty();
    }

    [Fact]
    public void GetMismatches_WhenTheTagIsAbsent_ReportsNotFound()
    {
        // Arrange
        var resolved = Resolved(new DIntDataPoint(new TagName("Ghost"), LogixDataPointTestDataFactory.DefaultPollFrequency, NoChannels), device: null);

        // Act
        var mismatches = LogixConfigurationVerifier.GetMismatches(resolved);

        // Assert
        mismatches.Should().ContainSingle()
            .Which.Value.Should().Contain("Ghost").And.Contain("not found");
    }

    [Fact]
    public void GetMismatches_WhenTheAtomicTypeDiffers_ReportsTheMismatch()
    {
        // Arrange
        // DINT configured, REAL on the controller.
        var resolved = Resolved(new DIntDataPoint(new TagName("Motor.Speed"), LogixDataPointTestDataFactory.DefaultPollFrequency, NoChannels), Declaration(dataType: AllenBradleyDataType.Real));

        // Act
        var mismatches = LogixConfigurationVerifier.GetMismatches(resolved);

        // Assert
        mismatches.Should().ContainSingle()
            .Which.Value.Should().Contain("DINT").And.Contain("Real");
    }

    [Fact]
    public void GetMismatches_WhenTheControllerTagIsAStructure_ReportsAShapeMismatch()
    {
        // Arrange
        var resolved = Resolved(new DIntDataPoint(new TagName("Motor"), LogixDataPointTestDataFactory.DefaultPollFrequency, NoChannels), StringDeclaration());

        // Act
        var mismatches = LogixConfigurationVerifier.GetMismatches(resolved);

        // Assert
        mismatches.Should().ContainSingle()
            .Which.Value.Should().Contain("structure");
    }

    [Fact]
    public void GetMismatches_WhenTheControllerTagIsAnArray_ReportsAShapeMismatch()
    {
        // Arrange
        var resolved = Resolved(new DIntDataPoint(new TagName("Counts"), LogixDataPointTestDataFactory.DefaultPollFrequency, NoChannels), Declaration(dataType: AllenBradleyDataType.Dint, dimensionCount: 1));

        // Act
        var mismatches = LogixConfigurationVerifier.GetMismatches(resolved);

        // Assert
        mismatches.Should().ContainSingle()
            .Which.Value.Should().Contain("array");
    }

    [Fact]
    public void GetMismatches_WhenABoolArrayIsConfiguredAsAScalar_ReportsAShapeMismatch()
    {
        // Arrange
        // The BOOL node is the atomic tag only. A BOOL[] packs eight to the byte, and this is what keeps
        // one configured as a scalar from being read a bit at a time.
        var resolved = Resolved(
            new BoolDataPoint(new TagName("Flags"), LogixDataPointTestDataFactory.DefaultPollFrequency, NoChannels),
            Declaration(dataType: AllenBradleyDataType.Bool, dimensionCount: 1));

        // Act
        var mismatches = LogixConfigurationVerifier.GetMismatches(resolved);

        // Assert
        mismatches.Should().ContainSingle()
            .Which.Value.Should().Contain("array");
    }

    [Fact]
    public void GetMismatches_WhenAStringMatchesTheDeclaredCapacity_ReportsNothing()
    {
        // Arrange
        var resolved = Resolved(new StringDataPoint(new TagName("Label"), LogixDataPointTestDataFactory.DefaultPollFrequency, NoChannels, new StringMaxLength(StringMaxLength.Standard.Value)), StringDeclaration());

        // Act
        var mismatches = LogixConfigurationVerifier.GetMismatches(resolved);

        // Assert
        mismatches.Should().BeEmpty();
    }

    [Fact]
    public void GetMismatches_WhenTheControllerTagIsElementaryButAStringIsConfigured_ReportsAShapeMismatch()
    {
        // Arrange
        // The inverse of the structure case: a STRING configured onto a DINT tag.
        var resolved = Resolved(
            new StringDataPoint(new TagName("Motor.Speed"), LogixDataPointTestDataFactory.DefaultPollFrequency, NoChannels, new StringMaxLength(StringMaxLength.Standard.Value)),
            Declaration(dataType: AllenBradleyDataType.Dint));

        // Act
        var mismatches = LogixConfigurationVerifier.GetMismatches(resolved);

        // Assert
        mismatches.Should().ContainSingle()
            .Which.Value.Should().Contain("STRING").And.Contain("Dint");
    }

    [Fact]
    public void GetMismatches_WhenTheDeclaredStringCapacityDiffers_ReportsTheCapacity()
    {
        // Arrange
        // A STRING (82) configured onto a STRING_20. A round trip of a short value would never show
        // this — hence its own mismatch kind. Both numbers are characters, the unit the configuration
        // is written in.
        var resolved = Resolved(
            new StringDataPoint(new TagName("Label"), LogixDataPointTestDataFactory.DefaultPollFrequency, NoChannels, new StringMaxLength(StringMaxLength.Standard.Value)),
            StringDeclaration(maxLength: 20));

        // Act
        var mismatches = LogixConfigurationVerifier.GetMismatches(resolved);

        // Assert
        mismatches.Should().ContainSingle()
            .Which.Value.Should().Contain("82").And.Contain("20").And.Contain("characters");
    }

    [Fact]
    public async Task Verify_ReturnsOnlyTheMisconfiguredDataPoints_WithTheirReasons()
    {
        // Arrange
        var client = new FakeClient
        {
            ["Good"] = Declaration(dataType: AllenBradleyDataType.Dint),
            ["WrongType"] = Declaration(dataType: AllenBradleyDataType.Real),
            // "Missing" is deliberately absent — the client resolves it with null metadata.
        };
        var verifier = new LogixConfigurationVerifier(client);

        // Act
        var result = await verifier.Verify(
            [
                new DIntDataPoint(new TagName("Good"), LogixDataPointTestDataFactory.DefaultPollFrequency, NoChannels),
                new DIntDataPoint(new TagName("WrongType"), LogixDataPointTestDataFactory.DefaultPollFrequency, NoChannels),
                new DIntDataPoint(new TagName("Missing"), LogixDataPointTestDataFactory.DefaultPollFrequency, NoChannels),
            ],
            TestContext.Current.CancellationToken);

        // Assert
        result.Select(m => m.DataPoint.TagName.Value).Should().BeEquivalentTo("WrongType", "Missing");
        // The reason travels with the point: a base class that logs the result names the tag, not a count.
        result.Should().AllSatisfy(m => m.MismatchingConfigurations.Should().ContainSingle()
            .Which.Value.Should().Contain(m.DataPoint.TagName.Value));
    }

    [Fact]
    public async Task Verify_ATagUnderAProgramTheControllerHasNot_ReportsTheQualifiedAddressAsNotFound()
    {
        // Arrange
        var client = new FakeClient
        {
            ["Program:MainProgram.Count"] = Declaration(dataType: AllenBradleyDataType.Dint),
        };
        var noSuchProgram = CreateProgramTagsNode("NoSuchProgram");
        var dataPoints = DataPointsOf(CreateCommunicationOf(
        [
            noSuchProgram,
            CreateDIntNode("Speed", "Count", noSuchProgram.Id),
        ]));
        var verifier = new LogixConfigurationVerifier(client);

        // Act
        var result = await verifier.Verify(dataPoints, TestContext.Current.CancellationToken);

        // Assert
        result.Should().ContainSingle()
            .Which.MismatchingConfigurations.Should().ContainSingle()
            .Which.Value.Should().Be("Tag 'Program:NoSuchProgram.Count' was not found on the controller.");
    }

    [Fact]
    public async Task Verify_ATagUnderTheProgramThatOwnsIt_ReportsNothing()
    {
        // Arrange
        var client = new FakeClient
        {
            ["Program:MainProgram.Count"] = Declaration(dataType: AllenBradleyDataType.Dint),
        };
        var mainProgram = CreateProgramTagsNode("MainProgram");
        var dataPoints = DataPointsOf(CreateCommunicationOf(
        [
            mainProgram,
            CreateDIntNode("Speed", "Count", mainProgram.Id),
        ]));
        var verifier = new LogixConfigurationVerifier(client);

        // Act
        var result = await verifier.Verify(dataPoints, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeEmpty();
    }

    // The tree the engine's configuration produces, walked into the points the verifier is handed. A
    // hand-built point could not disagree with the walk about a program prefix, which is the whole
    // question these two cases ask.
    private static IReadOnlyList<ILogixDataPoint> DataPointsOf(LogixCommunication communication) =>
        new LogixDataPointsGroupsMapper().ToDataPoints(
            TypedLogixNodeMapper.Instance().MapToTypedNodes(communication));

    // Stands in for LogixClient: it maps a tag name to the metadata the controller would report and
    // resolves each data point against it, with null for a tag the controller does not have. Only
    // ResolveDataPoints matters here — the verifier neither reads nor writes, and it does not drive the
    // client's lifecycle.
    private sealed class FakeClient : ILogixClient
    {
        private readonly Dictionary<TagName, TagDefinition> _declarations =
            new(TagName.CaseInsensitiveComparer);

        public TagDefinition this[string tagName]
        {
            set => _declarations[new TagName(tagName)] = value;
        }

        public bool IsConnected => true;

        public Task<IReadOnlyList<ResolvedDataPoint>> ResolveDataPoints(
            IReadOnlyList<ILogixDataPoint> dataPoints, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<ResolvedDataPoint>>(
            [
                .. dataPoints.Select(dataPoint => new ResolvedDataPoint(
                    dataPoint,
                    _declarations.TryGetValue(dataPoint.TagName, out var declaration) ? declaration : null)),
            ]);

        public Task ConnectAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task DisconnectAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public ValueTask<IReadOnlyList<ILogixDataPointValue>> ReadAsync(
            LogixDataPointGroup dataPointGroup, CancellationToken cancellationToken) =>
            throw new NotSupportedException("Verification resolves metadata; it does not read.");

        public ValueTask WriteAsync(
            IReadOnlyList<ILogixDataPointValue> values, CancellationToken cancellationToken) =>
            throw new NotSupportedException("Verification resolves metadata; it does not write.");

        public void Dispose()
        {
        }
    }
}
