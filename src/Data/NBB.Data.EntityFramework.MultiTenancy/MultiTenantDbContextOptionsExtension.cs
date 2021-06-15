﻿using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NBB.MultiTenancy.Abstractions.Context;
using System;
using System.Collections.Generic;

namespace NBB.Data.EntityFramework.MultiTenancy
{
    internal class MultiTenantOptionsExtension : IDbContextOptionsExtension
    {
        private readonly IServiceProvider _serviceProvider;
        public MultiTenantOptionsExtension(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public DbContextOptionsExtensionInfo Info => new MultiTenantDbContextExtensionInfo(this);

        public void ApplyServices(IServiceCollection services)
        {
            services.TryAddScoped(_ => _serviceProvider.GetService<ITenantContextAccessor>());
            services.TryAddScoped(_ => _serviceProvider.GetService<ITenantDatabaseConfigService>());
        }

        public void Validate(IDbContextOptions options)
        {
        }
    }

    internal class MultiTenantDbContextExtensionInfo : DbContextOptionsExtensionInfo
    {
        public MultiTenantDbContextExtensionInfo(IDbContextOptionsExtension extension) : base(extension)
        {

        }

        public override bool IsDatabaseProvider => false;

        public override string LogFragment => string.Empty;

        public override long GetServiceProviderHashCode()
        {
            return 0;
        }

        public override void PopulateDebugInfo([NotNull] IDictionary<string, string> debugInfo)
        {

        }
    }
}
