using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.Extensions.Exceptions;
using ViciOne.Suite.DataPort.Extensions.Outgoing.Client;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Client;

/// <summary>
/// Writes a batch of Legacy data point values to the controller. A failing element does not stop the rest
/// of the batch; afterwards the write throws, naming every element that was dropped and why. A write
/// throws <see cref="ConnectionFailureException"/> when the connection to the controller is lost, and
/// <see cref="OperationCanceledException"/> when it is cancelled.
/// </summary>
public interface ILegacyWriteClient : IWriteClient<ILegacyDataPointValue>;
