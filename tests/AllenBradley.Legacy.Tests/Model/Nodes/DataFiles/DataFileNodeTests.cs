using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataFiles.AsciiFile;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataFiles.BinaryFile;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataFiles.FloatFile;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataFiles.IntegerFile;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataFiles.LongFile;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataFiles.StringFile;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataPoints.Binary.Bit;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataPoints.Floats.Float;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataPoints.Integers.Integer;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataPoints.Integers.Long;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataPoints.Strings;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.Model.Nodes.DataFiles;

public sealed class DataFileNodeTests
{
    private readonly IntegerFileNode _integerFile =
        new(LinkedNodeOf(IntegerFileNode.LinkedNodeTypeId), new FileNumber(7));

    private readonly LongFileNode _longFile =
        new(LinkedNodeOf(LongFileNode.LinkedNodeTypeId), new FileNumber(9));

    private readonly FloatFileNode _floatFile =
        new(LinkedNodeOf(FloatFileNode.LinkedNodeTypeId), new FileNumber(8));

    private readonly StringFileNode _stringFile =
        new(LinkedNodeOf(StringFileNode.LinkedNodeTypeId), new FileNumber(9));

    private readonly AsciiFileNode _asciiFile =
        new(LinkedNodeOf(AsciiFileNode.LinkedNodeTypeId), new FileNumber(10));

    private readonly BinaryFileNode _binaryFile =
        new(LinkedNodeOf(BinaryFileNode.LinkedNodeTypeId), new FileNumber(3));

    private readonly IntegerNode _integerElement = new(
        LinkedNodeOf(IntegerNode.LinkedNodeTypeId), new ElementNumber(0), PollFrequency.FromMilliseconds(500));

    private readonly LongNode _longElement = new(
        LinkedNodeOf(LongNode.LinkedNodeTypeId), new ElementNumber(0), PollFrequency.FromMilliseconds(500));

    private readonly FloatNode _floatElement = new(
        LinkedNodeOf(FloatNode.LinkedNodeTypeId), new ElementNumber(0), PollFrequency.FromMilliseconds(500));

    private readonly StringNode _stringElement = new(
        LinkedNodeOf(StringNode.LinkedNodeTypeId), new ElementNumber(1), PollFrequency.FromMilliseconds(500));

    private readonly AsciiNode _asciiElement = new(
        LinkedNodeOf(AsciiNode.LinkedNodeTypeId), new ElementNumber(0), PollFrequency.FromMilliseconds(500));

    private readonly BitNode _bit = new(
        LinkedNodeOf(BitNode.LinkedNodeTypeId), new ElementNumber(2), new BitNumber(5), PollFrequency.FromMilliseconds(500));

    [Fact]
    public void AnIntegerFileHoldsIntegerElements()
    {
        // Arrange

        // Act
        var canBeAdded = _integerFile.CanBeAdded(_integerElement);

        // Assert
        canBeAdded.Should().BeTrue();
    }

    [Fact]
    public void AnIntegerFileHoldsNoBit()
    {
        // Arrange

        // Act
        var canBeAdded = _integerFile.CanBeAdded(_bit);

        // Assert
        canBeAdded.Should().BeFalse();
    }

    [Fact]
    public void AnIntegerFileHoldsNoFloatElement()
    {
        // Arrange

        // Act
        var canBeAdded = _integerFile.CanBeAdded(_floatElement);

        // Assert
        canBeAdded.Should().BeFalse();
    }

    [Fact]
    public void AnIntegerFileHoldsNoLongElement()
    {
        // Arrange

        // Act
        var canBeAdded = _integerFile.CanBeAdded(_longElement);

        // Assert
        canBeAdded.Should().BeFalse();
    }

    [Fact]
    public void ALongFileHoldsLongElements()
    {
        // Arrange

        // Act
        var canBeAdded = _longFile.CanBeAdded(_longElement);

        // Assert
        canBeAdded.Should().BeTrue();
    }

    [Fact]
    public void ALongFileHoldsNoIntegerElement()
    {
        // Arrange

        // Act
        var canBeAdded = _longFile.CanBeAdded(_integerElement);

        // Assert
        canBeAdded.Should().BeFalse();
    }

    [Fact]
    public void AFloatFileHoldsFloatElements()
    {
        // Arrange

        // Act
        var canBeAdded = _floatFile.CanBeAdded(_floatElement);

        // Assert
        canBeAdded.Should().BeTrue();
    }

    [Fact]
    public void AFloatFileHoldsNoIntegerElement()
    {
        // Arrange

        // Act
        var canBeAdded = _floatFile.CanBeAdded(_integerElement);

        // Assert
        canBeAdded.Should().BeFalse();
    }

    [Fact]
    public void AStringFileHoldsStringElements()
    {
        // Arrange

        // Act
        var canBeAdded = _stringFile.CanBeAdded(_stringElement);

        // Assert
        canBeAdded.Should().BeTrue();
    }

    [Fact]
    public void AStringFileHoldsNoIntegerElement()
    {
        // Arrange

        // Act
        var canBeAdded = _stringFile.CanBeAdded(_integerElement);

        // Assert
        canBeAdded.Should().BeFalse();
    }

    [Fact]
    public void AnIntegerFileHoldsNoStringElement()
    {
        // Arrange

        // Act
        var canBeAdded = _integerFile.CanBeAdded(_stringElement);

        // Assert
        canBeAdded.Should().BeFalse();
    }

    [Fact]
    public void AnAsciiFileHoldsAsciiElements()
    {
        // Arrange

        // Act
        var canBeAdded = _asciiFile.CanBeAdded(_asciiElement);

        // Assert
        canBeAdded.Should().BeTrue();
    }

    [Fact]
    public void AnAsciiFileHoldsNoStringElement()
    {
        // Arrange

        // Act
        var canBeAdded = _asciiFile.CanBeAdded(_stringElement);

        // Assert
        canBeAdded.Should().BeFalse();
    }

    [Fact]
    public void AStringFileHoldsNoAsciiElement()
    {
        // Arrange

        // Act
        var canBeAdded = _stringFile.CanBeAdded(_asciiElement);

        // Assert
        canBeAdded.Should().BeFalse();
    }

    [Fact]
    public void ABinaryFileHoldsBits()
    {
        // Arrange

        // Act
        var canBeAdded = _binaryFile.CanBeAdded(_bit);

        // Assert
        canBeAdded.Should().BeTrue();
    }

    [Fact]
    public void ABinaryFileHoldsNoIntegerElement()
    {
        // Arrange

        // Act
        var canBeAdded = _binaryFile.CanBeAdded(_integerElement);

        // Assert
        canBeAdded.Should().BeFalse();
    }

    [Fact]
    public void ADataFileHoldsNoOtherDataFile()
    {
        // Arrange
        var nestedFile = new IntegerFileNode(LinkedNodeOf(IntegerFileNode.LinkedNodeTypeId), new FileNumber(10));

        // Act
        var canBeAdded = _integerFile.CanBeAdded(nestedFile);

        // Assert
        canBeAdded.Should().BeFalse();
    }

    private static LinkedNode LinkedNodeOf(string designId) =>
        LinkedNodeFactory.Create([new Node { DesignId = designId, Name = designId, Id = Guid.NewGuid() }]).Single();
}
