// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Moq;
using NBB.MultiTenancy.Abstractions;
using NBB.MultiTenancy.Abstractions.Context;
using NBB.Tools.Serilog.Enrichers.TenantId;
using Serilog.Core;
using Serilog.Events;
using Serilog.Parsing;
using System;
using System.Collections.Generic;
using Xunit;

namespace NBB.MultiTenancy.Serilog.Tests
{
    public class TenantIdEnricherTests
    {
        private static ILogEventPropertyFactory GetPropertyFactory()
        {
            var propertyFactory = new Mock<ILogEventPropertyFactory>();
            propertyFactory.Setup(x => x.CreateProperty(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<bool>())).Returns<string, object, bool>(
                (name, value, destructureObjects) =>
                {
                    var property = new LogEventProperty(name, new ScalarValue(value));
                    return property;
                });
            return propertyFactory.Object;
        }

        private static LogEvent GetLogEvent()
        {
            var logEvent = new LogEvent(DateTimeOffset.UtcNow, LogEventLevel.Verbose, null,
                  new MessageTemplate("mesagetemplate", new List<MessageTemplateToken>()),
                  new List<LogEventProperty>
                  {
                                    new LogEventProperty("prop1", new ScalarValue("val1")),
                                    new LogEventProperty("prop2", new ScalarValue(22))
                  });
            return logEvent;
        }

        [Fact]
        public void TenantId_enricher_correct_value_from_multitenant_context()
        {
            var tid = Guid.Parse("68a448a2-e7d8-4875-8127-f18668217eb6");

            var propertyFactory = GetPropertyFactory();

            var logEvent = GetLogEvent();

            var tenantContextAccessor = new TenantContextAccessor
            {
                TenantContext = new TenantContext(new Tenant(tid, tid.ToString()))
            };

            var enricher = new TenantEnricher(tenantContextAccessor);

            enricher.Enrich(logEvent, propertyFactory);

            Assert.Equal(3, logEvent.Properties.Count);
            Assert.Equal(tid.ToString(), logEvent.Properties[TenantEnricher.PropertyName].ToString());
        }

        [Fact]
        public void TenantId_enricher_correct_value_from_monotenant_context()
        {
            var tid = Tenant.Default.TenantId;

            var propertyFactory = GetPropertyFactory();

            var logEvent = GetLogEvent();

            var tenantContextAccessor = new TenantContextAccessor
            {
                TenantContext = new TenantContext(new Tenant(tid, tid.ToString()))
            };

            var enricher = new TenantEnricher(tenantContextAccessor);

            enricher.Enrich(logEvent, propertyFactory);

            Assert.Equal(3, logEvent.Properties.Count);
            Assert.Equal(tid.ToString(), logEvent.Properties[TenantEnricher.PropertyName].ToString());
        }

        [Fact]
        public void TenantId_enricher_monotenant_correct_value_null_context()
        {
            var tid = Tenant.Default.TenantId;

            var propertyFactory = GetPropertyFactory();

            var logEvent = GetLogEvent();

            var enricher = new TenantEnricher(new TenantContextAccessor());

            enricher.Enrich(logEvent, propertyFactory);

            Assert.Equal(3, logEvent.Properties.Count);
            Assert.Equal(tid.ToString(), logEvent.Properties[TenantEnricher.PropertyName].ToString());
        }

        [Fact]
        public void TenantId_enricher_correct_value_null_context()
        {
            var propertyFactory = GetPropertyFactory();

            var logEvent = GetLogEvent();

            var enricher = new TenantEnricher(new TenantContextAccessor());

            enricher.Enrich(logEvent, propertyFactory);

            Assert.Equal(3, logEvent.Properties.Count);
            Assert.Equal(Tenant.Default.TenantId.ToString(), logEvent.Properties[TenantEnricher.PropertyName].ToString());
        }

        [Fact]
        public void TenantId_enricher_value_with_no_tenant_default_value()
        {
            var propertyFactory = GetPropertyFactory();

            var logEvent = GetLogEvent();

            var enricher = new TenantEnricher(new TenantContextAccessor {  TenantContext = new TenantContext(null)});

            enricher.Enrich(logEvent, propertyFactory);

            Assert.Equal(3, logEvent.Properties.Count);
            Assert.Equal(Tenant.Default.TenantId.ToString(), logEvent.Properties[TenantEnricher.PropertyName].ToString());
        }
    }
}
