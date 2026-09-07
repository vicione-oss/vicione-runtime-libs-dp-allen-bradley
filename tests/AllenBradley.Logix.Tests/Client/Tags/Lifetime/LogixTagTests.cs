using NSubstitute;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Access;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Lifetime;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client.Tags.Lifetime;

/// <summary>
/// <see cref="LogixTag"/> is the join: it surfaces the data point and its metadata, and passes
/// every exchange straight through to the inner handle it wraps. These run with no controller — the inner
/// handle is a substitute — because the whole point of the wrapper is that it adds only data, not exchange logic.
/// </summary>
public class LogixTagTests
{
    private static readonly DIntDataPoint Speed = new(new TagName("Motor.Speed"), DefaultPollFrequency, NoChannels);

    private static readonly TagDefinition DintMetadata =
        new(new TagName("Motor.Speed"), LogixTypeKind.Atomic, AllenBradleyDataType.Dint, MaxLength: null, new DimensionCount(0), new ElementCount(1));

    [Fact]
    public void DataPoint_And_Metadata_AreSurfacedAsGiven()
    {
        // Arrange
        var inner = Substitute.For<ILogixTagAccess>();

        // Act
        using var tag = new LogixTag(Speed, DintMetadata, inner);

        // Assert
        tag.DataPoint.Should().BeSameAs(Speed);
        tag.Metadata.Should().Be(DintMetadata);
    }

    [Fact]
    public void Metadata_IsNull_WhenTheTagIsAbsentFromTheController()
    {
        // Arrange
        var inner = Substitute.For<ILogixTagAccess>();

        // Act
        using var tag = new LogixTag(Speed, Metadata: null, inner);

        // Assert
        tag.Metadata.Should().BeNull();
    }

    [Fact]
    public async Task ReadAsync_DelegatesToTheInnerAccess()
    {
        // Arrange
        var inner = Substitute.For<ILogixTagAccess>();
        inner.ReadAsync(Arg.Any<CancellationToken>())
            .Returns(LogixTagReadResult.Ok(new byte[] { 42, 0, 0, 0 }));
        using var tag = new LogixTag(Speed, DintMetadata, inner);

        // Act
        var result = await tag.ReadAsync(TestContext.Current.CancellationToken);

        // Assert
        await inner.Received(1).ReadAsync(Arg.Any<CancellationToken>());
        result.Buffer.ToArray().Should().Equal(42, 0, 0, 0);
    }

    [Fact]
    public async Task WriteAsync_DelegatesTheBufferToTheInnerAccess()
    {
        // Arrange
        var inner = Substitute.For<ILogixTagAccess>();
        using var tag = new LogixTag(Speed, DintMetadata, inner);

        // Act
        await tag.WriteAsync([1, 2, 3, 4], TestContext.Current.CancellationToken);

        // Assert
        await inner.Received(1).WriteAsync(
            Arg.Is<byte[]>(buffer => buffer.SequenceEqual(new byte[] { 1, 2, 3, 4 })),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Dispose_DisposesTheInnerHandleOnce()
    {
        // Arrange
        var inner = Substitute.For<ILogixTagAccess>();
        var tag = new LogixTag(Speed, DintMetadata, inner);

        // Act
        tag.Dispose();

        // Assert
        // A libplctag handle left unfreed fail-fasts the process (0xC0000602); the wrapper must not swallow
        // the owner's dispose.
        inner.Received(1).Dispose();
    }
}
