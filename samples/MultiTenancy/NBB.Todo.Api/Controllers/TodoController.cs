using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NBB.Messaging.Abstractions;
using NBB.Todo.Data;
using NBB.Todo.Data.Entities;
using NBB.Todo.PublishedLanguage;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Todo.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TodoController : ControllerBase
    {
        private readonly ILogger<TodoController> _logger;
        private readonly TodoDbContext _todoDbContext;
        private readonly IMessageBus _messageBus;

        public TodoController(ILogger<TodoController> logger, TodoDbContext todoDbContext, IMessageBus messageBus)
        {
            _logger = logger;
            _todoDbContext = todoDbContext;
            _messageBus = messageBus;
        }

        [HttpGet]
        public IEnumerable<TodoTask> Get() 
            => _todoDbContext.TodoTasks.ToList();

        [HttpPost]
        public async Task Post([FromBody]CreateTodoTask command, CancellationToken cancellationToken) 
            => await _messageBus.PublishAsync(command, cancellationToken);
    }
}
