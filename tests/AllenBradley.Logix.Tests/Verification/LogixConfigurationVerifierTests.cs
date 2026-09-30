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
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.DeclaredTypeTestDataFactory;

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
        var resolved = new ResolvedDataPoint(DIntPointNamed("Motor.Speed"), DefaultAtomicDeclaredType());

        // Act
        var mismatches = LogixConfigurationVerifier.GetMismatches(resolved);

        // Assert
        mismatches.Should().BeEmpty();
    }

    [Fact]
    public void ATagAbsentFromTheControllerIsReportedAsNotFound()
    {
        // Arrange
        var resolved = new ResolvedDataPoint(DIntPointNamed("Ghost"), DeclaredType: null);

        // Act
        var mismatches = LogixConfigurationVerifier.GetMismatches(resolved);

        // Assert
        mismatches.Should().ContainSingle().Which.Value.Should().Contain("Ghost").And.Contain("not found");
    }

    [Fact]
    public void ATagOfAnotherAtomicTypeIsReportedWithBothTypesNamed()
    {
        // Arrange
        var declaration = DefaultAtomicDeclaredType() with { DataType = AllenBradleyDataType.Real };
        var resolved = new ResolvedDataPoint(DIntPointNamed("Motor.Speed"), declaration);

        // Act
        var mismatches = LogixConfigurationVerifier.GetMismatches(resolved);

        // Assert
        mismatches.Should().ContainSingle().Which.Value.Should().Be(
            "Data type mismatch for tag 'Motor.Speed': configured DINT, controller reports REAL.");
    }

    [Fact]
    public void AStringWhereAnElementaryTypeWasConfiguredIsReportedWithBothTypesNamed()
    {
        // Arrange
        var resolved = new ResolvedDataPoint(DIntPointNamed("Motor"), DefaultStringDeclaredType());

        // Act
        var mismatches = LogixConfigurationVerifier.GetMismatches(resolved);

        // Assert
        mismatches.Should().ContainSingle().Which.Value.Should().Contain("DINT").And.Contain("STRING");
    }

    [Fact]
    public void AnArrayWhereAScalarWasConfiguredIsReportedWithBothShapesNamed()
    {
        // Arrange
        var declaration = DefaultAtomicDeclaredType() with { DimensionCount = DimensionCount.OneDimensional };
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
        var resolved = new ResolvedDataPoint(IntArrayPointNamed("Readings"), DefaultAtomicDeclaredType());

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
        var declaration = DefaultAtomicDeclaredType() with
        {
            DataType = AllenBradleyDataType.Bool,
            DimensionCount = DimensionCount.OneDimensional,
        };
        var resolved = new ResolvedDataPoint(
            new BoolDataPoint(TagPath.Parse("Flags"), DefaultPollFrequency, NoChannels),
            declaration);

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
            IntArrayPointNamed("Readings"), DefaultIntArrayDeclaredType());

        // Act
        var mismatches = LogixConfigurationVerifier.GetMismatches(resolved);

        // Assert
        mismatches.Should().BeEmpty();
    }

    [Fact]
    public void AnElementCountThatDiffersIsReportedInElements()
    {
        // Arrange
        var declaration = DefaultIntArrayDeclaredType() with { ElementCount = new ElementCount(20) };
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
        var resolved = new ResolvedDataPoint(StringPointNamed("Label"), DefaultStringDeclaredType());

        // Act
        var mismatches = LogixConfigurationVerifier.GetMismatches(resolved);

        // Assert
        mismatches.Should().BeEmpty();
    }

    [Fact]
    public void AnElementaryTagWhereAStringWasConfiguredIsReportedWithBothTypesNamed()
    {
        // Arrange
        var resolved = new ResolvedDataPoint(StringPointNamed("Motor.Speed"), DefaultAtomicDeclaredType());

        // Act
        var mismatches = LogixConfigurationVerifier.GetMismatches(resolved);

        // Assert
        mismatches.Should().ContainSingle().Which.Value.Should().Contain("STRING").And.Contain("DINT");
    }

    [Fact]
    public void AStringCapacityThatDiffersIsReportedInCharacters()
    {
        // Arrange
        var declaration = DefaultStringDeclaredType() with { MaxLength = new StringMaxLength(20) };
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
        // "Missing" is deliberately absent, so the client resolves it with no declared type.
        var verifier = new LogixConfigurationVerifier(new FakeClient
        {
            ["Good"] = DefaultAtomicDeclaredType(),
            ["WrongType"] = DefaultAtomicDeclaredType() with { DataType = AllenBradleyDataType.Real },
        });

        // Act
        var mismatches = await verifier.Verify(
            [DIntPointNamed("Good"), DIntPointNamed("WrongType"), DIntPointNamed("Missing")],
            TestContext.Current.CancellationToken);

        // Assert
        mismatches.Select(mismatch => mismatch.DataPoint.TagAddress.Value).Should()
            .BeEquivalentTo("WrongType", "Missing");
    }

    [Fact]
    public async Task EveryReportedMismatchNamesTheTagItIsAbout()
    {
        // Arrange
        var verifier = new LogixConfigurationVerifier(new FakeClient
        {
            ["WrongType"] = DefaultAtomicDeclaredType() with { DataType = AllenBradleyDataType.Real },
        });

        // Act
        var mismatches = await verifier.Verify(
            [DIntPointNamed("WrongType"), DIntPointNamed("Missing")], TestContext.Current.CancellationToken);

        // Assert
        mismatches.Should().AllSatisfy(mismatch => mismatch.MismatchingConfigurations.Should().ContainSingle()
            .Which.Value.Should().Contain(mismatch.DataPoint.TagAddress.Value));
    }

    [Fact]
    public async Task ATagUnderAProgramTheControllerHasNotIsReportedByItsQualifiedAddress()
    {
        // Arrange
        var verifier = new LogixConfigurationVerifier(new FakeClient
        {
            ["Program:MainProgram.Count"] = DefaultAtomicDeclaredType(),
        });
        var noSuchProgram = CreateProgramTagsNode("NoSuchProgram");
        var dataPoints = DataPointsOf(CreateCommunicationOf(
            [noSuchProgram, CreateDIntNode("Speed", "Count", noSuchProgram.Id)]));

        // Act
        var mismatches = await verifier.Verify(dataPoints, TestContext.Current.CancellationToken);

        // Assert
        mismatches.Should().ContainSingle()
            .Which.MismatchingConfigurations.Should().ContainSingle()
            .Which.Value.Should().Be("RootTagName 'Program:NoSuchProgram.Count' was not found on the controller.");
    }

    [Fact]
    public async Task ATagUnderTheProgramThatOwnsItReportsNothing()
    {
        // Arrange
        var verifier = new LogixConfigurationVerifier(new FakeClient
        {
            ["Program:MainProgram.Count"] = DefaultAtomicDeclaredType(),
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
        new(TagPath.Parse(tagName), DefaultPollFrequency, NoChannels);

    private static StringDataPoint StringPointNamed(string tagName) =>
        new(TagPath.Parse(tagName), DefaultPollFrequency, NoChannels, StringMaxLength.Standard);

    private static IntArrayDataPoint IntArrayPointNamed(string tagName) =>
        new(TagPath.Parse(tagName), DefaultPollFrequency, NoChannels, TenElements);

    // A hand-built point could not disagree with the walk about a program prefix.
    private static IReadOnlyList<ILogixDataPoint> DataPointsOf(LogixCommunication communication) =>
        new LogixDataPointsGroupsMapper().ToDataPoints(
            TypedLogixNodeMapper.Instance().MapToTypedNodes(communication));

    private sealed class FakeClient : ILogixClient
    {
        private readonly Dictionary<TagAddress, DeclaredType> _declarations =
            new(TagAddress.CaseInsensitiveComparer);

        public DeclaredType this[string tagName]
        {
            set => _declarations[new TagAddress(tagName)] = value;
        }

        public bool IsConnected => true;

        public Task<IReadOnlyList<ResolvedDataPoint>> ResolveDataPoints(
            IReadOnlyList<ILogixDataPoint> dataPoints, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<ResolvedDataPoint>>(
            [
                .. dataPoints.Select(dataPoint => new ResolvedDataPoint(
                    dataPoint,
                    _declarations.TryGetValue(dataPoint.TagAddress, out var declaration) ? declaration : null)),
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
