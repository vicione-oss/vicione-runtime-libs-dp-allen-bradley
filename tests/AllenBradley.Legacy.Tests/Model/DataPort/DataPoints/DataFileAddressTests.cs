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

    /// <summary>Each long element with the address RSLogix 500 shows for it.</summary>
    public static TheoryData<DataFileAddress, string> LongElements =>
        new()
        {
            { new DataFileAddress(DataFileType.Long, new FileNumber(9), new ElementNumber(0)), "L9:0" },
            { new DataFileAddress(DataFileType.Long, new FileNumber(255), new ElementNumber(255)), "L255:255" },
        };

    [Theory]
    [MemberData(nameof(LongElements))]
    public void ALongElementIsWrittenTheWayRsLogixWritesIt(DataFileAddress address, string expectedText)
    {
        // Arrange

        // Act
        var text = address.ToString();

        // Assert
        text.Should().Be(expectedText);
    }

    /// <summary>Each float element with the address RSLogix 500 shows for it.</summary>
    public static TheoryData<DataFileAddress, string> FloatElements =>
        new()
        {
            { new DataFileAddress(DataFileType.Float, new FileNumber(8), new ElementNumber(12)), "F8:12" },
            { new DataFileAddress(DataFileType.Float, new FileNumber(255), new ElementNumber(255)), "F255:255" },
        };

    [Theory]
    [MemberData(nameof(FloatElements))]
    public void AFloatElementIsWrittenTheWayRsLogixWritesIt(DataFileAddress address, string expectedText)
    {
        // Arrange

        // Act
        var text = address.ToString();

        // Assert
        text.Should().Be(expectedText);
    }

    /// <summary>Each bit of a binary element with the address RSLogix 500 shows for it.</summary>
    public static TheoryData<DataFileAddress, string> BinaryBits =>
        new()
        {
            { new DataFileAddress(DataFileType.Binary, new FileNumber(3), new ElementNumber(2), new BitNumber(0)), "B3:2/0" },
            { new DataFileAddress(DataFileType.Binary, new FileNumber(255), new ElementNumber(255), new BitNumber(15)), "B255:255/15" },
        };

    [Theory]
    [MemberData(nameof(BinaryBits))]
    public void ABitIsWrittenTheWayRsLogixWritesIt(DataFileAddress address, string expectedText)
    {
        // Arrange

        // Act
        var text = address.ToString();

        // Assert
        text.Should().Be(expectedText);
    }
}
