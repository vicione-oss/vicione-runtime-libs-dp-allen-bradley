using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.Tags;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Client;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Client.Tags;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.Extensions.Exceptions;
using ViciOne.Suite.DataPort.Extensions.Testing.Logging;
using static ViciOne.Suite.DataPort.AllenBradley.Legacy.Client.LegacyClient;
using static ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.TestData.LegacyClientTestDataFactory;
using static ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.TestData.LegacyDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.Client;

public sealed class LegacyClientTests : IDisposable
{
    private const string Timeout = "timeout";

    private readonly FakeTagAccessFactory _tagAccessFactory = new();
    private readonly LegacyClient _client;

    public LegacyClientTests() =>
        _client = new LegacyClient(
            new LegacyTagSource(_tagAccessFactory, TestLogging.CreateLogger<LegacyTagSource>()),
            DefaultClientInformation(),
            TestLogging.CreateLogger<LegacyClient>());

    public void Dispose() => _client.Dispose();

    [Fact]
    public async Task ReadReturnsTheValueOfAnIntegerElement()
    {
        // Arrange
        var element = N7Element(0);
        _tagAccessFactory.ReturnOnRead(element, [0x2A, 0x00]);

        // Act
        var values = await _client.ReadAsync(GroupOf(element), CancellationToken.None);

        // Assert
        ILegacyDataPointValue[] expected = [element.CreateLegacyValue(42)];
        values.Should().Equal(expected);
    }

    /// <summary>The bytes of an element at both ends of the short range and around zero, with their value.</summary>
    public static TheoryData<byte[], short> IntegerElements =>
        new()
        {
            { [0x00, 0x80], -32768 },
            { [0xFF, 0xFF], -1 },
            { [0x00, 0x00], 0 },
            { [0xFF, 0x7F], 32767 },
        };

    [Theory]
    [MemberData(nameof(IntegerElements))]
    public async Task ReadDecodesTheWholeShortRange(byte[] bytes, short expectedValue)
    {
        // Arrange
        var element = N7Element(0);
        _tagAccessFactory.ReturnOnRead(element, bytes);

        // Act
        var values = await _client.ReadAsync(GroupOf(element), CancellationToken.None);

        // Assert
        ILegacyDataPointValue[] expected = [element.CreateLegacyValue(expectedValue)];
        values.Should().Equal(expected);
    }

    [Fact]
    public async Task ReadReturnsTheValueOfALongElement()
    {
        // Arrange
        var element = L9Element(0);
        _tagAccessFactory.ReturnOnRead(element, [0xA0, 0x86, 0x01, 0x00]);

        // Act
        var values = await _client.ReadAsync(GroupOf(element), CancellationToken.None);

        // Assert
        ILegacyDataPointValue[] expected = [element.CreateLegacyValue(100_000)];
        values.Should().Equal(expected);
    }

    [Fact]
    public async Task ReadReturnsTheValueOfAStringElement()
    {
        // Arrange
        var element = ST9Element(1);
        var bytes = new byte[84];
        byte[] lengthAndCharacters = [0x02, 0x00, 0x69, 0x48];
        lengthAndCharacters.CopyTo(bytes, 0);
        _tagAccessFactory.ReturnOnRead(element, bytes);

        // Act
        var values = await _client.ReadAsync(GroupOf(element), CancellationToken.None);

        // Assert
        ILegacyDataPointValue[] expected = [element.CreateLegacyValue("Hi")];
        values.Should().Equal(expected);
    }

    [Fact]
    public async Task ReadReturnsTheValueOfAFloatElement()
    {
        // Arrange
        var element = F8Element(12);
        _tagAccessFactory.ReturnOnRead(element, [0x00, 0x00, 0xC0, 0x3F]);

        // Act
        var values = await _client.ReadAsync(GroupOf(element), CancellationToken.None);

        // Assert
        ILegacyDataPointValue[] expected = [element.CreateLegacyValue(1.5f)];
        values.Should().Equal(expected);
    }

    [Theory]
    [InlineData(0x01, true)]
    [InlineData(0x00, false)]
    public async Task ReadReturnsTheValueOfABit(byte bitByte, bool expectedValue)
    {
        // Arrange
        var bit = B3Bit(2, 0);
        _tagAccessFactory.ReturnOnRead(bit, [bitByte]);

        // Act
        var values = await _client.ReadAsync(GroupOf(bit), CancellationToken.None);

        // Assert
        ILegacyDataPointValue[] expected = [bit.CreateLegacyValue(expectedValue)];
        values.Should().Equal(expected);
    }

    [Fact]
    public async Task ReadReturnsTheOtherValuesWhenOneElementFails()
    {
        // Arrange
        var readable = N7Element(0);
        var unreadable = N7Element(1);
        _tagAccessFactory.ReturnOnRead(readable, [0x2A, 0x00]);
        _tagAccessFactory.FailOnRead(unreadable, Timeout);

        // Act
        var values = await _client.ReadAsync(GroupOf(readable, unreadable), CancellationToken.None);

        // Assert
        ILegacyDataPointValue[] expected = [readable.CreateLegacyValue(42)];
        values.Should().Equal(expected);
    }

