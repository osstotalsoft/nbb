using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NBB.Messaging.Abstractions;
using NBB.Payments.Application.Commands;
using NBB.Payments.Domain.PayableAggregate;

namespace NBB.Payments.Api.Controllers
{
    [Route("api/[controller]")]
    public class PayablesController : Controller
    {
        private readonly IMessageBusPublisher _messageBusPublisher;
        private readonly IQueryable<Payable> _payableQuery;

        public PayablesController(IMessageBusPublisher messageBusPublisher, IQueryable<Payable> payableQuery)
        {
            _messageBusPublisher = messageBusPublisher;
            _payableQuery = payableQuery;
        }

        // GET api/payables
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var payables = await _payableQuery.ToListAsync();
            return Ok(payables);
        }

        // GET api/payables/7327223E-22EA-48DC-BC44-FFF6AB3B9489
        [HttpGet("{id}")]
        public Task<Payable> Get(Guid id)
        {
            return _payableQuery.FirstOrDefaultAsync(x=> x.PayableId == id);
        }

        // POSt api/payables/6AF6F8C8-117C-45C0-BB88-F49C13B8DE8D/pay
        [HttpPost("{id}/pay")]
        public Task Pay([FromBody]PayPayable command, CancellationToken cancellationToken)
        {
            return _messageBusPublisher.PublishAsync(command, cancellationToken);
        }
    }
}
