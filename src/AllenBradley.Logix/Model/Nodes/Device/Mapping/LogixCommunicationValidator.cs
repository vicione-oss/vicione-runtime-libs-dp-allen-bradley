using FluentValidation;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using ViciOne.Suite.DataPort.Extensions.Outgoing.QueueProcessing;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Device.Mapping;

/// <summary>
/// Rejects a device configuration the client stack could not be built from, before anything opens a
/// socket. It checks only what is decidable here — a controller that answers to the gateway and path is
/// the connect's business, not this class's.
/// </summary>
/// <remarks>
/// One rule depends on the device node type: a chassis controller's CPU sits in whichever slot the
/// chassis was built with, so a ControlLogix has to declare a path and is held to it. A CompactLogix is
/// never asked for one, and <see cref="DeviceNodeMapper"/> supplies
/// <see cref="DataPort.Device.Path.VirtualBackplane"/>.
/// </remarks>
public sealed class LogixCommunicationValidator : AbstractValidator<LogixCommunication>
{
    public LogixCommunicationValidator()
    {
        RuleFor(static communication => communication.Gateway)
            .NotEmpty()
            .WithMessage("Gateway must be the IP address or host name of the controller.");

        RuleFor(static communication => communication.Path)
            .NotEmpty()
            .When(static communication =>
                DeviceNode.ControllerFamilyOf(communication.DesignId) is LogixControllerFamily.ControlLogix)
            .WithMessage("Path must be a CIP routing path to the CPU, e.g. \"1,0\".");

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
