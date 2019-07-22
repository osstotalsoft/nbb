using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NBB.Contracts.Application.Commands;
using NBB.Contracts.Application.Queries;
using NBB.Correlation;

namespace NBB.Contracts.Api.Controllers
{
    [Route("api/[controller]")]
    public class ContractsController : Controller
    {
        private readonly IMediator _mediator;

        public ContractsController(IMediator mediator)
        {
            _mediator = mediator;
        }


        // GET api/contracts
        [HttpGet]
        public async Task<IActionResult> Get([FromRoute] GetContracts.Query query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        // GET api/contracts/7327223E-22EA-48DC-BC44-FFF6AB3B9489
        [HttpGet("{id}")]
        public async Task<IActionResult> Get([FromRoute] GetContractById.Query query)
        {
            var contract = await _mediator.Send(query);

            if (contract != null)
                return Ok(contract);

            return NotFound();
        }

        // POST api/contracts
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]CreateContract command, CancellationToken cancellationToken)
        {
            await _mediator.Send(command, cancellationToken);
            return Ok(new {command.Metadata.CommandId, CorrelationId = CorrelationManager.GetCorrelationId()});
        }

        // POST api/contracts/7327223E-22EA-48DC-BC44-FFF6AB3B9489/lines
        [HttpPost("{id}/lines")]
        public async Task<IActionResult> Post([FromBody]AddContractLine command, CancellationToken cancellationToken)
        {
            await _mediator.Send(command, cancellationToken);
            return Ok(new {command.Metadata.CommandId, CorrelationId = CorrelationManager.GetCorrelationId()});
        }

        // POST api/contracts/7327223E-22EA-48DC-BC44-FFF6AB3B9489/validate
        [HttpPost("{id}/validate")]
        public async Task<IActionResult> Post([FromBody]ValidateContract command, CancellationToken cancellationToken)
        {
            await _mediator.Send(command, cancellationToken);
            return Ok(new {command.Metadata.CommandId, CorrelationId = CorrelationManager.GetCorrelationId()});
        }

    }
}
