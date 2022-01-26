// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NBB.MultiTenancy.Abstractions.Context;
using System;
using System.Collections.Generic;
using NBB.MultiTenancy.Abstractions.Configuration;

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
            services.TryAddSingleton(_serviceProvider.GetService<ITenantContextAccessor>());
            services.TryAddSingleton(_serviceProvider.GetService<ITenantConfiguration>());
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

        public override int GetServiceProviderHashCode() => 0;

        public override void PopulateDebugInfo([NotNull] IDictionary<string, string> debugInfo)
        {

        }

        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other)
            => other is MultiTenantDbContextExtensionInfo;
    }
}
