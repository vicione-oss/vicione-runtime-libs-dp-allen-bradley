using libplctag;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Access.LibPlcTag;

/// <summary>
/// Builds libplctag-backed access for the controller identified by <paramref name="clientInformation"/>:
/// it maps our controller families onto the native <see cref="PlcType"/> and binds the connection
/// attributes onto every tag. It creates and never owns — disposal belongs to whoever holds the
/// access, in practice <see cref="Lifetime.CachingLogixTagManager"/>.
/// </summary>
/// <remarks>
/// Access comes out wrapped in <see cref="SynchronizedLogixTagAccess"/>, because a cached access is a
/// shared one and a shared access takes one operation at a time.
/// </remarks>
/// <param name="clientInformation">
/// Gateway, path, controller family and per-operation timeout — everything the attribute string needs,
/// carried by the same value the pool keys the connection under.
/// </param>
internal sealed class LogixTagAccessFactory(LogixClientInformation clientInformation) : ILogixTagAccessFactory
{
    private readonly string _gateway = clientInformation.Gateway.Value;
    private readonly string _path = clientInformation.Path.Value;
    private readonly PlcType _plcType = ToLibPlcTagType(clientInformation.ControllerType);
    private readonly TimeSpan _timeout = clientInformation.OperationTimeout.Value;

    // Exhaustive by design: a new LogixControllerType must decide its mapping rather than fall
    // through to a silent default.
    private static PlcType ToLibPlcTagType(LogixControllerType controllerType) => controllerType switch
    {
        LogixControllerType.ControlLogix => PlcType.ControlLogix,
        LogixControllerType.CompactLogix => PlcType.ControlLogix,
        _ => throw new ArgumentOutOfRangeException(
            nameof(controllerType), controllerType, "Unsupported Logix controller type."),
    };

    /// <inheritdoc />
    public ILogixTagAccess Create(ILogixDataPoint dataPoint) => CreateAccess(dataPoint.TagName);

    /// <inheritdoc />
    public ILogixTagAccess CreateForSchemaTag(TagName tagName) => CreateAccess(tagName);

    // A schema name is bound exactly like a tag name: libplctag resolves @tags and @udt/<id> itself,
    // so the attribute string is the same either way.
    private SynchronizedLogixTagAccess CreateAccess(TagName tagName)
    {
        var tag = new Tag
        {
            Gateway = _gateway,
            Path = _path,
            PlcType = _plcType,
            Protocol = Protocol.ab_eip,
            Name = tagName.Value,
            Timeout = _timeout,
            AllowPacking = true,
        };

        return new SynchronizedLogixTagAccess(new LogixTagAccess(tag));
    }
}
