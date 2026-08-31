using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Strings;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Mapping;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Device;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Mapper;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixCommunicationTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.Mapping;

/// <summary>
/// The step between a configured tree and the points both ports run on. Every case here maps a real
/// communication through <see cref="TypedLogixNodeMapper"/> first, because the tree the mapper walks is
/// the one the engine's configuration produces and a hand-built tree could not disagree with it.
/// </summary>
public sealed class LogixDataPointsGroupMapperTests
{
    private readonly LogixDataPointsGroupsMapper _mapper = new();

    [Fact]
    public void ToDataPoints_ConfiguredTag_CarriesTagNamePollFrequencyAndChannels()
    {
        // Arrange
        var deviceNode = MapTree(CreateCommunication([CreateDIntNode("Speed", "MotorSpeed", 250)]));

        // Act
        var dataPoints = _mapper.ToDataPoints(deviceNode);

        // Assert
        dataPoints.Should().ContainSingle()
            .Which.Should().BeOfType<DIntDataPoint>()
            .Which.Should().BeEquivalentTo(new
            {
                TagName = new TagName("MotorSpeed"),
                PollFrequency = PollFrequency.FromMilliseconds(250),
            });
        dataPoints[0].Channels.Transferred.Should()
            .ContainSingle().Which.Value.Should().Be("Speed");
    }

    /// <remarks>
    /// Controller scope contributes no segment to a tag address, so the point's identifier is the tag
    /// name as configured. This is the assertion that fails first if program scope is ever wired up
    /// without teaching the walk to prefix.
    /// </remarks>
    [Fact]
    public void ToDataPoints_ControllerScope_AddsNoAddressPrefix()
    {
        // Arrange
        var deviceNode = MapTree(CommunicationWithSingleDInt("Speed", "MotorSpeed"));

        // Act
        var dataPoints = _mapper.ToDataPoints(deviceNode);

        // Assert
        dataPoints[0].Identifier.Value.Should().Be("MotorSpeed");
    }

    /// <remarks>
    /// The configured tag name stays bare — <c>Count</c>, not <c>Program:MainProgram.Count</c>. The prefix
    /// is the container's, composed here, which is what keeps a leaf ignorant of its scope.
    /// </remarks>
    [Fact]
    public void ToDataPoints_ProgramScope_PrefixesTheAddressWithTheProgram()
    {
        // Arrange
        var deviceNode = MapTree(CreateCommunicationOf(
        [
            CreateProgramTagsNode("MainProgram", ProgramTagsId),
            CreateDIntNode("Speed", "Count", parentId: ProgramTagsId),
        ]));

        // Act
        var dataPoints = _mapper.ToDataPoints(deviceNode);

        // Assert
        dataPoints.Should().ContainSingle()
            .Which.TagName.Value.Should().Be("Program:MainProgram.Count");
    }

    /// <remarks>
    /// The scopes are peers under the device, so the walk has to reach the right prefix per branch rather
    /// than once for the tree. Two programs also prove the segment is the container's own and not the
    /// first one the walk met.
    /// </remarks>
    [Fact]
    public void ToDataPoints_BothScopesConfigured_PrefixesOnlyTheProgramTags()
    {
        // Arrange
        var deviceNode = MapTree(CreateCommunicationOf(
        [
            .. WrapInControllerTags([CreateDIntNode("Speed", "MotorSpeed")], DeviceDesignId),
            CreateProgramTagsNode("MainProgram", ProgramTagsId),
            CreateDIntNode("Count", "Count", parentId: ProgramTagsId),
        ]));

        // Act
        var dataPoints = _mapper.ToDataPoints(deviceNode);

        // Assert
        dataPoints.Select(static dataPoint => dataPoint.TagName.Value).Should()
            .BeEquivalentTo("MotorSpeed", "Program:MainProgram.Count");
    }

    /// <remarks>
    /// The capacity is the half of a string node that has nowhere else to come from: the tag name and poll
    /// frequency are the scalar pair every type carries, but <c>MaxLength</c> is configuration the manifest
    /// supplies and the converter sizes its buffer from.
    /// </remarks>
    [Fact]
    public void ToDataPoints_ConfiguredStringTag_CarriesTagNamePollFrequencyAndMaxLength()
    {
        // Arrange
        var deviceNode = MapTree(CreateCommunication([CreateStringNode("Label", "MotorLabel", 20, 500)]));

        // Act
        var dataPoints = _mapper.ToDataPoints(deviceNode);

        // Assert
        dataPoints.Should().ContainSingle()
            .Which.Should().BeOfType<StringDataPoint>()
            .Which.Should().BeEquivalentTo(new
            {
                TagName = new TagName("MotorLabel"),
                PollFrequency = PollFrequency.FromMilliseconds(500),
                MaxLength = new StringMaxLength(20),
            });
        dataPoints[0].Channels.Transferred.Should()
            .ContainSingle().Which.Value.Should().Be("Label");
    }

    [Fact]
    public void ToDataPoints_TagsOfDifferentTypes_ReturnsThePointEachTypeMapsTo()
    {
        // Arrange
        var deviceNode = MapTree(CreateCommunication(
        [
            CreateDIntNode("Speed", "MotorSpeed"),
            CreateStringNode("Label", "MotorLabel"),
        ]));

        // Act
        var dataPoints = _mapper.ToDataPoints(deviceNode);

        // Assert
        dataPoints.Should().SatisfyRespectively(
            static dataPoint => dataPoint.Should().BeOfType<DIntDataPoint>(),
            static dataPoint => dataPoint.Should().BeOfType<StringDataPoint>());
    }

    [Fact]
    public void ToDataPoints_SeveralTags_ReturnsOnePointEach()
    {
        // Arrange
        var deviceNode = MapTree(CreateCommunication(
        [
            CreateDIntNode("First", "TagOne"),
            CreateDIntNode("Second", "TagTwo"),
            CreateDIntNode("Third", "TagThree"),
        ]));

        // Act
        var dataPoints = _mapper.ToDataPoints(deviceNode);

        // Assert
        dataPoints.Select(static dataPoint => dataPoint.TagName.Value).Should()
            .BeEquivalentTo("TagOne", "TagTwo", "TagThree");
    }

    [Fact]
    public void ToDataPoints_DeviceWithoutTags_ReturnsEmpty()
    {
        // Arrange
        var deviceNode = MapTree(CreateCommunication());

        // Act
        var dataPoints = _mapper.ToDataPoints(deviceNode);

        // Assert
        dataPoints.Should().BeEmpty();
    }

    [Fact]
    public void CreateGroup_HoldsThePointsAtTheFrequencyItWasGiven()
    {
        // Arrange
        var pollFrequency = PollFrequency.FromMilliseconds(250);
        var deviceNode = MapTree(CommunicationWithSingleDInt("Speed"));
        var dataPoints = _mapper.ToDataPoints(deviceNode);

        // Act
        var group = _mapper.CreateGroup(pollFrequency, dataPoints);

        // Assert
        group.PollFrequency.Should().Be(pollFrequency);
        group.DataPoints.Should().BeSameAs(dataPoints);
    }

    private static DeviceNode MapTree(LogixCommunication communication) =>
        TypedLogixNodeMapper.Instance().MapToTypedNodes(communication);
}
