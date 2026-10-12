using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.Tags;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Client.Tags;

/// <summary>
/// Creates the tag handle for a data point. The interface keeps libplctag out of
/// <see cref="LegacyTagSource"/>, so the client can be tested with a fake factory and without a controller.
/// </summary>
internal interface ILegacyTagAccessFactory
{
    /// <summary>Creates the tag handle for the data-file address of <paramref name="dataPoint"/>.</summary>
    ITagAccess CreateAccessForDataPoint(ILegacyDataPoint dataPoint);
}
