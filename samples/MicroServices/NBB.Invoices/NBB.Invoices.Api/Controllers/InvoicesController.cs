using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NBB.Invoices.Application.Commands;
using NBB.Invoices.Domain.InvoiceAggregate;
using NBB.Messaging.Abstractions;

namespace NBB.Invoices.Api.Controllers
{
    [Route("api/[controller]")]
    public class InvoicesController : Controller
    {
        private readonly IMessageBusPublisher _messageBusPublisher;
        private readonly IQueryable<Invoice> _invoiceQuery;

        public InvoicesController(IMessageBusPublisher messageBusPublisher, IQueryable<Invoice> invoiceQuery)
        {
            _messageBusPublisher = messageBusPublisher;
            _invoiceQuery = invoiceQuery;
        }


        // GET api/invoices
        [HttpGet]
        public Task<List<Invoice>> Get()
        {
            return _invoiceQuery.ToListAsync();
        }

        // GET api/invoices/7327223E-22EA-48DC-BC44-FFF6AB3B9489
        [HttpGet("{InvoiceId}")]
        public async Task<IActionResult> Get(Guid invoiceId, CancellationToken cancellationToken)
        {
            //var result = await _messageBusMediator.Send<GetInvoice.Model>(query, cancellationToken);
            var result = await _invoiceQuery.SingleAsync(x => x.InvoiceId == invoiceId, cancellationToken);
            return Ok(result);
        }

        // POST api/invoices
        [HttpPost]
        public Task Post([FromBody]CreateInvoice command, CancellationToken cancellationToken)
        {
            return _messageBusPublisher.PublishAsync(command, cancellationToken);
        }

    }
}
