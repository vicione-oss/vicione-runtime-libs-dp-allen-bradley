using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.Tags;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Client.Tags;

/// <summary>
/// Creates the tag handle for a data-file address. The interface keeps libplctag out of
/// <see cref="LegacyTagSource"/>, so the client can be tested with a fake factory and without a controller.
/// </summary>
internal interface ILegacyTagAccessFactory
{
    /// <summary>Creates the tag handle for <paramref name="address"/>.</summary>
    ITagAccess CreateAccessForAddress(DataFileAddress address);
}
