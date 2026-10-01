using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.FloatingPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Booleans;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Counters;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.FloatingPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Strings;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Timers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Mapping;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes;
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
    public void ADIntTagBecomesADIntPoint()
    {
        // Arrange
        var counter = DefaultDIntNode with { PollFrequency = PollFrequency.FromMilliseconds(250) };
        var deviceNode = DeviceNodeHoldingInControllerScope(counter);

        // Act
        var dataPoints = _mapper.ToDataPoints(deviceNode);

        // Assert
        var expected = new DIntDataPoint(counter.TagName.ToTagPath(), counter.PollFrequency, counter.Channels);
        dataPoints.Should().ContainSingle().Which.Should().Be(expected);
    }

    [Fact]
    public void AControllerTagIsAddressedByItsName()
    {
        // Arrange
        var deviceNode = DeviceNodeHoldingInControllerScope(DefaultDIntNode);

        // Act
        var dataPoints = _mapper.ToDataPoints(deviceNode);

        // Assert
        dataPoints.Should().ContainSingle().Which.Identifier.Value.Should().Be(DefaultDIntTagName.Value);
    }

    [Fact]
    public void AProgramTagIsAddressedBehindItsProgram()
    {
        // Arrange
        var deviceNode = DeviceNodeHoldingInProgramScope(DefaultDIntNode);

        // Act
        var dataPoints = _mapper.ToDataPoints(deviceNode);

        // Assert
        var expected = new TagAddress($"Program:{DefaultProgramName.Value}.{DefaultDIntTagName.Value}");
        dataPoints.Should().ContainSingle().Which.TagAddress.Should().Be(expected);
    }

    [Fact]
    public void AnIntTagBecomesAnIntPoint()
    {
        // Arrange
        var setpoint = DefaultIntNode with { PollFrequency = PollFrequency.FromMilliseconds(250) };
        var deviceNode = DeviceNodeHoldingInControllerScope(setpoint);

        // Act
        var dataPoints = _mapper.ToDataPoints(deviceNode);

        // Assert
        var expected = new IntDataPoint(setpoint.TagName.ToTagPath(), setpoint.PollFrequency, setpoint.Channels);
        dataPoints.Should().ContainSingle().Which.Should().Be(expected);
    }

    [Fact]
    public void ASIntTagBecomesASIntPoint()
    {
        // Arrange
        var level = DefaultSIntNode with { PollFrequency = PollFrequency.FromMilliseconds(250) };
        var deviceNode = DeviceNodeHoldingInControllerScope(level);

        // Act
        var dataPoints = _mapper.ToDataPoints(deviceNode);

        // Assert
        var expected = new SIntDataPoint(level.TagName.ToTagPath(), level.PollFrequency, level.Channels);
        dataPoints.Should().ContainSingle().Which.Should().Be(expected);
    }

    [Fact]
    public void ALIntTagBecomesALIntPoint()
    {
        // Arrange
        var ticks = DefaultLIntNode with { PollFrequency = PollFrequency.FromMilliseconds(250) };
        var deviceNode = DeviceNodeHoldingInControllerScope(ticks);

        // Act
        var dataPoints = _mapper.ToDataPoints(deviceNode);

        // Assert
        var expected = new LIntDataPoint(ticks.TagName.ToTagPath(), ticks.PollFrequency, ticks.Channels);
        dataPoints.Should().ContainSingle().Which.Should().Be(expected);
    }

    [Fact]
    public void AUSIntTagBecomesAUSIntPoint()
    {
        // Arrange
        var pressure = DefaultUSIntNode with { PollFrequency = PollFrequency.FromMilliseconds(250) };
        var deviceNode = DeviceNodeHoldingInControllerScope(pressure);

        // Act
        var dataPoints = _mapper.ToDataPoints(deviceNode);

        // Assert
        var expected = new USIntDataPoint(pressure.TagName.ToTagPath(), pressure.PollFrequency, pressure.Channels);
        dataPoints.Should().ContainSingle().Which.Should().Be(expected);
    }

    [Fact]
    public void AUIntTagBecomesAUIntPoint()
    {
        // Arrange
        var revolutions = DefaultUIntNode with { PollFrequency = PollFrequency.FromMilliseconds(250) };
        var deviceNode = DeviceNodeHoldingInControllerScope(revolutions);

        // Act
        var dataPoints = _mapper.ToDataPoints(deviceNode);

        // Assert
        var expected = new UIntDataPoint(
            revolutions.TagName.ToTagPath(), revolutions.PollFrequency, revolutions.Channels);
        dataPoints.Should().ContainSingle().Which.Should().Be(expected);
    }

    [Fact]
    public void AUDIntTagBecomesAUDIntPoint()
    {
        // Arrange
        var runtime = DefaultUDIntNode with { PollFrequency = PollFrequency.FromMilliseconds(250) };
        var deviceNode = DeviceNodeHoldingInControllerScope(runtime);

        // Act
        var dataPoints = _mapper.ToDataPoints(deviceNode);

        // Assert
        var expected = new UDIntDataPoint(runtime.TagName.ToTagPath(), runtime.PollFrequency, runtime.Channels);
        dataPoints.Should().ContainSingle().Which.Should().Be(expected);
    }

    [Fact]
    public void AULIntTagBecomesAULIntPoint()
    {
        // Arrange
        var cycles = DefaultULIntNode with { PollFrequency = PollFrequency.FromMilliseconds(250) };
        var deviceNode = DeviceNodeHoldingInControllerScope(cycles);

        // Act
        var dataPoints = _mapper.ToDataPoints(deviceNode);

        // Assert
        var expected = new ULIntDataPoint(cycles.TagName.ToTagPath(), cycles.PollFrequency, cycles.Channels);
        dataPoints.Should().ContainSingle().Which.Should().Be(expected);
    }

    [Fact]
    public void ARealTagBecomesARealPoint()
    {
        // Arrange
        var flowRate = DefaultRealNode with { PollFrequency = PollFrequency.FromMilliseconds(250) };
        var deviceNode = DeviceNodeHoldingInControllerScope(flowRate);

        // Act
        var dataPoints = _mapper.ToDataPoints(deviceNode);

        // Assert
        var expected = new RealDataPoint(flowRate.TagName.ToTagPath(), flowRate.PollFrequency, flowRate.Channels);
        dataPoints.Should().ContainSingle().Which.Should().Be(expected);
    }

    [Fact]
    public void ABoolTagBecomesABoolPoint()
    {
        // Arrange
        var running = DefaultBoolNode with { PollFrequency = PollFrequency.FromMilliseconds(250) };
        var deviceNode = DeviceNodeHoldingInControllerScope(running);

        // Act
        var dataPoints = _mapper.ToDataPoints(deviceNode);

        // Assert
        var expected = new BoolDataPoint(running.TagName.ToTagPath(), running.PollFrequency, running.Channels);
        dataPoints.Should().ContainSingle().Which.Should().Be(expected);
    }

    [Fact]
    public void AStringPointKeepsTheDeclaredMaxLength()
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
            label.TagName.ToTagPath(), label.PollFrequency, label.Channels, label.MaxLength);
        dataPoints.Should().ContainSingle().Which.Should().Be(expected);
    }

    [Fact]
    public void ATimerTagBecomesATimerPoint()
    {
        // Arrange
        var startDelay = DefaultTimerNode with { PollFrequency = PollFrequency.FromMilliseconds(250) };
        var deviceNode = DeviceNodeHoldingInProgramScope(startDelay);

        // Act
        var dataPoints = _mapper.ToDataPoints(deviceNode);

        // Assert
        var expected = new TimerDataPoint(
            new TagPath(DefaultProgramName, startDelay.TagName, UdtMemberPath: null, ArrayElementIndex: null),
            startDelay.PollFrequency,
            startDelay.Channels);
        dataPoints.Should().ContainSingle().Which.Should().Be(expected);
    }

    [Fact]
    public void ACounterTagBecomesACounterPoint()
    {
        // Arrange
        var partCount = DefaultCounterNode with { PollFrequency = PollFrequency.FromMilliseconds(250) };
        var deviceNode = DeviceNodeHoldingInProgramScope(partCount);

        // Act
        var dataPoints = _mapper.ToDataPoints(deviceNode);

        // Assert
        var expected = new CounterDataPoint(
            new TagPath(DefaultProgramName, partCount.TagName, UdtMemberPath: null, ArrayElementIndex: null),
            partCount.PollFrequency,
            partCount.Channels);
        dataPoints.Should().ContainSingle().Which.Should().Be(expected);
    }

    [Fact]
    public void AnArrayPointKeepsTheDeclaredElementCount()
    {
        // Arrange
        var readings = DefaultIntArrayDataPointNode with
        {
            PollFrequency = PollFrequency.FromMilliseconds(500),
            ElementCount = new ElementCount(20),
        };
        var deviceNode = DeviceNodeHoldingInControllerScope(readings);

        // Act
        var dataPoints = _mapper.ToDataPoints(deviceNode);

        // Assert
        var expected = new IntArrayDataPoint(
            readings.TagName.ToTagPath(), readings.PollFrequency, readings.Channels, readings.ElementCount);
        dataPoints.Should().ContainSingle().Which.Should().Be(expected);
    }

    [Fact]
    public void EachTagBecomesThePointOfItsType()
    {
        // Arrange
        var deviceNode = DeviceNodeHoldingInControllerScope(
            DefaultDIntNode, DefaultIntNode, DefaultStringNode, DefaultIntArrayDataPointNode,
            DefaultSIntArrayDataPointNode,
            DefaultDIntArrayDataPointNode, DefaultLIntArrayDataPointNode, DefaultUsIntArrayDataPointNode,
            DefaultUIntArrayDataPointNode,
            DefaultUdIntArrayDataPointNode, DefaultUlIntArrayDataPointNode, DefaultRealArrayDataPointNode,
            DefaultLRealArrayDataPointNode);

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
            static dataPoint => dataPoint.Should().BeOfType<RealArrayDataPoint>(),
            static dataPoint => dataPoint.Should().BeOfType<LRealArrayDataPoint>());
    }

    [Fact]
    public void EachTagBecomesOnePoint()
    {
        // Arrange
        var deviceNode = DeviceNodeHoldingInControllerScope(
            DefaultDIntNode with { TagName = new TagName("TagOne") },
            DefaultDIntNode with { TagName = new TagName("TagTwo") },
            DefaultDIntNode with { TagName = new TagName("TagThree") });

        // Act
        var dataPoints = _mapper.ToDataPoints(deviceNode);

        // Assert
        dataPoints.Select(static dataPoint => dataPoint.TagAddress.Value).Should()
            .BeEquivalentTo("TagOne", "TagTwo", "TagThree");
    }

    [Fact]
    public void AnArrayElementIsAddressedBehindItsArrayAndProgram()
    {
        // Arrange
        var readings = DefaultIntArrayContainerNode;
        var thirdReading = DefaultIntNode with { TagName = new TagName("[3]") };
        readings.DataPointNodes.Add(thirdReading);
        var deviceNode = DeviceNodeHoldingInProgramScope(readings);

        // Act
        var dataPoints = _mapper.ToDataPoints(deviceNode);

        // Assert
        var expected = new TagAddress($"Program:{DefaultProgramName.Value}.{DefaultIntArrayTagName.Value}[3]");
        dataPoints.Should().ContainSingle().Which.TagAddress.Should().Be(expected);
    }

    [Fact]
    public void AnArrayContainerCanHoldSeveralElements()
    {
        // Arrange
        var readings = DefaultIntArrayContainerNode;
        var thirdReading = DefaultIntNode with { TagName = new TagName("[3]") };
        var seventhReading = DefaultIntNode with { TagName = new TagName("[7]") };
        readings.DataPointNodes.AddRange([thirdReading, seventhReading]);
        var deviceNode = DeviceNodeHoldingInProgramScope(readings);

        // Act
        var dataPoints = _mapper.ToDataPoints(deviceNode);

        // Assert
        var arrayAddress = $"Program:{DefaultProgramName.Value}.{DefaultIntArrayTagName.Value}";
        dataPoints.Select(static dataPoint => dataPoint.TagAddress.Value).Should()
            .BeEquivalentTo($"{arrayAddress}[3]", $"{arrayAddress}[7]");
    }

    [Fact]
    public void AScopeGivesThePointsOfItsTagsAndItsContainers()
    {
        // Arrange
        var readings = DefaultIntArrayContainerNode;
        readings.DataPointNodes.Add(DefaultIntNode with { TagName = new TagName("[3]") });
        var motor = DefaultUdtContainerNode;
        motor.DataPointNodes.Add(DefaultIntNode with { TagName = new TagName("Speed") });
        var scope = DefaultControllerTagsNode;
        scope.DataPointNodes.Add(DefaultIntNode);
        scope.ConfigurationNodes.AddRange([readings, motor]);
        var deviceNode = DefaultDeviceNode;
        deviceNode.ConfigurationNodes.Add(scope);

        // Act
        var dataPoints = _mapper.ToDataPoints(deviceNode);

        // Assert
        dataPoints.Select(static dataPoint => dataPoint.TagAddress.Value).Should().BeEquivalentTo(
            DefaultIntTagName.Value, $"{DefaultIntArrayTagName.Value}[3]", $"{DefaultUdtTagName.Value}.Speed");
    }

    [Fact]
    public void AControllerWithoutTagsHasNoPoints()
    {
        // Arrange
        var deviceNode = DefaultDeviceNode;

        // Act
        var dataPoints = _mapper.ToDataPoints(deviceNode);

        // Assert
        dataPoints.Should().BeEmpty();
    }

    [Fact]
    public void AGroupHoldsItsPointsAndItsPollFrequency()
    {
        // Arrange
        var pollFrequency = PollFrequency.FromMilliseconds(250);
        IReadOnlyList<ILogixDataPoint> dataPoints =
        [
            new DIntDataPoint(DefaultDIntTagName.ToTagPath(), DefaultPollFrequency, NoChannels),
        ];

        // Act
        var group = _mapper.CreateGroup(pollFrequency, dataPoints);

        // Assert
        group.PollFrequency.Should().Be(pollFrequency);
        group.DataPoints.Should().BeSameAs(dataPoints);
    }

    [Fact]
    public void AUdtMemberIsAddressedBehindItsTagAndProgram()
    {
        // Arrange
        var motor = DefaultUdtContainerNode;
        var speed = DefaultIntNode with { TagName = new TagName("Speed") };
        motor.DataPointNodes.Add(speed);
        var deviceNode = DeviceNodeHoldingInProgramScope(motor);

        // Act
        var dataPoints = _mapper.ToDataPoints(deviceNode);

        // Assert
        var expected = new TagAddress($"Program:{DefaultProgramName.Value}.{DefaultUdtTagName.Value}.Speed");
        dataPoints.Should().ContainSingle().Which.TagAddress.Should().Be(expected);
    }

    [Fact]
    public void AUdtMemberKeepsTagNameAndMemberNameApart()
    {
        // Arrange
        var motor = DefaultUdtContainerNode;
        var speed = DefaultIntNode with { TagName = new TagName("Speed") };
        motor.DataPointNodes.Add(speed);
        var deviceNode = DeviceNodeHoldingInControllerScope(motor);

        // Act
        var dataPoints = _mapper.ToDataPoints(deviceNode);

        // Assert
        var expected = new TagPath(
            Program: null, DefaultUdtTagName, UdtMemberPath.Of(new UdtMemberName("Speed")), ArrayElementIndex: null);
        dataPoints.Should().ContainSingle().Which.TagPath.Should().Be(expected);
    }

    [Fact]
    public void ANestedUdtMemberIsAddressedThroughEveryContainerAboveIt()
    {
        // Arrange
        var motor = DefaultUdtContainerNode;
        var ramp = DefaultUdtContainerNode with { TagName = new TagName("Ramp") };
        var target = DefaultRealNode with { TagName = new TagName("Target") };
        ramp.DataPointNodes.Add(target);
        motor.ConfigurationNodes.Add(ramp);
        var deviceNode = DeviceNodeHoldingInControllerScope(motor);

        // Act
        var dataPoints = _mapper.ToDataPoints(deviceNode);

        // Assert
        dataPoints.Should().ContainSingle().Which.TagAddress.Should().Be(new TagAddress("Motor.Ramp.Target"));
    }

    [Fact]
    public void AnElementOfAnArrayMemberIsAddressedBehindTheMember()
    {
        // Arrange
        var motor = DefaultUdtContainerNode;
        var readings = DefaultIntArrayContainerNode with { TagName = new TagName("Readings") };
        readings.DataPointNodes.Add(DefaultIntNode with { TagName = new TagName("[3]") });
        motor.ConfigurationNodes.Add(readings);
        var deviceNode = DeviceNodeHoldingInControllerScope(motor);

        // Act
        var dataPoints = _mapper.ToDataPoints(deviceNode);

        // Assert
        var expected = new TagPath(
            Program: null, DefaultUdtTagName, UdtMemberPath.Of(new UdtMemberName("Readings")), new ElementIndex(3));
        dataPoints.Should().ContainSingle().Which.TagPath.Should().Be(expected);
    }

    [Fact]
    public void EachUdtMemberBecomesThePointOfItsType()
    {
        // Arrange
        var motor = DefaultUdtContainerNode;
        motor.DataPointNodes.AddRange(
        [
            DefaultIntNode with { TagName = new TagName("Speed") },
            DefaultRealNode with { TagName = new TagName("Temperature") },
            DefaultStringNode with { TagName = new TagName("Label") },
            DefaultIntArrayDataPointNode with { TagName = new TagName("Readings") },
        ]);
        var deviceNode = DeviceNodeHoldingInControllerScope(motor);

        // Act
        var dataPoints = _mapper.ToDataPoints(deviceNode);

        // Assert
        dataPoints.Should().SatisfyRespectively(
            static dataPoint => dataPoint.Should().BeOfType<IntDataPoint>(),
            static dataPoint => dataPoint.Should().BeOfType<RealDataPoint>(),
            static dataPoint => dataPoint.Should().BeOfType<StringDataPoint>(),
            static dataPoint => dataPoint.Should().BeOfType<IntArrayDataPoint>());
    }

    private static DeviceNode DeviceNodeHoldingInControllerScope(params IDataPointNode[] dataPointNodes) =>
        DeviceNodeHolding(DefaultControllerTagsNode, dataPointNodes);

    private static DeviceNode DeviceNodeHoldingInProgramScope(params IDataPointNode[] dataPointNodes) =>
        DeviceNodeHolding(DefaultProgramTagsNode, dataPointNodes);

    private static DeviceNode DeviceNodeHoldingInProgramScope(ILogixContainerNode childContainer) =>
        DeviceNodeHolding(DefaultProgramTagsNode, childContainer);

    private static DeviceNode DeviceNodeHoldingInControllerScope(ILogixContainerNode childContainer) =>
        DeviceNodeHolding(DefaultControllerTagsNode, childContainer);

    private static DeviceNode DeviceNodeHolding(ILogixContainerNode tagScope, ILogixContainerNode childContainer)
    {
        var deviceNode = DefaultDeviceNode;
        deviceNode.ConfigurationNodes.Add(tagScope);
        tagScope.ConfigurationNodes.Add(childContainer);

        return deviceNode;
    }

    private static DeviceNode DeviceNodeHolding(ILogixContainerNode tagScope, IDataPointNode[] dataPointNodes)
    {
        var deviceNode = DefaultDeviceNode;
        deviceNode.ConfigurationNodes.Add(tagScope);
        tagScope.DataPointNodes.AddRange(dataPointNodes);

        return deviceNode;
    }
}
