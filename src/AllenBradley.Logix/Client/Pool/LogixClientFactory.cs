using Microsoft.Extensions.Logging;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Access.LibPlcTag;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Symbols;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Lifetime;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Pool;

/// <summary>
/// Assembles the production client stack for one controller:
/// <see cref="LogixTagAccessFactory"/> → <see cref="SymbolTableLoader"/> →
/// <see cref="CachingLogixTagManager"/> → <see cref="LogixClient"/>.
/// </summary>
internal sealed class LogixClientFactory(ILoggerFactory loggerFactory) : ILogixClientFactory
{
    /// <inheritdoc />
    public ILogixClient Create(LogixClientInformation clientInformation)
    {
        var accessFactory = new LogixTagAccessFactory(clientInformation);

        var tagManager = new CachingLogixTagManager(
            accessFactory,
            new SymbolTableLoader(accessFactory),
            loggerFactory.CreateLogger<CachingLogixTagManager>());

        return new LogixClient(tagManager, clientInformation, loggerFactory.CreateLogger<LogixClient>());
    }
}
