using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NBB.Core.Abstractions;
using NBB.Invoices.Application.Commands;
using NBB.Invoices.Domain.InvoiceAggregate;
using NBB.Invoices.PublishedLanguage.IntegrationQueries;
using NBB.Messaging.Abstractions;

namespace NBB.Invoices.Api.Controllers
{
    [Route("api/[controller]")]
    public class InvoicesController : Controller
    {
        private readonly IMessageBusPublisher _messageBusPublisher;
        private readonly IQueryable<Invoice> _invoiceQuery;
        private readonly IMessageBusMediator _messageBusMediator;

        public InvoicesController(IMessageBusPublisher messageBusPublisher, IQueryable<Invoice> invoiceQuery, IMessageBusMediator messageBusMediator)
        {
            _messageBusPublisher = messageBusPublisher;
            _invoiceQuery = invoiceQuery;
            _messageBusMediator = messageBusMediator;
        }


        // GET api/invoices
        [HttpGet]
        public Task<List<Invoice>> Get()
        {
            return _invoiceQuery.ToListAsync();
        }

        // GET api/invoices/7327223E-22EA-48DC-BC44-FFF6AB3B9489
        [HttpGet("{InvoiceId}")]
        public async Task<IActionResult> Get(GetInvoice.Query query, CancellationToken cancellationToken)
        {
            var result = await _messageBusMediator.Send<GetInvoice.Model>(query, cancellationToken);
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
