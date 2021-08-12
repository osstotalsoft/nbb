// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NBB.Todo.Data.Entities;
using NBB.Data.EntityFramework.MultiTenancy;

namespace NBB.Todo.Data.EntityConfigurations
{
    public class TodoTaskConfiguration: IEntityTypeConfiguration<TodoTask>
    {
        public void Configure(EntityTypeBuilder<TodoTask> builder)
        {
            builder
                .ToTable("TodoTasks")
                .IsMultiTenant()
                .HasKey(x => x.TodoTaskId);
        }
    }
}
