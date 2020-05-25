using MediatR;
using NBB.Contracts.Domain.ContractAggregate.DomainEvents;
using NBB.Contracts.ReadModel;
using NBB.Data.Abstractions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

        public async Task Handle(ContractCreated notification, CancellationToken cancellationToken)
        {

            var c = await _contractReadModelRepository.GetByIdAsync(notification.ContractId, cancellationToken);
            if (c == null)
            {
                await _contractReadModelRepository.AddAsync(new ContractReadModel(notification.ContractId, notification.ClientId, 0), cancellationToken);
                await _contractReadModelRepository.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task Handle(ContractAmountUpdated notification, CancellationToken cancellationToken)
        {
            var e = await _contractReadModelRepository.GetByIdAsync(notification.ContractId, cancellationToken);

            //if(e == null)
            //    throw new Exception("Could not find entity in readModel");

            if (e != null)
            {
                e.Amount = notification.NewAmount;
                e.Version += 1;

                await _contractReadModelRepository.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task Handle(ContractLineAdded notification, CancellationToken cancellationToken)
        {
            var e = await _contractReadModelRepository.GetByIdAsync(notification.ContractId, cancellationToken, nameof(ContractReadModel.ContractLines));

            if (e != null && e.ContractLines.All(cl => cl.ContractLineId != notification.ContractLineId))
            {
                var contractLine = new ContractLineReadModel(notification.ContractLineId, notification.Product, notification.Price, notification.Quantity, notification.ContractId);
                e.ContractLines.Add(contractLine);
                e.Version += 1;

                await _contractReadModelRepository.SaveChangesAsync(cancellationToken);
            }

        }

        public async Task Handle(ContractValidated notification, CancellationToken cancellationToken)
        {
            var contract = await _contractReadModelRepository.GetByIdAsync(notification.ContractId, cancellationToken);

            //if(e == null)
            //    throw new Exception("Could not find entity in readModel");

            if (contract != null)
            {
                contract.IsValidated = true;
                contract.Version += 1;
                await _contractReadModelRepository.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
