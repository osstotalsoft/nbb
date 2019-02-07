using Microsoft.Extensions.DependencyInjection;
using NBB.Contracts.Domain.ContractAggregate;
using NBB.Data.Abstractions;
using NBB.Data.EventSourcing;
using NBB.Domain.Abstractions;

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
