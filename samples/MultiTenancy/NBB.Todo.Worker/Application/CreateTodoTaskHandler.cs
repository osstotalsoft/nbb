using MediatR;
using NBB.Data.Abstractions;
using NBB.Todo.Data.Entities;
using NBB.Todo.PublishedLanguage;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Todo.Worker.Application
{
    public class CreateTodoTaskHandler : IRequestHandler<CreateTodoTask>
    {
        private readonly ICrudRepository<TodoTask> _todoTaskRepository;

        public CreateTodoTaskHandler(ICrudRepository<TodoTask> todoTaskRepository)
        {
            _todoTaskRepository = todoTaskRepository;
        }

        public async Task<Unit> Handle(CreateTodoTask request, CancellationToken cancellationToken)
        {
            var todoTask = new TodoTask
            {
                Name = request.Name,
                Description = request.Description,
                DueDate = request.DueDate
            };

            await _todoTaskRepository.AddAsync(todoTask, cancellationToken);

            await _todoTaskRepository.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
