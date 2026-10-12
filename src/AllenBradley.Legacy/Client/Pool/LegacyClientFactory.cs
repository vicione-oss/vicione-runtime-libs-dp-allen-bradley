using Microsoft.Extensions.Logging;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Client.Tags;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.Device;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Client.Pool;

/// <summary>
/// Creates the production client for one controller:
/// <see cref="LegacyTagAccessFactory"/> → <see cref="LegacyTagSource"/> → <see cref="LegacyClient"/>.
/// </summary>
internal sealed class LegacyClientFactory(ILoggerFactory loggerFactory)
{
    /// <summary>Creates a disconnected client. The pool connects it.</summary>
    public ILegacyClient Create(LegacyClientInformation clientInformation)
    {
        var tagSource = new LegacyTagSource(
            new LegacyTagAccessFactory(clientInformation), loggerFactory.CreateLogger<LegacyTagSource>());

        return new LegacyClient(tagSource, clientInformation, loggerFactory.CreateLogger<LegacyClient>());
    }
}
