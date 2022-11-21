// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.Extensions.Configuration;
using Moq;
using Serilog.Core;
using Serilog.Events;
using Serilog.Parsing;
using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit;

namespace NBB.Tools.Serilog.Enrichers.ServiceIdentifier.Tests
{
    public class ServiceIdentifierEnricherTests
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
                  new MessageTemplate("atemplate", new List<MessageTemplateToken>()),
                  new List<LogEventProperty>
                  {
                                    new LogEventProperty("aname1", new ScalarValue("avalue")),
                                    new LogEventProperty("aname2", new ScalarValue(42))
                  });
            return logEvent;
        }



        [Fact]
        public void Service_identifier_enricher_from_messaging_source()
        {
            var propertyFactory = GetPropertyFactory();
            var service = "WorkerService";

            var logEvent = GetLogEvent();
            var myConfiguration = new Dictionary<string, string>
            {
                {"Messaging:Source", service},
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration)
                .Build();

            var enricher = new ServiceIdentifierEnricher(configuration);

            enricher.Enrich(logEvent, propertyFactory);

            Assert.Equal(3, logEvent.Properties.Count);
            var value = logEvent.Properties[ServiceIdentifierEnricher.PropertyName] as ScalarValue;
            Assert.NotNull(value);
            Assert.Equal(service, value.Value);
        }

        [Fact]
        public void Service_identifier_enricher_from_default()
        {
            var propertyFactory = GetPropertyFactory();
            var expectedName = Assembly.GetEntryAssembly().GetName().Name;

            var logEvent = GetLogEvent();
            var myConfiguration = new Dictionary<string, string> { };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration)
                .Build();

            var enricher = new ServiceIdentifierEnricher(configuration);

            enricher.Enrich(logEvent, propertyFactory);

            Assert.Equal(3, logEvent.Properties.Count);
            var value = logEvent.Properties[ServiceIdentifierEnricher.PropertyName] as ScalarValue;
            Assert.NotNull(value);
            Assert.Equal(expectedName, value.Value);
        }
    }
}
