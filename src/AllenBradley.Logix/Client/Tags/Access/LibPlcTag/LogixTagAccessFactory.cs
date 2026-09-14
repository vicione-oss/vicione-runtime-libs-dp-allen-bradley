using libplctag;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.Booleans;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Access.LibPlcTag;

/// <summary>
/// Builds libplctag-backed access for the controller identified by <paramref name="clientInformation"/>,
/// binding the connection attributes onto every tag. It creates and never owns — disposal belongs to
/// whoever holds the access, in practice <see cref="Lifetime.CachingLogixTagManager"/>.
/// </summary>
internal sealed class LogixTagAccessFactory(LogixClientInformation clientInformation) : ILogixTagAccessFactory
{
    /// <summary>
    /// libplctag has one PLC type for the whole Logix-5000 line: a CompactLogix is opened as a
    /// ControlLogix and answers identically.
    /// </summary>
    private const PlcType LogixPlcType = PlcType.ControlLogix;

    private readonly string _gateway = GatewayAttribute.For(clientInformation).Value;
    private readonly string _cipRoutePath = clientInformation.CipRoutePath.Value;
    private readonly TimeSpan _timeout = clientInformation.OperationTimeout.Value;

    /// <inheritdoc />
    public ILogixTagAccess Create(ILogixDataPoint dataPoint) => CreateSynchronizedLogixTagFrom(CreateTagFor(dataPoint));

    /// <inheritdoc />
    public ILogixTagAccess CreateForSchemaTag(TagName tagName) => CreateSynchronizedLogixTagFrom(CreateTag(tagName));

    // Internal rather than private because nothing above the factory exposes a handle's attributes, and
    // the element count is the difference between reading an array and reading its first element.
    internal Tag CreateTagFor(ILogixDataPoint dataPoint)
    {
        var tag = CreateTag(dataPoint.TagName);
        tag.ElementCount = (int)GetElementCount(dataPoint).Value;
        return tag;
    }

    // libplctag treats every tag as an array and puts this count on the CIP request as it stands, so it
    // is what the controller counts and not what a value holds: a BOOL array is counted in the 32-bit
    // words its bits are packed into. A scalar is said to be one rather than left to the library's
    // default. ElementSize stays unset: the library ignores it for Allen-Bradley and takes the width
    // from the controller's own declaration.
    private static ElementCount GetElementCount(ILogixDataPoint dataPoint) => dataPoint switch
    {
        BoolArrayDataPoint boolArrayDataPoint => boolArrayDataPoint.WordCount,
        ILogixArrayDataPoint arrayDataPoint => arrayDataPoint.ElementCount,
        _ => ElementCount.Scalar,
    };

    // A schema name needs no special binding: libplctag resolves @tags and @udt/<id> itself, so the
    // attribute string is the same either way.
    internal Tag CreateTag(TagName tagName) =>
        new()
        {
            Gateway = _gateway,
            Path = _cipRoutePath,
            PlcType = LogixPlcType,
            Protocol = Protocol.ab_eip,
            Name = tagName.Value,
            Timeout = _timeout,
            AllowPacking = true
        };

    private static SynchronizedLogixTagAccess CreateSynchronizedLogixTagFrom(Tag tag) =>
        new(new LogixTagAccess(tag));
}
