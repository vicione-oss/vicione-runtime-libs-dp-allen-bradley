using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Mapping;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.Scope.ProgramTags;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.TypedNodeTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.Mapping;

public sealed class ContainerPathTests
{
    private static readonly ContainerPathSegment.Program MainProgram = new(new ProgramName("MainProgram"));
    private static readonly ContainerPathSegment.Udt Motor = new(new TagName("Motor"));
    private static readonly ContainerPathSegment.Udt Ramp = new(new TagName("Ramp"));
    private static readonly ContainerPathSegment.ArrayContainer Readings = new(new TagName("Readings"));
    private static readonly ContainerPathSegment.DataPoint Speed = new(new TagName("Speed"));
    private static readonly ContainerPathSegment.ArrayElement Third = new(new ElementIndex(3));

    [Fact]
    public void ADataPointUnderControllerScopeIsTheTagItself()
    {
        // Arrange
        var path = ContainerPath.Root.Append(Speed);

        // Act
        var tagPath = path.ToTagPath();

        // Assert
        var expected = new TagPath(Program: null, Speed.TagName, UdtMemberPath: null, ArrayElementIndex: null);
        tagPath.Should().Be(expected);
    }

    [Fact]
    public void ADataPointUnderProgramScopeIsScopedToTheProgram()
    {
        // Arrange
        var path = ContainerPath.Root.Append(MainProgram).Append(Speed);

        // Act
        var tagPath = path.ToTagPath();

        // Assert
        var expected = new TagPath(MainProgram.ProgramName, Speed.TagName, UdtMemberPath: null, ArrayElementIndex: null);
        tagPath.Should().Be(expected);
    }

    [Fact]
    public void EveryNameBehindTheFirstIsAMemberReachedThrough()
    {
        // Arrange
        var path = ContainerPath.Root.Append(Motor).Append(Ramp).Append(Speed);

        // Act
        var tagPath = path.ToTagPath();

        // Assert
        var expected = new TagPath(
            Program: null,
            Motor.TagName,
            UdtMemberPath.Of(new(Ramp.TagName.Value), new(Speed.TagName.Value)),
            ArrayElementIndex: null);
        tagPath.Should().Be(expected);
    }

    [Fact]
    public void ASubscriptUnderAnArrayContainerIsTheElementOfTheArray()
    {
        // Arrange
        var path = ContainerPath.Root.Append(Readings).Append(Third);

        // Act
        var tagPath = path.ToTagPath();

        // Assert
        var expected = new TagPath(Program: null, Readings.TagName, UdtMemberPath: null, Third.Index);
        tagPath.Should().Be(expected);
    }

    [Fact]
    public void ASubscriptBehindAMemberIsTheElementOfThatMember()
    {
        // Arrange
        var path = ContainerPath.Root.Append(Motor).Append(Readings).Append(Third);

        // Act
        var tagPath = path.ToTagPath();

        // Assert
        var expected = new TagPath(
            Program: null, Motor.TagName, UdtMemberPath.Of((UdtMemberName)new(Readings.TagName.Value)), Third.Index);
        tagPath.Should().Be(expected);
    }

    [Fact]
    public void APathThroughNoTagIsRefused()
    {
        // Arrange
        var path = ContainerPath.Root.Append(MainProgram);

        // Act
        var reading = path.Invoking(p => p.ToTagPath());

        // Assert
        reading.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AProgramBehindTheTagIsRefused()
    {
        // Arrange
        var path = ContainerPath.Root.Append(Motor).Append(MainProgram).Append(Speed);

        // Act
        var reading = path.Invoking(p => p.ToTagPath());

        // Assert
        reading.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ANameBehindASubscriptIsRefused()
    {
        // Arrange
        var path = ContainerPath.Root.Append(Readings).Append(Third).Append(Speed);

        // Act
        var reading = path.Invoking(p => p.ToTagPath());

        // Assert
        reading.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ASubscriptBeforeAnyTagIsRefused()
    {
        // Arrange
        var path = ContainerPath.Root.Append(Third);

        // Act
        var reading = path.Invoking(p => p.ToTagPath());

        // Assert
        reading.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void PassingThroughANextSegmentLeavesThePathItCameFromAsItWas()
    {
        // Arrange
        var path = ContainerPath.Root.Append(Motor);

        // Act
        _ = path.Append(Speed);

        // Assert
        path.Segments.Should().Equal(Motor);
    }

    [Fact]
    public void ControllerScopeContributesNoSegment()
    {
        // Arrange

        // Act
        var path = ContainerPath.Root.Append(DefaultControllerTagsNode);

        // Assert
        path.Segments.Should().BeEmpty();
    }

    [Fact]
    public void AProgramScopeContributesItsProgram()
    {
        // Arrange
        var scope = DefaultProgramTagsNode;

        // Act
        var path = ContainerPath.Root.Append(scope);

        // Assert
        path.Segments.Should().Equal(new ContainerPathSegment.Program(scope.ProgramName));
    }

    [Fact]
    public void AnArrayContainerContributesItsTagName()
    {
        // Arrange
        var container = DefaultIntArrayContainerNode;

        // Act
        var path = ContainerPath.Root.Append(container);

        // Assert
        path.Segments.Should().Equal(new ContainerPathSegment.ArrayContainer(container.TagName));
    }

    [Fact]
    public void AUdtContainerContributesItsTagName()
    {
        // Arrange
        var container = DefaultUdtContainerNode;

        // Act
        var path = ContainerPath.Root.Append(container);

        // Assert
        path.Segments.Should().Equal(new ContainerPathSegment.Udt(container.TagName));
    }

    [Fact]
    public void AContainerTheWalkDoesNotKnowIsRefused()
    {
        // Arrange
        var container = new UnknownContainerNode();

        // Act
        var appending = ContainerPath.Root.Invoking(root => root.Append(container));

        // Assert
        appending.Should().Throw<NotSupportedException>();
    }

    [Fact]
    public void ADataPointNodeUnderAnArrayContainerContributesItsSubscriptAsTheElement()
    {
        // Arrange
        var thirdReading = DefaultIntNode with { TagName = new TagName("[3]") };

        // Act
        var path = ContainerPath.Root.Append(thirdReading, DefaultIntArrayContainerNode);

        // Assert
        path.Segments.Should().Equal(new ContainerPathSegment.ArrayElement(new ElementIndex(3)));
    }

    [Fact]
    public void ADataPointNodeUnderAnyOtherContainerContributesItsTagName()
    {
        // Arrange
        var tagNode = DefaultIntNode;

        // Act
        var path = ContainerPath.Root.Append(tagNode, DefaultUdtContainerNode);

        // Assert
        path.Segments.Should().Equal(new ContainerPathSegment.DataPoint(tagNode.TagName));
    }

    private sealed class UnknownContainerNode : ILogixContainerNode
    {
        public LinkedNode OriginalNode { get; init; } = DummyOriginalNode;
        public IConfigurationNode? ParentConfigurationNode { get; set; }
        public List<IConfigurationNode> ConfigurationNodes { get; } = [];
        public List<IDataPointNode> DataPointNodes { get; } = [];
        public bool CanBeAdded(IConfigurationNode configurationNode) => false;
        public bool CanBeAdded(IDataPointNode dataPointNode) => false;
    }
}
