using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NBB.Contracts.PublishedLanguage;
using NBB.Contracts.ReadModel;
using NBB.Messaging.Abstractions;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Contracts.Api.Controllers
{
    [Route("api/[controller]")]
    public class ContractsController : Controller
    {
        private readonly IQueryable<ContractReadModel> _contractReadModelQuery;
        private readonly IMessageBusPublisher _messageBusPublisher;

        public ContractsController(IMessageBusPublisher messageBusPublisher, IQueryable<ContractReadModel> contractReadModelQuery)
        {
            _messageBusPublisher = messageBusPublisher;
            _contractReadModelQuery = contractReadModelQuery;
        }


        // GET api/contracts
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var query = await _contractReadModelQuery.ToListAsync();
            return Ok(query.ToList());
        }

        // GET api/contracts/7327223E-22EA-48DC-BC44-FFF6AB3B9489
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            //var contract = await _contractReadModelRepository.GetFirstOrDefaultAsync(x=> x.ContractId == id, "ContractLines");
            var contract = await _contractReadModelQuery
                .Include(x=> x.ContractLines)
                .SingleOrDefaultAsync(x => x.ContractId == id, CancellationToken.None);

            if (contract != null)
                return Ok(contract);

            return NotFound();
        }

        // POST api/contracts
        [HttpPost]
        public Task Post([FromBody]CreateContract command, CancellationToken cancellationToken)
        {
            return _messageBusPublisher.PublishAsync(command, cancellationToken);
        }

        // POST api/contracts/7327223E-22EA-48DC-BC44-FFF6AB3B9489/lines
        [HttpPost("{id}/lines")]
        public Task Post([FromBody]AddContractLine command, CancellationToken cancellationToken)
        {
            return _messageBusPublisher.PublishAsync(command, cancellationToken);
        }

        // POST api/contracts/7327223E-22EA-48DC-BC44-FFF6AB3B9489/validate
        [HttpPost("{id}/validate")]
        public Task Post([FromBody]ValidateContract command, CancellationToken cancellationToken)
        {
            return _messageBusPublisher.PublishAsync(command, cancellationToken);
        }

    }
}
