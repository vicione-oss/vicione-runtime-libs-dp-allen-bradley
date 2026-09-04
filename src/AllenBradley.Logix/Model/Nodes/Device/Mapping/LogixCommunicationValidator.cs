using FluentValidation;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using ViciOne.Suite.DataPort.Extensions.Outgoing.QueueProcessing;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Device.Mapping;

/// <summary>
/// Rejects a device configuration the client stack could not be built from, before anything opens a
/// socket. It checks only what is decidable here — a controller that answers at the connection endpoint and CIP route
/// path is the connect's business, not this class's.
/// A ControlLogix has to declare a route path; a CompactLogix is never asked for one and gets <see
/// cref="DataPort.Device.CipRoutePath.VirtualBackplane"/> from <see cref="DeviceNodeMapper"/>.
/// </summary>
public sealed class LogixCommunicationValidator : AbstractValidator<LogixCommunication>
{
    public LogixCommunicationValidator()
    {
        RuleFor(static communication => communication.ConnectionEndpoint)
            .NotEmpty()
            .WithMessage("Connection endpoint must be the IP address or host name of the controller.");

        RuleFor(static communication => communication.CipRoutePath)
            .NotEmpty()
            .When(static communication =>
                DeviceNode.KindOf(communication.DesignId)?.Family is LogixControllerFamily.ControlLogix)
            .WithMessage("CIP route path must be the sequence of hops to the CPU, e.g. \"1,0\".");

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
