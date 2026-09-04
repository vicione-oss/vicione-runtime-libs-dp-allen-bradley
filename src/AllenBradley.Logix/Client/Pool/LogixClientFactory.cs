using Microsoft.Extensions.Logging;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Access.LibPlcTag;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Definitions;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Lifetime;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Pool;

/// <summary>
/// Assembles the production client stack for one controller:
/// <see cref="LogixTagAccessFactory"/> → <see cref="TagDefinitionsLoader"/> →
/// <see cref="CachingLogixTagManager"/> → <see cref="LogixClient"/>.
/// </summary>
/// <param name="loggerFactory">Supplies the loggers for the stack this factory builds.</param>
internal sealed class LogixClientFactory(ILoggerFactory loggerFactory) : ILogixClientFactory
{
    /// <inheritdoc />
    public ILogixClient Create(LogixClientInformation clientInformation)
    {
        var accessFactory = new LogixTagAccessFactory(clientInformation);

        var tagManager = new CachingLogixTagManager(
            accessFactory,
            new TagDefinitionsLoader(accessFactory),
            loggerFactory.CreateLogger<CachingLogixTagManager>());

        return new LogixClient(tagManager, clientInformation, loggerFactory.CreateLogger<LogixClient>());
    }
}
