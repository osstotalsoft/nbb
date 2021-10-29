// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

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
        private readonly TodoDbContext _todoDbContext;
        private readonly IMessageBus _messageBus;

        public TodoController(TodoDbContext todoDbContext, IMessageBus messageBus)
        {
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
