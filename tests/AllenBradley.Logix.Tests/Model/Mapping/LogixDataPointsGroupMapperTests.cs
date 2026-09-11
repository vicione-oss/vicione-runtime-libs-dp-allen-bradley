using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.FloatingPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Booleans;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.FloatingPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Strings;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Mapping;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Device;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.TypedNodeTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.Mapping;

public sealed class LogixDataPointsGroupMapperTests
{
    private readonly LogixDataPointsGroupsMapper _mapper = new();

    [Fact]
    public void AConfiguredTagBecomesAPointCarryingItsTagNamePollFrequencyAndChannels()
    {
        // Arrange
        var counter = DefaultDIntNode with { PollFrequency = PollFrequency.FromMilliseconds(250) };
        var deviceNode = DeviceNodeHoldingInControllerScope(counter);

        // Act
        var dataPoints = _mapper.ToDataPoints(deviceNode);

        // Assert
        var expected = new DIntDataPoint(counter.TagName, counter.PollFrequency, counter.Channels);
        dataPoints.Should().ContainSingle().Which.Should().Be(expected);
    }

    [Fact]
    public void ATagInControllerScopeAddressesItselfWithoutAPrefix()
    {
        // Arrange
        var deviceNode = DeviceNodeHoldingInControllerScope(DefaultDIntNode);

        // Act
        var dataPoints = _mapper.ToDataPoints(deviceNode);

        // Assert
        dataPoints.Should().ContainSingle().Which.Identifier.Value.Should().Be(DefaultDIntTagName.Value);
    }

    [Fact]
    public void ATagInAProgramAddressesItselfBehindItsProgram()
    {
        // Arrange
        var deviceNode = DeviceNodeHoldingInProgramScope(DefaultDIntNode);

        // Act
        var dataPoints = _mapper.ToDataPoints(deviceNode);

        // Assert
        var expected = new TagName($"Program:{DefaultProgramName.Value}.{DefaultDIntTagName.Value}");
        dataPoints.Should().ContainSingle().Which.TagName.Should().Be(expected);
    }

    [Fact]
    public void AConfiguredIntTagBecomesAnIntPointCarryingItsTagNamePollFrequencyAndChannels()
    {
        // Arrange
        var setpoint = DefaultIntNode with { PollFrequency = PollFrequency.FromMilliseconds(250) };
        var deviceNode = DeviceNodeHoldingInControllerScope(setpoint);

        // Act
        var dataPoints = _mapper.ToDataPoints(deviceNode);

        // Assert
        var expected = new IntDataPoint(setpoint.TagName, setpoint.PollFrequency, setpoint.Channels);
        dataPoints.Should().ContainSingle().Which.Should().Be(expected);
    }

    [Fact]
    public void AConfiguredSIntTagBecomesASIntPointCarryingItsTagNamePollFrequencyAndChannels()
    {
        // Arrange
        var level = DefaultSIntNode with { PollFrequency = PollFrequency.FromMilliseconds(250) };
        var deviceNode = DeviceNodeHoldingInControllerScope(level);

        // Act
        var dataPoints = _mapper.ToDataPoints(deviceNode);

        // Assert
        var expected = new SIntDataPoint(level.TagName, level.PollFrequency, level.Channels);
        dataPoints.Should().ContainSingle().Which.Should().Be(expected);
    }

    [Fact]
    public void AConfiguredLIntTagBecomesALIntPointCarryingItsTagNamePollFrequencyAndChannels()
    {
        // Arrange
        var ticks = DefaultLIntNode with { PollFrequency = PollFrequency.FromMilliseconds(250) };
        var deviceNode = DeviceNodeHoldingInControllerScope(ticks);

        // Act
        var dataPoints = _mapper.ToDataPoints(deviceNode);

        // Assert
        var expected = new LIntDataPoint(ticks.TagName, ticks.PollFrequency, ticks.Channels);
        dataPoints.Should().ContainSingle().Which.Should().Be(expected);
    }

    [Fact]
    public void AConfiguredUSIntTagBecomesAUSIntPointCarryingItsTagNamePollFrequencyAndChannels()
    {
        // Arrange
        var pressure = DefaultUSIntNode with { PollFrequency = PollFrequency.FromMilliseconds(250) };
        var deviceNode = DeviceNodeHoldingInControllerScope(pressure);

        // Act
        var dataPoints = _mapper.ToDataPoints(deviceNode);

        // Assert
        var expected = new USIntDataPoint(pressure.TagName, pressure.PollFrequency, pressure.Channels);
        dataPoints.Should().ContainSingle().Which.Should().Be(expected);
    }

