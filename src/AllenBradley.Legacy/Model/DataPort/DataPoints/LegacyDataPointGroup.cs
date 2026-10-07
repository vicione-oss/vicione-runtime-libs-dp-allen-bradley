using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;

/// <summary>
/// The data points an incoming port reads together because they share a poll frequency. One polling job
/// runs per group.
/// </summary>
/// <param name="PollFrequency">The frequency every point in this group is read at.</param>
/// <param name="DataPoints">The points read together.</param>
public sealed record LegacyDataPointGroup(PollFrequency PollFrequency, IReadOnlyList<ILegacyDataPoint> DataPoints)
    : IDataPointPollingGroup<ILegacyDataPoint>;
