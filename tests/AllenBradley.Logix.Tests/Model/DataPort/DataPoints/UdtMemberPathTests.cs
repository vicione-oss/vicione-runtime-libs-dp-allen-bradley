using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.DataPort.DataPoints;

public sealed class UdtMemberPathTests
{
    private static readonly UdtMemberName Ramp = new("Ramp");
    private static readonly UdtMemberName Target = new("Target");

    [Fact]
    public void APathThroughNoMemberIsRefused()
    {
        // Arrange

        // Act
        var building = () => UdtMemberPath.Of();

        // Assert
        building.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AMemberReachedNextComesAfterTheOnesBeforeIt()
    {
        // Arrange
        var path = UdtMemberPath.Of(Ramp);

        // Act
        var extended = path.Append(Target);

        // Assert
        extended.Members.Should().Equal(Ramp, Target);
    }

    [Fact]
    public void ReachingANextMemberLeavesThePathItCameFromAsItWas()
    {
        // Arrange
        var path = UdtMemberPath.Of(Ramp);

        // Act
        _ = path.Append(Target);

        // Assert
        path.Members.Should().Equal(Ramp);
    }

    [Fact]
    public void TwoPathsNamingTheSameMembersAreEqual()
    {
        // Arrange
        var built = UdtMemberPath.Of(Ramp).Append(Target);
        var declared = UdtMemberPath.Of(Ramp, Target);

        // Act
        var equal = built == declared;

        // Assert
        equal.Should().BeTrue();
    }

    [Fact]
    public void TwoPathsNamingTheSameMembersHashAlike()
    {
        // Arrange
        var built = UdtMemberPath.Of(Ramp).Append(Target);
        var declared = UdtMemberPath.Of(Ramp, Target);

        // Act
        var hashesAlike = built.GetHashCode() == declared.GetHashCode();

        // Assert
        hashesAlike.Should().BeTrue();
    }

    [Fact]
    public void TwoPathsNamingTheSameMembersInAnotherOrderDiffer()
    {
        // Arrange
        var rampTarget = UdtMemberPath.Of(Ramp, Target);
        var targetRamp = UdtMemberPath.Of(Target, Ramp);

        // Act
        var equal = rampTarget == targetRamp;

        // Assert
        equal.Should().BeFalse();
    }

    [Fact]
    public void APathRendersItsMembersJoinedByDots()
    {
        // Arrange
        var path = UdtMemberPath.Of(Ramp, Target);

        // Act
        var rendered = path.ToString();

        // Assert
        rendered.Should().Be("Ramp.Target");
    }
}
