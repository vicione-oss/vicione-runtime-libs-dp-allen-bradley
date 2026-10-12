using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.Tags;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Client;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Client.Tags;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.Extensions.Testing.Logging;
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
    public async Task DisconnectDisposesEveryTagHandleOfTheClient()
    {
        // Arrange
        var element = N7Element(0);
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

        internal void ReturnOnRead(ILegacyDataPoint dataPoint, byte[] bytes) =>
            HandleFor(dataPoint.Address).ReadResult = TagReadResult.Ok(bytes);

        internal void FailOnRead(ILegacyDataPoint dataPoint, string error) =>
            HandleFor(dataPoint.Address).ReadResult = TagReadResult.Failed(error);

        internal void FailOnWrite(ILegacyDataPoint dataPoint, string error) =>
            HandleFor(dataPoint.Address).WriteResult = TagWriteResult.Failed(error);

        internal byte[]? BytesWrittenTo(ILegacyDataPoint dataPoint) => HandleFor(dataPoint.Address).Written;

        internal bool IsDisposed(ILegacyDataPoint dataPoint) => HandleFor(dataPoint.Address).IsDisposed;

        public ITagAccess CreateAccessForDataPoint(ILegacyDataPoint dataPoint) => HandleFor(dataPoint.Address);

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

        internal TagWriteResult WriteResult { get; set; } = TagWriteResult.Ok();

        internal byte[]? Written { get; private set; }

        internal bool IsDisposed { get; private set; }

        public Task<TagReadResult> ReadAsync(CancellationToken cancellationToken) => Task.FromResult(ReadResult);

        public Task<TagWriteResult> WriteAsync(byte[] buffer, CancellationToken cancellationToken)
        {
            Written = buffer;
            return Task.FromResult(WriteResult);
        }

        public void Dispose() => IsDisposed = true;
    }
}
