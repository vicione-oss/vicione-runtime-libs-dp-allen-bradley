using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.Extensions.Outgoing.Client;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client;

/// <summary>
/// Writes a batch of Logix data point values to the controller.
/// A failing tag does not stop the rest of the batch; afterwards a <see cref="LogixTagException"/> names
/// every tag that was dropped and why.
/// </summary>
public interface ILogixWriteClient : IWriteClient<ILogixDataPointValue>;