    [Fact]
    public void AConfiguredUIntTagBecomesAUIntPointCarryingItsTagNamePollFrequencyAndChannels()
    {
        // Arrange
        var revolutions = DefaultUIntNode with { PollFrequency = PollFrequency.FromMilliseconds(250) };
        var deviceNode = DeviceNodeHoldingInControllerScope(revolutions);

        // Act
        var dataPoints = _mapper.ToDataPoints(deviceNode);

        // Assert
        var expected = new UIntDataPoint(
            revolutions.TagName, revolutions.PollFrequency, revolutions.Channels);
        dataPoints.Should().ContainSingle().Which.Should().Be(expected);
    }

    [Fact]
    public void AConfiguredUDIntTagBecomesAUDIntPointCarryingItsTagNamePollFrequencyAndChannels()
    {
        // Arrange
        var runtime = DefaultUDIntNode with { PollFrequency = PollFrequency.FromMilliseconds(250) };
        var deviceNode = DeviceNodeHoldingInControllerScope(runtime);

        // Act
        var dataPoints = _mapper.ToDataPoints(deviceNode);

        // Assert
        var expected = new UDIntDataPoint(runtime.TagName, runtime.PollFrequency, runtime.Channels);
        dataPoints.Should().ContainSingle().Which.Should().Be(expected);
    }

    [Fact]
    public void AConfiguredULIntTagBecomesAULIntPointCarryingItsTagNamePollFrequencyAndChannels()
    {
        // Arrange
        var cycles = DefaultULIntNode with { PollFrequency = PollFrequency.FromMilliseconds(250) };
        var deviceNode = DeviceNodeHoldingInControllerScope(cycles);

        // Act
        var dataPoints = _mapper.ToDataPoints(deviceNode);

        // Assert
        var expected = new ULIntDataPoint(cycles.TagName, cycles.PollFrequency, cycles.Channels);
        dataPoints.Should().ContainSingle().Which.Should().Be(expected);
    }

    [Fact]
    public void AConfiguredRealTagBecomesARealPointCarryingItsTagNamePollFrequencyAndChannels()
    {
        // Arrange
        var flowRate = DefaultRealNode with { PollFrequency = PollFrequency.FromMilliseconds(250) };
        var deviceNode = DeviceNodeHoldingInControllerScope(flowRate);

        // Act
        var dataPoints = _mapper.ToDataPoints(deviceNode);

        // Assert
        var expected = new RealDataPoint(flowRate.TagName, flowRate.PollFrequency, flowRate.Channels);
        dataPoints.Should().ContainSingle().Which.Should().Be(expected);
    }

    [Fact]
    public void AConfiguredBoolTagBecomesABoolPointCarryingItsTagNamePollFrequencyAndChannels()
    {
        // Arrange
        var running = DefaultBoolNode with { PollFrequency = PollFrequency.FromMilliseconds(250) };
        var deviceNode = DeviceNodeHoldingInControllerScope(running);

        // Act
        var dataPoints = _mapper.ToDataPoints(deviceNode);

        // Assert
        var expected = new BoolDataPoint(running.TagName, running.PollFrequency, running.Channels);
        dataPoints.Should().ContainSingle().Which.Should().Be(expected);
    }

    [Fact]
    public void AConfiguredStringTagBecomesAPointCarryingTheCapacityItWasDeclaredWith()
    {
        // Arrange
        var label = DefaultStringNode with
        {
            PollFrequency = PollFrequency.FromMilliseconds(500),
            MaxLength = new StringMaxLength(20),
        };
        var deviceNode = DeviceNodeHoldingInControllerScope(label);

        // Act
        var dataPoints = _mapper.ToDataPoints(deviceNode);

        // Assert
        var expected = new StringDataPoint(
            label.TagName, label.PollFrequency, label.Channels, label.MaxLength);
        dataPoints.Should().ContainSingle().Which.Should().Be(expected);
    }

    [Fact]
    public void AConfiguredIntArrayTagBecomesAPointCarryingTheCountItWasDeclaredWith()
    {
        // Arrange
        var readings = DefaultIntArrayNode with
        {
            PollFrequency = PollFrequency.FromMilliseconds(500),
            ElementCount = new ElementCount(20),
        };
        var deviceNode = DeviceNodeHoldingInControllerScope(readings);

        // Act
        var dataPoints = _mapper.ToDataPoints(deviceNode);

        // Assert
        var expected = new IntArrayDataPoint(
            readings.TagName, readings.PollFrequency, readings.Channels, readings.ElementCount);
        dataPoints.Should().ContainSingle().Which.Should().Be(expected);
    }

