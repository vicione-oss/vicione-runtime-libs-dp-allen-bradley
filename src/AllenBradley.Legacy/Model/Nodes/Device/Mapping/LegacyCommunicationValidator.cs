using FluentValidation;
using ViciOne.Suite.DataPort.Extensions.Outgoing.QueueProcessing;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.Device.Mapping;

/// <summary>
/// Rejects a device configuration a connection could not be opened from, before anything opens a socket.
/// Whether a controller answers at the configured endpoint is the connect's business, not this class's.
/// </summary>
public sealed class LegacyCommunicationValidator : AbstractValidator<LegacyCommunication>
{
    public LegacyCommunicationValidator()
    {
        RuleFor(static communication => communication.ConnectionEndpoint)
            .NotEmpty()
            .WithMessage("Connection endpoint must be the IP address or host name of the controller.");

        RuleFor(static communication => communication.TcpPort)
            .GreaterThan((ushort)0)
            .WithMessage("TCP port must be at least 1.");

        RuleFor(static communication => communication.OperationTimeout)
            .GreaterThanOrEqualTo(100)
            .WithMessage("OperationTimeout must be at least 100ms.");

        RuleFor(static communication => communication.MaxPendingMessages)
            .GreaterThanOrEqualTo(1)
            .WithMessage("MaxPendingMessages must be at least 1.");

        RuleFor(static communication => communication.Strategy)
            .Must(static strategy => Enum.IsDefined((QueueStrategy)strategy))
            .WithMessage($"Strategy must be one of: {string.Join(", ", Enum.GetNames<QueueStrategy>())}.");
    }
}
