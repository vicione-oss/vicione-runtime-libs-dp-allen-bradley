using ViciOne.Suite.DataPort.AllenBradley.Logix.Client;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.DataPort.DataPoints;

/// <summary>
/// What a Logix data point looks like to the framework: the <see cref="IPollingDataPoint"/> members the
/// port groups, logs and routes by, and the values the point makes. Nothing else in the suite reads the
/// first set, because everything below the client works off the concrete record instead.
/// </summary>
/// <remarks>
/// The value half is where the type system earns its keep, so it is tested through the model rather
/// than through a converter: a typed value is Good and carries the point's own .NET type by
/// construction, a failed read is a different shape entirely, and <c>ConvertValue</c> is the one place
/// an untyped engine value can become either.
/// </remarks>
public class LogixDataPointTests
{
    public static TheoryData<ILogixDataPoint, string> TypeNames => new()
    {
        { CreateDInt("Motor.Speed"), "DINT" },
        { CreateReal("Tank.Level"), "REAL" },
        { CreateString("Line.Label"), "STRING" },
    };

    public static TheoryData<ILogixDataPoint> EveryModelledType =>
        [CreateDInt("Motor.Speed"), CreateReal("Tank.Level"), CreateString("Line.Label")];

    /// <summary>Each modelled point with an engine value of the .NET type it exchanges.</summary>
    public static TheoryData<ILogixDataPoint, object> EveryModelledTypeAndItsEngineValue => new()
    {
        { CreateDInt("Motor.Speed"), 42 },
        { CreateReal("Tank.Level"), 1.5f },
        { CreateString("Line.Label"), "Hi" },
    };

    /// <summary>
    /// Each modelled point with an engine value it cannot hold, and the type it wanted instead. A REAL
    /// is offered a <see cref="double"/> deliberately: the near-miss numeric type is the mistake a
    /// configuration actually makes, and the model does not widen or narrow silently.
    /// </summary>
    public static TheoryData<ILogixDataPoint, object, Type> EveryModelledTypeAndAValueItCannotHold => new()
    {
        { CreateDInt("Motor.Speed"), "42", typeof(int) },
        { CreateReal("Tank.Level"), 1.5d, typeof(float) },
        { CreateString("Line.Label"), 42, typeof(string) },
    };

    [Theory]
    [MemberData(nameof(TypeNames))]
    public void DataTypeName_IsTheStudio5000Spelling(ILogixDataPoint dataPoint, string expected)
    {
        // Arrange

        // Act
        var dataTypeName = dataPoint.DataTypeName;

        // Assert
        dataTypeName.Should().Be(new DataTypeName(expected));
    }

    [Theory]
    [MemberData(nameof(EveryModelledType))]
    public void DataTypeName_IsTheNameItsConverterReports(ILogixDataPoint dataPoint)
    {
        // Arrange

        // Act
        var converter = DataPointConverterRegistry.GetConverter(dataPoint);

        // Assert
        // One spelling per type, read from two places: the point names itself for the framework's logs,
        // the converter names the same type in a verification message. They are the same constant, and
        // this is what keeps a type added later from acquiring a second spelling.
        converter.ExpectedTypeName.Value.Should().Be(dataPoint.DataTypeName.Value);
    }

    [Fact]
    public void Identifier_IsTheTagAddress()
    {
        // Arrange
        var dataPoint = CreateDInt("Program:MainProgram.Counter.PRE");

        // Act
        var identifier = dataPoint.Identifier;

        // Assert
        // The address, not a synthesised name: it is what an operator reads in a log and searches the
        // controller for.
        identifier.Should().Be(new DataPointIdentifier("Program:MainProgram.Counter.PRE"));
    }

    [Fact]
    public void AValue_ProjectsItsDataPointAndPayloadOntoTheFrameworkView()
    {
        // Arrange
        var dataPoint = CreateDInt("Motor.Speed");

        // Act
        // The pipeline carries values as IDataPointValue, so this is the view the outgoing port validates
        // and the incoming port forwards — projected from the typed members, never stored twice.
        IDataPointValue value = CreateValue(dataPoint, 42);

        // Assert
        value.DataPoint.Should().BeSameAs(dataPoint);
        value.Value.Should().Be(42);
        value.IsInValueRange().Should().BeTrue();
    }

