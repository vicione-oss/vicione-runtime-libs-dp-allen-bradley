using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.Extensions.Outgoing.Client;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client;

/// <summary>
/// Writes a batch of Logix data point values to the controller.
/// </summary>
/// <remarks>
/// Where a failed read degrades its own point, a failed write throws
/// <see cref="LogixTagException"/> naming every tag that was dropped and why: <c>WriteAsync</c> returns
/// no per-value result, so silence would be a dropped write the caller cannot detect. The values the
/// exception does not name were written — a failing tag does not stop the rest of the batch.
/// </remarks>
public interface ILogixWriteClient : IWriteClient<ILogixDataPointValue>;
