using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.Model.DataPort.DataPoints;

public sealed class DataFileAddressTests
{
    /// <summary>Each integer element with the address RSLogix 500 shows for it.</summary>
    public static TheoryData<DataFileAddress, string> IntegerElements =>
        new()
        {
            { new DataFileAddress(DataFileType.Integer, new FileNumber(7), new ElementNumber(0)), "N7:0" },
            { new DataFileAddress(DataFileType.Integer, new FileNumber(10), new ElementNumber(255)), "N10:255" },
        };

    [Theory]
    [MemberData(nameof(IntegerElements))]
    public void AnIntegerElementIsWrittenTheWayRsLogixWritesIt(DataFileAddress address, string expectedText)
    {
        // Arrange

        // Act
        var text = address.ToString();

        // Assert
        text.Should().Be(expectedText);
    }
}
