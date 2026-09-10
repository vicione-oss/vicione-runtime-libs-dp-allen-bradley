using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.Extensions.Incoming.Client;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client;

/// <summary>
/// Reads a <see cref="LogixDataPointGroup"/> in one batched operation and returns a typed value per
/// point that answered, in the order the group listed them. A point that failed is left out; only a read
/// that produced no value at all throws a <see cref="LogixTagException"/> naming every point and reason.
/// </summary>
public interface ILogixReadClient : IReadClient<LogixDataPointGroup, ILogixDataPointValue>;