    [Fact]
    public void EachTagBecomesThePointItsOwnTypeMapsTo()
    {
        // Arrange
        var deviceNode = DeviceNodeHoldingInControllerScope(
            DefaultDIntNode, DefaultIntNode, DefaultStringNode, DefaultIntArrayNode, DefaultSIntArrayNode,
            DefaultDIntArrayNode, DefaultLIntArrayNode, DefaultUSIntArrayNode, DefaultUIntArrayNode,
            DefaultUDIntArrayNode, DefaultULIntArrayNode, DefaultRealArrayNode);

        // Act
        var dataPoints = _mapper.ToDataPoints(deviceNode);

        // Assert
        dataPoints.Should().SatisfyRespectively(
            static dataPoint => dataPoint.Should().BeOfType<DIntDataPoint>(),
            static dataPoint => dataPoint.Should().BeOfType<IntDataPoint>(),
            static dataPoint => dataPoint.Should().BeOfType<StringDataPoint>(),
            static dataPoint => dataPoint.Should().BeOfType<IntArrayDataPoint>(),
            static dataPoint => dataPoint.Should().BeOfType<SIntArrayDataPoint>(),
            static dataPoint => dataPoint.Should().BeOfType<DIntArrayDataPoint>(),
            static dataPoint => dataPoint.Should().BeOfType<LIntArrayDataPoint>(),
            static dataPoint => dataPoint.Should().BeOfType<USIntArrayDataPoint>(),
            static dataPoint => dataPoint.Should().BeOfType<UIntArrayDataPoint>(),
            static dataPoint => dataPoint.Should().BeOfType<UDIntArrayDataPoint>(),
            static dataPoint => dataPoint.Should().BeOfType<ULIntArrayDataPoint>(),
            static dataPoint => dataPoint.Should().BeOfType<RealArrayDataPoint>());
    }

    [Fact]
    public void EveryConfiguredTagBecomesOnePointOfItsOwn()
    {
        // Arrange
        var deviceNode = DeviceNodeHoldingInControllerScope(
            DefaultDIntNode with { TagName = new TagName("TagOne") },
            DefaultDIntNode with { TagName = new TagName("TagTwo") },
            DefaultDIntNode with { TagName = new TagName("TagThree") });

        // Act
        var dataPoints = _mapper.ToDataPoints(deviceNode);

        // Assert
        dataPoints.Select(static dataPoint => dataPoint.TagName.Value).Should()
            .BeEquivalentTo("TagOne", "TagTwo", "TagThree");
    }

    [Fact]
    public void AControllerWithNoTagsConfiguredHasNoPoints()
    {
        // Arrange
        var deviceNode = DefaultDeviceNode;

        // Act
        var dataPoints = _mapper.ToDataPoints(deviceNode);

        // Assert
        dataPoints.Should().BeEmpty();
    }

    [Fact]
    public void AGroupHoldsThePointsAtTheFrequencyItWasGiven()
    {
        // Arrange
        var pollFrequency = PollFrequency.FromMilliseconds(250);
        IReadOnlyList<ILogixDataPoint> dataPoints =
        [
            new DIntDataPoint(DefaultDIntTagName, DefaultPollFrequency, NoChannels),
        ];

        // Act
        var group = _mapper.CreateGroup(pollFrequency, dataPoints);

        // Assert
        group.PollFrequency.Should().Be(pollFrequency);
        group.DataPoints.Should().BeSameAs(dataPoints);
    }

    private static DeviceNode DeviceNodeHoldingInControllerScope(params IDataPointNode[] dataPointNodes) =>
        DeviceNodeHolding(DefaultControllerTagsNode, dataPointNodes);

    private static DeviceNode DeviceNodeHoldingInProgramScope(params IDataPointNode[] dataPointNodes) =>
        DeviceNodeHolding(DefaultProgramTagsNode, dataPointNodes);

    private static DeviceNode DeviceNodeHolding(ITagScopeNode tagScope, IDataPointNode[] dataPointNodes)
    {
        var deviceNode = DefaultDeviceNode;
        deviceNode.ConfigurationNodes.Add(tagScope);
        tagScope.DataPointNodes.AddRange(dataPointNodes);

        return deviceNode;
    }
}
