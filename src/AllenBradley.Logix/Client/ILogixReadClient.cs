using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.Extensions.Incoming.Client;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client;

/// <summary>
/// Reads a <see cref="LogixDataPointGroup"/> in one batched operation and returns a typed value per
/// point that answered, in the order the group listed them.
/// Batching is throughput and nothing else, so a point that could not be read or decoded is left out and
/// logged while the rest come back. Only a read where no point at all produced a value throws a
/// <see cref="LogixTagException"/>, naming every point and its reason.
/// </summary>
public interface ILogixReadClient : IReadClient<LogixDataPointGroup, ILogixDataPointValue>;
