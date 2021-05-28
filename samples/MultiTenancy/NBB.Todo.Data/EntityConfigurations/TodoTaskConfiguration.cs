using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NBB.Todo.Data.Entities;

namespace NBB.Todo.Data.EntityConfigurations
{
    public class TodoTaskConfiguration: IEntityTypeConfiguration<TodoTask>
    {
        public void Configure(EntityTypeBuilder<TodoTask> builder)
        {
            builder
                .ToTable("TodoTasks")
                .HasKey(x => x.TodoTaskId);
        }
    }
}
