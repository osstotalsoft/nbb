using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NBB.Contracts.Domain.ContractAggregate.DomainEvents;
using NBB.Contracts.ReadModel;
using NBB.Data.Abstractions;

namespace NBB.Contracts.Application.DomainEventHandlers
{
    public class ReadModelGenerator :
        INotificationHandler<ContractCreated>,
        INotificationHandler<ContractAmountUpdated>,
        INotificationHandler<ContractLineAdded>,
        INotificationHandler<ContractValidated>
    {
        private readonly ICrudRepository<ContractReadModel> _contractReadModelRepository;

        public ReadModelGenerator(ICrudRepository<ContractReadModel> contractReadModelRepository)
        {
            _contractReadModelRepository = contractReadModelRepository;
        }

        public async Task Handle(ContractCreated @event, CancellationToken cancellationToken)
        {

            var c = await _contractReadModelRepository.GetByIdAsync(@event.ContractId);
            if (c == null)
            {
                await _contractReadModelRepository.AddAsync(new ContractReadModel(@event.ContractId, @event.ClientId, @event.Metadata?.SequenceNumber ?? default(int)));
                await _contractReadModelRepository.SaveChangesAsync();
            }
        }

        public async Task Handle(ContractAmountUpdated @event, CancellationToken cancellationToken)
        {
            var e = await _contractReadModelRepository.GetByIdAsync(@event.ContractId);

            //if(e == null)
            //    throw new Exception("Could not find entity in readModel");

            if (e != null)
            {
                e.Amount = @event.NewAmount;
                e.Version = @event.Metadata?.SequenceNumber ?? default(int);

                await _contractReadModelRepository.SaveChangesAsync();

            }
        }

        public async Task Handle(ContractLineAdded @event, CancellationToken cancellationToken)
        {
            var e = await _contractReadModelRepository.GetByIdAsync(@event.ContractId, nameof(ContractReadModel.ContractLines));

            if (e != null)
            {
                if (e.ContractLines.All(cl => cl.ContractLineId != @event.ContractLineId))
                {
                    var contractLine = new ContractLineReadModel(@event.ContractLineId, @event.Product, @event.Price, @event.Quantity, @event.ContractId);
                    e.ContractLines.Add(contractLine);
                    e.Version = @event.Metadata?.SequenceNumber ?? default(int);

                    await _contractReadModelRepository.SaveChangesAsync();
                }
            }
        }

        public async Task Handle(ContractValidated @event, CancellationToken cancellationToken)
        {
            var contract = await _contractReadModelRepository.GetByIdAsync(@event.ContractId);

            //if(e == null)
            //    throw new Exception("Could not find entity in readModel");

            if (contract != null)
            {
                contract.IsValidated = true;
                contract.Version = @event.Metadata?.SequenceNumber ?? default(int);
                await _contractReadModelRepository.SaveChangesAsync();
            }
        }
    }
}
