// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Mediator;

namespace ProcessManagerSample.Commands
{
    public record DoPayment : IRequest;
    public record ShipOrder : IRequest;
}
