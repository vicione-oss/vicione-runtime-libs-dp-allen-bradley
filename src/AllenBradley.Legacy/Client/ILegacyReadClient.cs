using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.Extensions.Exceptions;
using ViciOne.Suite.DataPort.Extensions.Incoming.Client;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Client;

/// <summary>
/// Reads a <see cref="LegacyDataPointGroup"/> and returns a typed value per element that answered, in the
/// order the group listed them. An element that failed is left out. A read throws
/// <see cref="ConnectionFailureException"/> when the connection to the controller is lost, and
/// <see cref="OperationCanceledException"/> when it is cancelled.
/// </summary>
public interface ILegacyReadClient : IReadClient<LegacyDataPointGroup, ILegacyDataPointValue>;
