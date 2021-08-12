// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using MediatR;

namespace NBB.ProcessManager.Tests.Commands
{
    public record DoPayment : IRequest;
    public record ShipOrder(Guid OrderId, decimal Amount, string ShippingAddress) : IRequest;
}
