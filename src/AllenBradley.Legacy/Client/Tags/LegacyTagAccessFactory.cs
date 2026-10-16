using libplctag;
using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.Tags;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints.Strings;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.Device;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Client.Tags;

/// <summary>
/// Creates the libplctag tag handles for the data-file addresses of one controller. The caller disposes each
/// handle. In production, the caller is <see cref="LegacyTagSource"/>.
/// </summary>
internal sealed class LegacyTagAccessFactory(LegacyClientInformation clientInformation) : ILegacyTagAccessFactory
{
    private readonly string _gateway = GatewayAttribute.For(
        clientInformation.ConnectionEndpoint.Value,
        clientInformation.TcpPort.Value).Value;

    private readonly string? _path = PathOf(clientInformation.CipRoutePath);
    private readonly PlcType _plcType = PlcTypeOf(clientInformation.Family);
    private readonly TimeSpan _timeout = clientInformation.OperationTimeout.Value;

    /// <inheritdoc />
    public ITagAccess CreateAccessForAddress(DataFileAddress address) =>
        new SynchronizedTagAccess(CreateLibPlcAccess(address));

    // Internal so that tests can check which access a bit address gets. The synchronized wrapper hides it.
    internal ITagAccess CreateLibPlcAccess(DataFileAddress address) =>
        address.BitNumber is null
            ? new LibPlcTagAccess(CreateTagForAddress(address))
            : new LibPlcBitTagAccess(CreateTagForAddress(address));

    // Internal so that tests can check the attributes of the handle. ITagAccess does not expose them.
    internal Tag CreateTagForAddress(DataFileAddress address) =>
        new()
        {
            Gateway = _gateway,
            Path = _path,
            PlcType = _plcType,
            Name = address.ToString(),
            ElementCount = ElementCountOf(address),
            Protocol = Protocol.ab_eip,
            Timeout = _timeout,
        };

    // libplctag sizes an A element as 1 byte (pccc.c:711), but an A element is a word with two characters.
    // Two such elements give the handle the 2 bytes of one element.
    private static int? ElementCountOf(DataFileAddress address) =>
        address.FileType == DataFileType.Ascii ? AsciiDataPoint.MaxLength : null;

    // For PCCC, libplctag uses a route path only when it ends in a DH+ address such as A:1:5, and logs a
    // warning for any other route path. A direct controller gets no path, so libplctag only gets a route
    // path that the user configured.
    private static string? PathOf(CipRoutePath cipRoutePath) =>
        cipRoutePath == CipRoutePath.Direct ? null : cipRoutePath.Value;

    private static PlcType PlcTypeOf(LegacyControllerFamily family) => family switch
    {
        LegacyControllerFamily.Slc500 => PlcType.Slc500,
        LegacyControllerFamily.MicroLogix => PlcType.MicroLogix,
        _ => throw new ArgumentOutOfRangeException(nameof(family), family, "The controller family is not known."),
    };
}
