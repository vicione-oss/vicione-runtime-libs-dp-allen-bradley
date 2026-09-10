using ViciOne.Suite.DataPort.AllenBradley.Logix.Client;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Booleans;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Strings;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Mapping;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Device;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Mapper;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Verification;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixCommunicationTestDataFactory;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.ProgramTagsNodeTestDataFactory;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.TagNodeTestDataFactory;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.TagDefinitionTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Verification;

/// <summary>
/// <see cref="LogixConfigurationVerifier.GetMismatches"/> is exercised directly per mismatch class;
/// <c>Verify</c>, the seam the dataport base class calls, end to end against a fake client.
/// </summary>
public sealed class LogixConfigurationVerifierTests
{
    [Fact]
    public void ATagWhoseTypeAndShapeAgreeWithTheControllerReportsNothing()
    {
        // Arrange
        var resolved = new ResolvedDataPoint(DIntPointNamed("Motor.Speed"), DefaultAtomicTagDefinition());

        // Act
        var mismatches = LogixConfigurationVerifier.GetMismatches(resolved);

        // Assert
        mismatches.Should().BeEmpty();
    }

    [Fact]
    public void ATagAbsentFromTheControllerIsReportedAsNotFound()
    {
        // Arrange
        var resolved = new ResolvedDataPoint(DIntPointNamed("Ghost"), TagDefinition: null);

        // Act
        var mismatches = LogixConfigurationVerifier.GetMismatches(resolved);

        // Assert
        mismatches.Should().ContainSingle().Which.Value.Should().Contain("Ghost").And.Contain("not found");
    }

    [Fact]
    public void ATagOfAnotherAtomicTypeIsReportedWithBothTypesNamed()
    {
        // Arrange
        var declaration = DefaultAtomicTagDefinition() with { DataType = AllenBradleyDataType.Real };
        var resolved = new ResolvedDataPoint(DIntPointNamed("Motor.Speed"), declaration);

        // Act
        var mismatches = LogixConfigurationVerifier.GetMismatches(resolved);

        // Assert
        mismatches.Should().ContainSingle().Which.Value.Should().Contain("DINT").And.Contain("Real");
    }

    [Fact]
    public void AStructureWhereAnElementaryTypeWasConfiguredIsReportedAsAStructure()
    {
        // Arrange
        var resolved = new ResolvedDataPoint(DIntPointNamed("Motor"), DefaultStringTagDefinition());

        // Act
        var mismatches = LogixConfigurationVerifier.GetMismatches(resolved);

        // Assert
        mismatches.Should().ContainSingle().Which.Value.Should().Contain("structure");
    }

    [Fact]
    public void AnArrayWhereAScalarWasConfiguredIsReportedWithBothShapesNamed()
    {
        // Arrange
        var declaration = DefaultAtomicTagDefinition() with { DimensionCount = DimensionCount.OneDimensional };
        var resolved = new ResolvedDataPoint(DIntPointNamed("Counts"), declaration);

        // Act
        var mismatches = LogixConfigurationVerifier.GetMismatches(resolved);

        // Assert
        mismatches.Should().ContainSingle()
            .Which.Value.Should().Contain("configured a scalar").And.Contain("1-dimensional array");
    }

    [Fact]
    public void AScalarWhereAnArrayWasConfiguredIsReportedWithBothShapesNamed()
    {
        // Arrange
        var resolved = new ResolvedDataPoint(IntArrayPointNamed("Readings"), DefaultAtomicTagDefinition());

        // Act
        var mismatches = LogixConfigurationVerifier.GetMismatches(resolved);

        // Assert
        mismatches.Should().ContainSingle()
            .Which.Value.Should().Contain("configured a 1-dimensional array").And.Contain("a scalar");
    }

    [Fact]
    public void ABoolArrayConfiguredAsAScalarIsReportedAsAShapeMismatch()
    {
        // Arrange
        var declaration = DefaultAtomicTagDefinition() with
        {
            DataType = AllenBradleyDataType.Bool,
            DimensionCount = DimensionCount.OneDimensional,
        };
        var resolved = new ResolvedDataPoint(
            new BoolDataPoint(new TagName("Flags"), DefaultPollFrequency, NoChannels), declaration);

        // Act
        var mismatches = LogixConfigurationVerifier.GetMismatches(resolved);

        // Assert
        mismatches.Should().ContainSingle().Which.Value.Should().Contain("array");
    }

    [Fact]
    public void AnIntArrayOfTheDeclaredCountReportsNothing()
    {
        // Arrange
        var resolved = new ResolvedDataPoint(
            IntArrayPointNamed("Readings"), DefaultIntArrayTagDefinition());

        // Act
        var mismatches = LogixConfigurationVerifier.GetMismatches(resolved);

        // Assert
        mismatches.Should().BeEmpty();
    }

    [Fact]
    public void AnElementCountThatDiffersIsReportedInElements()
    {
        // Arrange
        var declaration = DefaultIntArrayTagDefinition() with { ElementCount = new ElementCount(20) };
        var resolved = new ResolvedDataPoint(IntArrayPointNamed("Readings"), declaration);

        // Act
        var mismatches = LogixConfigurationVerifier.GetMismatches(resolved);

        // Assert
        mismatches.Should().ContainSingle().Which.Value.Should()
            .Contain("Readings").And.Contain("10").And.Contain("20").And.Contain("elements");
    }

