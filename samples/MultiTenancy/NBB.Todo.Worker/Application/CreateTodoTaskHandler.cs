using MediatR;
using NBB.Todo.Data;
using NBB.Todo.Data.Entities;
using NBB.Todo.PublishedLanguage;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Todo.Worker.Application
{
    public class CreateTodoTaskHandler : IRequestHandler<CreateTodoTask>
    {
        private readonly TodoDbContext _todoDbContext;

        public CreateTodoTaskHandler(TodoDbContext todoDbContext)
        {
            _todoDbContext = todoDbContext;
        }

        public async Task<Unit> Handle(CreateTodoTask request, CancellationToken cancellationToken)
        {
            var todoTask = new TodoTask
            {
                Name = request.Name,
                Description = request.Description,
                DueDate = request.DueDate
            };

            _todoDbContext.Add(todoTask);
            await _todoDbContext.SaveChangesAsync();

            return Unit.Value;
        }
    }
}
