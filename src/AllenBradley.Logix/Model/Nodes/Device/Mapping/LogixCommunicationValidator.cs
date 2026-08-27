using FluentValidation;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using ViciOne.Suite.DataPort.Extensions.Outgoing.QueueProcessing;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Device.Mapping;

/// <summary>
/// Rejects a device configuration the client stack could not be built from, before anything opens a
/// socket. It checks only what is decidable here — a controller that answers to the gateway and path is
/// the connect's business, not this class's.
/// </summary>
public sealed class LogixCommunicationValidator : AbstractValidator<LogixCommunication>
{
    public LogixCommunicationValidator()
    {
        RuleFor(static communication => communication.Gateway)
            .NotEmpty()
            .WithMessage("Gateway must be the IP address or host name of the controller.");

        RuleFor(static communication => communication.Path)
            .NotEmpty()
            .WithMessage("Path must be a CIP routing path to the CPU, e.g. \"1,0\".");

        RuleFor(static communication => communication.ControllerType)
            .Must(static controllerType => Enum.IsDefined((LogixControllerType)controllerType))
            .WithMessage($"ControllerType must be one of: {string.Join(", ", Enum.GetNames<LogixControllerType>())}.");

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