    [Fact]
    public void AValue_IsGoodByConstruction()
    {
        // Arrange
        var dataPoint = CreateDInt("Motor.Speed");

        // Act
        var value = CreateValue(dataPoint, 42);

        // Assert
        // There is no way to make a typed value that is not Good: a read with nothing to carry comes
        // home as a BadLogixDataPointValue instead, so a payload of zero is always a real zero.
        value.Quality.Should().Be(LogixQuality.Good);
    }

    [Fact]
    public void ABadValue_CarriesTheDataPointAndNoPayload()
    {
        // Arrange
        var dataPoint = CreateDInt("Motor.Speed");

        // Act
        ILogixDataPointValue value = new BadLogixDataPointValue(dataPoint);

        // Assert
        value.Quality.Should().Be(LogixQuality.Bad);
        value.DataPoint.Should().BeSameAs(dataPoint);
        value.Value.Should().BeNull("a failed read has nothing to report, and default(int) is a value a tag can hold");
    }

    [Theory]
    [MemberData(nameof(EveryModelledTypeAndItsEngineValue))]
    public void ConvertValue_WhenTheEngineValueIsTheDataPointsType_CarriesItThrough(
        ILogixDataPoint dataPoint, object engineValue)
    {
        // Arrange

        // Act
        var conversion = dataPoint.ConvertValue(engineValue);

        // Assert
        // The one door an object? enters the model through: past it a payload is the point's own .NET
        // type, which is what lets the write path skip a second type check.
        conversion.Should().BeOfType<ConvertedDataPointValue<ILogixDataPointValue>>()
            .Which.Value.Value.Should().Be(engineValue);
    }

    [Theory]
    [MemberData(nameof(EveryModelledType))]
    public void ConvertValue_WhenTheEngineValueIsNull_FailsNamingThePointAndItsType(ILogixDataPoint dataPoint)
    {
        // Arrange

        // Act
        var conversion = dataPoint.ConvertValue(null);

        // Assert
        // A rejected write is diagnosed from this message alone: the engine value that caused it is gone
        // by the time anyone reads the log.
        var failure = conversion.Should()
            .BeOfType<NotConvertedDataPointValue<ILogixDataPointValue>>().Subject.Failure;
        failure.DataPoint.Should().BeSameAs(dataPoint);
        failure.Reason.Should().Be(ValidationFailureReason.ConversionFailure);
        failure.Details.Should().Contain(dataPoint.Identifier.Value).And.Contain(dataPoint.DataTypeName.Value);
    }

    [Theory]
    [MemberData(nameof(EveryModelledTypeAndAValueItCannotHold))]
    public void ConvertValue_WhenTheEngineValueIsTheWrongType_FailsNamingBothTypes(
        ILogixDataPoint dataPoint, object engineValue, Type expected)
    {
        // Arrange

        // Act
        var conversion = dataPoint.ConvertValue(engineValue);

        // Assert
        var failure = conversion.Should()
            .BeOfType<NotConvertedDataPointValue<ILogixDataPointValue>>().Subject.Failure;
        failure.Details.Should().Contain(expected.ToString()).And.Contain(engineValue.GetType().ToString());
    }

    [Theory]
    [InlineData(82, true)]
    [InlineData(83, false)]
    public void AStringValue_IsInRangeWhileItFitsTheDeclaredCapacity(int length, bool expected)
    {
        // Arrange
        // The only modelled type whose range is not simply its .NET type. This is the gate that turns an
        // over-long write into a reported failure instead of the converter's exception.
        var dataPoint = CreateString("Line.Label");

        // Act
        var value = CreateValue(dataPoint, new string('X', length));

        // Assert
        value.IsInValueRange().Should().Be(expected);
    }
}
