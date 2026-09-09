using libplctag;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.LibPlcTag;

/// <summary>
/// libplctag's own round trip against the device, with none of this addon in the way — the baseline a
/// failure elsewhere is read against. The suite writes <c>strValue1</c>; that is what the tag is for, so
/// nothing is restored.
/// </summary>
public sealed class PlcConnectivityTests : LibPlcTagIntegrationTestBase
{
    [Fact]
    public void AStringWrittenWithTheSynchronousApiReadsBackUnchanged()
    {
        // Arrange
        // The first read is what initialises the tag's metadata; a write before it has nothing to size.
        using var tag = BenchController.RawTagFor(BenchControllerTags.StrValue1);
        tag.Read();
        var valueToWrite = $"TEST_{DateTime.UtcNow:HHmmss}";

        // Act
        tag.SetString(0, valueToWrite);
        tag.Write();
        tag.Read();

        // Assert
        tag.GetString(0).Should().Be(valueToWrite);
    }

    [Fact]
    public async Task AStringWrittenWithTheAsynchronousApiReadsBackUnchanged()
    {
        // Arrange
        using var tag = BenchController.RawTagFor(BenchControllerTags.StrValue1);
        await tag.ReadAsync();
        var valueToWrite = $"TEST_{DateTime.UtcNow:HHmmss}";

        // Act
        tag.SetString(0, valueToWrite);
        await tag.WriteAsync();
        await tag.ReadAsync();

        // Assert
        tag.GetString(0).Should().Be(valueToWrite);
    }
}
