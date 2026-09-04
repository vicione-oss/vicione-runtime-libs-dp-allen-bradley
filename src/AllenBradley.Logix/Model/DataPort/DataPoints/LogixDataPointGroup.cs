using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;

/// <summary>
/// The data points an incoming port reads together because they share a poll frequency. One polling job
/// runs per group, and one group is one call to <see cref="Client.ILogixReadClient"/>.
/// Reads are issued concurrently and reported per point, so a group is never all-or-nothing.
/// </summary>
/// <param name="PollFrequency">The frequency every point in this group is read at.</param>
/// <param name="DataPoints">The points read together.</param>
public sealed record LogixDataPointGroup(PollFrequency PollFrequency, IReadOnlyList<ILogixDataPoint> DataPoints)
    : IDataPointPollingGroup<ILogixDataPoint>;
