using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.Scope.ProgramTags;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.DataPort.DataPoints;

public sealed class TagPathTests
{
    private static readonly TagPath MyValuesAtIndexThree =
        new(new ProgramName("Main"), new TagName("MyValues"), UdtMemberPath: null, new ElementIndex(3));

    private static readonly TagPath RampTargetOfMotorInMain = new(
        new ProgramName("Main"),
        new TagName("Motor"),
        UdtMemberPath.Of(new UdtMemberName("Ramp"), new UdtMemberName("Target")),
        ArrayElementIndex: null);

    [Fact]
    public void AControllerScopedTagAddressesItselfByName()
    {
        // Arrange
        var path = new TagPath(Program: null, new TagName("Count"), UdtMemberPath: null, ArrayElementIndex: null);

        // Act
        var tagAddress = path.ToTagAddress();

        // Assert
        tagAddress.Should().Be(new TagAddress("Count"));
    }

    [Fact]
    public void AProgramScopedElementAddressesItselfBehindItsProgramAndArray()
    {
        // Arrange

        // Act
        var tagAddress = MyValuesAtIndexThree.ToTagAddress();

        // Assert
        tagAddress.Should().Be(new TagAddress("Program:Main.MyValues[3]"));
    }

    [Fact]
    public void AMemberAddressesItselfBehindEveryMemberAboveIt()
    {
        // Arrange

        // Act
        var tagAddress = RampTargetOfMotorInMain.ToTagAddress();

        // Assert
        tagAddress.Should().Be(new TagAddress("Program:Main.Motor.Ramp.Target"));
    }

    [Fact]
    public void AnElementIsDeclaredAsItsArray()
    {
        // Arrange

        // Act
        var declaredTagAddress = MyValuesAtIndexThree.RootTagAddress;

        // Assert
        declaredTagAddress.Should().Be(new TagAddress("Program:Main.MyValues"));
    }

    [Fact]
    public void ATagThatIsNoElementIsDeclaredAsItself()
    {
        // Arrange
        var path = MyValuesAtIndexThree with { ArrayElementIndex = null };

        // Act
        var declaredTagAddress = path.RootTagAddress;

        // Assert
        declaredTagAddress.Should().Be(path.ToTagAddress());
    }

    [Fact]
    public void AMemberIsDeclaredAsItsTag()
    {
        // Arrange

        // Act
        var declaredTagAddress = RampTargetOfMotorInMain.RootTagAddress;

        // Assert
        declaredTagAddress.Should().Be(new TagAddress("Program:Main.Motor"));
    }

    [Theory]
    [InlineData("Count")]
    [InlineData("Program:Main.Count")]
    [InlineData("MyValues[3]")]
    [InlineData("Program:Main.MyValues[3]")]
    [InlineData("Motor.Speed")]
    [InlineData("Program:Main.Motor.Ramp.Target")]
    [InlineData("Motor.MyValues[3]")]
    public void AnAddressParsesBackIntoThePathThatRendersIt(string address)
    {
        // Arrange

        // Act
        var path = TagPath.Parse(address);

        // Assert
        path.ToTagAddress().Should().Be(new TagAddress(address));
    }

    [Fact]
    public void ParsingFillsEveryPartOfAnElementAddress()
    {
        // Arrange

        // Act
        var path = TagPath.Parse("Program:Main.MyValues[3]");

        // Assert
        path.Should().Be(MyValuesAtIndexThree);
    }

    [Fact]
    public void ParsingSplitsADottedAddressIntoTheTagAndItsMembers()
    {
        // Arrange

        // Act
        var path = TagPath.Parse("Program:Main.Motor.Ramp.Target");

        // Assert
        path.Should().Be(RampTargetOfMotorInMain);
    }

    [Fact]
    public void TwoPathsToTheSameMemberAreEqual()
    {
        // Arrange
        var parsed = TagPath.Parse("Program:Main.Motor.Ramp.Target");

        // Act
        var equal = parsed == RampTargetOfMotorInMain;

        // Assert
        equal.Should().BeTrue();
    }

    [Theory]
    [InlineData("Delay", "Delay.ACC")]
    [InlineData("Program:Main.Motor.Delay", "Program:Main.Motor.Delay.ACC")]
    public void AnAppendedMemberIsReachedBehindTheMembersAlreadyOnThePath(string address, string expectedAddress)
    {
        // Arrange
        var path = TagPath.Parse(address);

        // Act
        var appended = path.AppendMember(new UdtMemberName("ACC"));

        // Assert
        appended.ToTagAddress().Should().Be(new TagAddress(expectedAddress));
    }

    [Theory]
    [InlineData("MyValues[05]", "a leading zero")]
    [InlineData("MyValues[-1]", "a negative index")]
    [InlineData("MyValues[1,2]", "two dimensions")]
    [InlineData("MyValues[5].Value", "a member behind the subscript")]
    [InlineData("Motor..Speed", "an empty member")]
    [InlineData("", "nothing")]
    public void AnAddressThatIsNotATagAMemberOrOneElementIsRefused(string address, string because)
    {
        // Arrange

        // Act
        var parsing = () => TagPath.Parse(address);

        // Assert
        parsing.Should().Throw<FormatException>(because);
    }
}
