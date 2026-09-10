using libplctag;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays;
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
    public ILogixTagAccess Create(ILogixDataPoint dataPoint)
    {
        var tag = CreateTag(dataPoint.TagName);

        // libplctag treats every tag as an array and reads one element unless told otherwise, so scalars
        // say nothing here. ElementSize stays unset: the library ignores it for Allen-Bradley and takes
        // the width from the controller's own declaration.
        if (dataPoint is ILogixArrayDataPoint arrayDataPoint)
        {
            tag.ElementCount = arrayDataPoint.ElementCount.Value;
        }

        return Wrap(tag);
    }

    /// <inheritdoc />
    public ILogixTagAccess CreateForSchemaTag(TagName tagName) => Wrap(CreateTag(tagName));

    // A schema name needs no special binding: libplctag resolves @tags and @udt/<id> itself, so the
    // attribute string is the same either way.
    private Tag CreateTag(TagName tagName) =>
        new()
        {
            Gateway = _gateway,
            Path = _cipRoutePath,
            PlcType = LogixPlcType,
            Protocol = Protocol.ab_eip,
            Name = tagName.Value,
            Timeout = _timeout,
            AllowPacking = true,
        };

    private static SynchronizedLogixTagAccess Wrap(Tag tag) =>
        new(new LogixTagAccess(tag));
}