    [Fact]
    public async Task ReadThrowsAndNamesEachElementWhenNoElementCanBeRead()
    {
        // Arrange
        var first = N7Element(0);
        var second = N7Element(1);
        _tagAccessFactory.FailOnRead(first, Timeout);
        _tagAccessFactory.FailOnRead(second, Timeout);

        // Act
        var reading = _client.Awaiting(client => client.ReadAsync(GroupOf(first, second), CancellationToken.None));

        // Assert
        await reading.Should().ThrowAsync<LegacyTagException>()
            .WithMessage("Read failed for N7:0: timeout; N7:1: timeout.");
    }

    [Fact]
    public async Task WriteSendsTheIntegerAsTwoLittleEndianBytes()
    {
        // Arrange
        var element = N7Element(0);

        // Act
        await _client.WriteAsync([element.CreateLegacyValue(42)], CancellationToken.None);

        // Assert
        byte[] expected = [0x2A, 0x00];
        _tagAccessFactory.BytesWrittenTo(element).Should().Equal(expected);
    }

    /// <summary>Values at both ends of the short range and minus one, with the bytes of the element.</summary>
    public static TheoryData<short, byte[]> IntegerValues =>
        new()
        {
            { -32768, [0x00, 0x80] },
            { -1, [0xFF, 0xFF] },
            { 32767, [0xFF, 0x7F] },
        };

    [Theory]
    [MemberData(nameof(IntegerValues))]
    public async Task WriteEncodesTheWholeShortRange(short value, byte[] expectedBytes)
    {
        // Arrange
        var element = N7Element(0);

        // Act
        await _client.WriteAsync([element.CreateLegacyValue(value)], CancellationToken.None);

        // Assert
        _tagAccessFactory.BytesWrittenTo(element).Should().Equal(expectedBytes);
    }

    [Theory]
    [InlineData(true, 0x01)]
    [InlineData(false, 0x00)]
    public async Task WriteSendsABitAsOneByte(bool value, byte expectedByte)
    {
        // Arrange
        var bit = B3Bit(2, 0);

        // Act
        await _client.WriteAsync([bit.CreateLegacyValue(value)], CancellationToken.None);

        // Assert
        _tagAccessFactory.BytesWrittenTo(bit).Should().Equal(expectedByte);
    }

    [Fact]
    public async Task WriteThrowsAndNamesEachElementThatWasNotWritten()
    {
        // Arrange
        var writable = N7Element(0);
        var firstUnwritable = N7Element(1);
        var secondUnwritable = N7Element(2);
        _tagAccessFactory.FailOnWrite(firstUnwritable, Timeout);
        _tagAccessFactory.FailOnWrite(secondUnwritable, Timeout);
        ILegacyDataPointValue[] values =
        [
            writable.CreateLegacyValue(1),
            firstUnwritable.CreateLegacyValue(2),
            secondUnwritable.CreateLegacyValue(3),
        ];

        // Act
        var writing = _client.Awaiting(client => client.WriteAsync(values, CancellationToken.None));

        // Assert
        await writing.Should().ThrowAsync<LegacyTagException>()
            .WithMessage("Write failed for N7:1: timeout; N7:2: timeout.");
    }

    [Fact]
    public async Task ConnectReadsTheFirstWordOfTheStatusFile()
    {
        // Arrange
        _tagAccessFactory.ReturnOnRead(StatusFile, [0x00, 0x00]);

        // Act
        await _client.ConnectAsync(CancellationToken.None);

        // Assert
        _tagAccessFactory.ReadCount(StatusFile).Should().Be(1);
    }

    [Fact]
    public async Task AClientWhoseStatusFileCouldNotBeReadReportsAConnectionFailureAndStaysDisconnected()
    {
        // Arrange
        _tagAccessFactory.FailOnRead(StatusFile, Timeout);

        // Act
        var connect = await Record.ExceptionAsync(() => _client.ConnectAsync(CancellationToken.None));

        // Assert
        connect.Should().BeOfType<ConnectionFailureException>();
        _client.IsConnected.Should().BeFalse();
    }

    [Fact]
    public async Task AClientWhoseStatusFileReadThrewReportsAConnectionFailureAndStaysDisconnected()
    {
        // Arrange
        _tagAccessFactory.ThrowOnRead(StatusFile, new DllNotFoundException("plctag"));

        // Act
        var connect = await Record.ExceptionAsync(() => _client.ConnectAsync(CancellationToken.None));

        // Assert
        connect.Should().BeOfType<ConnectionFailureException>()
            .Which.InnerException.Should().BeOfType<DllNotFoundException>();
        _client.IsConnected.Should().BeFalse();
    }

