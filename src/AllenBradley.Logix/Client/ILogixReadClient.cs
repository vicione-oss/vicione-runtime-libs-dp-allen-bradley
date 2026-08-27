using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.Extensions.Incoming.Client;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client;

/// <summary>
/// Reads a <see cref="LogixDataPointGroup"/> in one batched operation and returns a typed value per
/// point, in the order the group listed them.
/// </summary>
/// <remarks>
/// A point whose read fails comes back with <see cref="LogixQuality.Bad"/> rather than sinking the whole
/// group — the read half of the split described on <see cref="ILogixWriteClient"/>.
/// </remarks>
public interface ILogixReadClient : IReadClient<LogixDataPointGroup, ILogixDataPointValue>;
