// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.Extensions.DependencyInjection;
using NBB.Contracts.Domain.ContractAggregate;

namespace NBB.Contracts.WriteModel.Data
{
    public static class DependencyInjectionExtensions
    {
        public static void AddContractsWriteModelDataAccess(this IServiceCollection services)
        {
            services.AddEventSourcingDataAccess()
                .AddEventSourcedRepository<Contract>();
        }
    }
}
