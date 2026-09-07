using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.Extensions.Incoming.Client;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client;

/// <summary>
/// Reads a <see cref="LogixDataPointGroup"/> in one batched operation and returns a typed value per
/// point, in the order the group listed them.
/// The group is the unit of delivery: every point comes back or none does. A point that could not be
/// read or decoded fails the read with a <see cref="LogixTagException"/> naming every such point, and
/// the poll that drove it publishes nothing rather than a group with holes in it.
/// </summary>
public interface ILogixReadClient : IReadClient<LogixDataPointGroup, ILogixDataPointValue>;
