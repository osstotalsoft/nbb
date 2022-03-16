// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using FluentAssertions;
using NBB.MultiTenancy.Abstractions.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NBB.MultiTenancy.Abstractions.Tests
{
    public class TenantContextAccessorTests
    {
        [Fact]
        public void should_change_tenant_context_when_null()
        {
            //arrange
            var tenant = new Tenant();
            var sut = new TenantContextAccessor();

            //act
            var tf = sut.ChangeTenantContext(tenant);

            //assert
            sut.TenantContext.Tenant.Should().Be(tenant);
            tf.Dispose();
            sut.TenantContext.Should().BeNull();
        }
    }
}
