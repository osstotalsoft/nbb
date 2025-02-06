// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NBB.Data.EntityFramework.MultiTenancy.Tests
{
    public class SimpleEntityConfiguration : IEntityTypeConfiguration<SimpleEntity>
    {
        public void Configure(EntityTypeBuilder<SimpleEntity> builder)
        {
            builder.HasKey(x => x.Id);
        }
    }
}
