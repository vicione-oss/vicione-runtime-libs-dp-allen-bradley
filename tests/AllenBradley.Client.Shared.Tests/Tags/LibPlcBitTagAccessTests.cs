using libplctag;
using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.Tags;

namespace ViciOne.Suite.DataPort.AllenBradley.Client.Shared.Tests.Tags;

/// <summary>
/// The access is never connected, so only the checks before the first libplctag call can be tested here.
/// Disposing it is not optional: under MTP a tag handle left to its finalizer crashes the test process.
/// </summary>
public sealed class LibPlcBitTagAccessTests : IDisposable
{
    private readonly LibPlcBitTagAccess _access = new(new Tag { Name = "B3:2/0" });

    public void Dispose() => _access.Dispose();

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    public async Task ABufferThatIsNotOneByteIsNotWritten(int bufferLength)
    {
        // Arrange
        var buffer = new byte[bufferLength];

        // Act
        var write = await _access.WriteAsync(buffer, CancellationToken.None);

        // Assert
        write.Succeeded.Should().BeFalse();
    }
}
