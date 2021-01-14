using MediatR;

namespace ProcessManagerSample.Commands
{
    public record DoPayment : IRequest;
    public record ShipOrder : IRequest;
}
