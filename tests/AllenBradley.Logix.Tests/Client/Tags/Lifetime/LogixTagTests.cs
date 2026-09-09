using NSubstitute;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Access;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Lifetime;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.TagDefinitionTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client.Tags.Lifetime;

public sealed class LogixTagTests
{
    private static readonly DIntDataPoint Speed = new(new TagName("Motor.Speed"), DefaultPollFrequency, NoChannels);

    private static readonly TagDefinition SpeedDefinition =
        DefaultAtomicTagDefinition() with { TagName = Speed.TagName };

    private readonly ILogixTagAccess _handle = Substitute.For<ILogixTagAccess>();

    [Fact]
    public void ATagSurfacesTheDataPointAndDefinitionItWasBuiltWith()
    {
        // Arrange

        // Act
        using var tag = new LogixTag(Speed, SpeedDefinition, _handle);

        // Assert
        tag.DataPoint.Should().Be(Speed);
        tag.Metadata.Should().Be(SpeedDefinition);
    }

    [Fact]
    public void ATagAbsentFromTheControllerCarriesNoDefinition()
    {
        // Arrange

        // Act
        using var tag = new LogixTag(Speed, Metadata: null, _handle);

        // Assert
        tag.Metadata.Should().BeNull();
    }

    [Fact]
    public async Task AReadHandsBackTheBytesTheHandleAnswered()
    {
        // Arrange
        _handle.ReadAsync(Arg.Any<CancellationToken>()).Returns(LogixTagReadResult.Ok(new byte[] { 42, 0, 0, 0 }));
        using var tag = new LogixTag(Speed, SpeedDefinition, _handle);

        // Act
        var read = await tag.ReadAsync(CancellationToken.None);

        // Assert
        read.Buffer.ToArray().Should().Equal(42, 0, 0, 0);
    }

    [Fact]
    public async Task AWriteHandsTheHandleTheBufferItWasGiven()
    {
        // Arrange
        using var tag = new LogixTag(Speed, SpeedDefinition, _handle);

        // Act
        await tag.WriteAsync([1, 2, 3, 4], CancellationToken.None);

        // Assert
        await _handle.Received(1).WriteAsync(
            Arg.Is<byte[]>(buffer => buffer.SequenceEqual(new byte[] { 1, 2, 3, 4 })),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public void DisposingATagFreesTheHandleItWraps()
    {
        // Arrange
        var tag = new LogixTag(Speed, SpeedDefinition, _handle);

        // Act
        tag.Dispose();

        // Assert
        // A libplctag handle left unfreed fail-fasts the process (0xC0000602).
        _handle.Received(1).Dispose();
    }
}
