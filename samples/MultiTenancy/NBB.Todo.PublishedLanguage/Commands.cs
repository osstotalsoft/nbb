using MediatR;
using System;

namespace NBB.Todo.PublishedLanguage
{
    public record CreateTodoTask(string Name, string Description, DateTime DueDate) : IRequest;

    public record MarkTodoTaskAscompleted(Guid TodoTaskId) : IRequest;
}