    [Fact]
    public void AStringOfTheDeclaredCapacityReportsNothing()
    {
        // Arrange
        var resolved = new ResolvedDataPoint(StringPointNamed("Label"), DefaultStringTagDefinition());

        // Act
        var mismatches = LogixConfigurationVerifier.GetMismatches(resolved);

        // Assert
        mismatches.Should().BeEmpty();
    }

    [Fact]
    public void AnElementaryTagWhereAStringWasConfiguredIsReportedWithBothTypesNamed()
    {
        // Arrange
        var resolved = new ResolvedDataPoint(StringPointNamed("Motor.Speed"), DefaultAtomicTagDefinition());

        // Act
        var mismatches = LogixConfigurationVerifier.GetMismatches(resolved);

        // Assert
        mismatches.Should().ContainSingle().Which.Value.Should().Contain("STRING").And.Contain("Dint");
    }

    [Fact]
    public void AStringCapacityThatDiffersIsReportedInCharacters()
    {
        // Arrange
        var declaration = DefaultStringTagDefinition() with { MaxLength = new StringMaxLength(20) };
        var resolved = new ResolvedDataPoint(StringPointNamed("Label"), declaration);

        // Act
        var mismatches = LogixConfigurationVerifier.GetMismatches(resolved);

        // Assert
        mismatches.Should().ContainSingle()
            .Which.Value.Should().Contain("82").And.Contain("20").And.Contain("characters");
    }

    [Fact]
    public async Task OnlyTheMisconfiguredDataPointsComeBackFromAVerify()
    {
        // Arrange
        // "Missing" is deliberately absent, so the client resolves it with no definition.
        var verifier = new LogixConfigurationVerifier(new FakeClient
        {
            ["Good"] = DefaultAtomicTagDefinition(),
            ["WrongType"] = DefaultAtomicTagDefinition() with { DataType = AllenBradleyDataType.Real },
        });

        // Act
        var mismatches = await verifier.Verify(
            [DIntPointNamed("Good"), DIntPointNamed("WrongType"), DIntPointNamed("Missing")],
            TestContext.Current.CancellationToken);

        // Assert
        mismatches.Select(mismatch => mismatch.DataPoint.TagName.Value).Should()
            .BeEquivalentTo("WrongType", "Missing");
    }

    [Fact]
    public async Task EveryReportedMismatchNamesTheTagItIsAbout()
    {
        // Arrange
        var verifier = new LogixConfigurationVerifier(new FakeClient
        {
            ["WrongType"] = DefaultAtomicTagDefinition() with { DataType = AllenBradleyDataType.Real },
        });

        // Act
        var mismatches = await verifier.Verify(
            [DIntPointNamed("WrongType"), DIntPointNamed("Missing")], TestContext.Current.CancellationToken);

        // Assert
        mismatches.Should().AllSatisfy(mismatch => mismatch.MismatchingConfigurations.Should().ContainSingle()
            .Which.Value.Should().Contain(mismatch.DataPoint.TagName.Value));
    }

    [Fact]
    public async Task ATagUnderAProgramTheControllerHasNotIsReportedByItsQualifiedAddress()
    {
        // Arrange
        var verifier = new LogixConfigurationVerifier(new FakeClient
        {
            ["Program:MainProgram.Count"] = DefaultAtomicTagDefinition(),
        });
        var noSuchProgram = CreateProgramTagsNode("NoSuchProgram");
        var dataPoints = DataPointsOf(CreateCommunicationOf(
            [noSuchProgram, CreateDIntNode("Speed", "Count", noSuchProgram.Id)]));

        // Act
        var mismatches = await verifier.Verify(dataPoints, TestContext.Current.CancellationToken);

        // Assert
        mismatches.Should().ContainSingle()
            .Which.MismatchingConfigurations.Should().ContainSingle()
            .Which.Value.Should().Be("Tag 'Program:NoSuchProgram.Count' was not found on the controller.");
    }

    [Fact]
    public async Task ATagUnderTheProgramThatOwnsItReportsNothing()
    {
        // Arrange
        var verifier = new LogixConfigurationVerifier(new FakeClient
        {
            ["Program:MainProgram.Count"] = DefaultAtomicTagDefinition(),
        });
        var mainProgram = CreateProgramTagsNode("MainProgram");
        var dataPoints = DataPointsOf(CreateCommunicationOf(
            [mainProgram, CreateDIntNode("Speed", "Count", mainProgram.Id)]));

        // Act
        var mismatches = await verifier.Verify(dataPoints, TestContext.Current.CancellationToken);

        // Assert
        mismatches.Should().BeEmpty();
    }

    private static DIntDataPoint DIntPointNamed(string tagName) =>
        new(new TagName(tagName), DefaultPollFrequency, NoChannels);

    private static StringDataPoint StringPointNamed(string tagName) =>
        new(new TagName(tagName), DefaultPollFrequency, NoChannels, StringMaxLength.Standard);

    private static IntArrayDataPoint IntArrayPointNamed(string tagName) =>
        new(new TagName(tagName), DefaultPollFrequency, NoChannels, TenElements);

    // A hand-built point could not disagree with the walk about a program prefix.
    private static IReadOnlyList<ILogixDataPoint> DataPointsOf(LogixCommunication communication) =>
        new LogixDataPointsGroupsMapper().ToDataPoints(
            TypedLogixNodeMapper.Instance().MapToTypedNodes(communication));

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