    [Fact]
    public async Task ACancelledConnectThrowsTheCancellationRatherThanAConnectionFailure()
    {
        // Arrange
        _tagAccessFactory.ThrowOnRead(StatusFile, new OperationCanceledException());

        // Act
        var connect = await Record.ExceptionAsync(() => _client.ConnectAsync(CancellationToken.None));

        // Assert
        connect.Should().BeAssignableTo<OperationCanceledException>();
        _client.IsConnected.Should().BeFalse();
    }

    [Fact]
    public async Task ConnectDisposesTheTagHandleOfTheStatusFile()
    {
        // Arrange
        _tagAccessFactory.ReturnOnRead(StatusFile, [0x00, 0x00]);

        // Act
        await _client.ConnectAsync(CancellationToken.None);

        // Assert
        _tagAccessFactory.IsDisposed(StatusFile).Should().BeTrue();
    }

    [Fact]
    public async Task ASecondConnectDoesNotReadTheStatusFileAgain()
    {
        // Arrange
        _tagAccessFactory.ReturnOnRead(StatusFile, [0x00, 0x00]);
        await _client.ConnectAsync(CancellationToken.None);

        // Act
        await _client.ConnectAsync(CancellationToken.None);

        // Assert
        _tagAccessFactory.ReadCount(StatusFile).Should().Be(1);
    }

    [Fact]
    public async Task DisconnectDisposesEveryTagHandleOfTheClient()
    {
        // Arrange
        var element = N7Element(0);
        _tagAccessFactory.ReturnOnRead(StatusFile, [0x00, 0x00]);
        _tagAccessFactory.ReturnOnRead(element, [0x2A, 0x00]);
        await _client.ConnectAsync(CancellationToken.None);
        await _client.ReadAsync(GroupOf(element), CancellationToken.None);

        // Act
        await _client.DisconnectAsync(CancellationToken.None);

        // Assert
        _tagAccessFactory.IsDisposed(element).Should().BeTrue();
    }

    private static LegacyDataPointGroup GroupOf(params ILegacyDataPoint[] dataPoints) =>
        new(DefaultPollFrequency, dataPoints);

    private sealed class FakeTagAccessFactory : ILegacyTagAccessFactory
    {
        private readonly Dictionary<DataFileAddress, FakeTagHandle> _handles = [];

        internal void ReturnOnRead(ILegacyDataPoint dataPoint, byte[] bytes) => ReturnOnRead(dataPoint.Address, bytes);

        internal void ReturnOnRead(DataFileAddress address, byte[] bytes) =>
            HandleFor(address).ReadResult = TagReadResult.Ok(bytes);

        internal void FailOnRead(ILegacyDataPoint dataPoint, string error) => FailOnRead(dataPoint.Address, error);

        internal void FailOnRead(DataFileAddress address, string error) =>
            HandleFor(address).ReadResult = TagReadResult.Failed(error);

        internal void ThrowOnRead(DataFileAddress address, Exception exception) =>
            HandleFor(address).ReadException = exception;

        internal void FailOnWrite(ILegacyDataPoint dataPoint, string error) =>
            HandleFor(dataPoint.Address).WriteResult = TagWriteResult.Failed(error);

        internal byte[]? BytesWrittenTo(ILegacyDataPoint dataPoint) => HandleFor(dataPoint.Address).Written;

        internal int ReadCount(DataFileAddress address) => HandleFor(address).ReadCount;

        internal bool IsDisposed(ILegacyDataPoint dataPoint) => IsDisposed(dataPoint.Address);

        internal bool IsDisposed(DataFileAddress address) => HandleFor(address).IsDisposed;

        public ITagAccess CreateAccessForAddress(DataFileAddress address) => HandleFor(address);

        private FakeTagHandle HandleFor(DataFileAddress address)
        {
            if (!_handles.TryGetValue(address, out var handle))
            {
                handle = new FakeTagHandle();
                _handles.Add(address, handle);
            }

            return handle;
        }
    }

    private sealed class FakeTagHandle : ITagAccess
    {
        internal TagReadResult ReadResult { get; set; } = TagReadResult.Failed("no value configured");

        internal Exception? ReadException { get; set; }

        internal TagWriteResult WriteResult { get; set; } = TagWriteResult.Ok();

        internal byte[]? Written { get; private set; }

        internal int ReadCount { get; private set; }

        internal bool IsDisposed { get; private set; }

        public Task<TagReadResult> ReadAsync(CancellationToken cancellationToken)
        {
            ReadCount++;
            return ReadException is null ? Task.FromResult(ReadResult) : Task.FromException<TagReadResult>(ReadException);
        }

        public Task<TagWriteResult> WriteAsync(byte[] buffer, CancellationToken cancellationToken)
        {
            Written = buffer;
            return Task.FromResult(WriteResult);
        }

        public void Dispose() => IsDisposed = true;
    }
}
