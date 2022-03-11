// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.Extensions.Configuration;
using Serilog.Core;
using Serilog.Events;
using System.Reflection;

namespace NBB.Tools.Serilog.Enrichers.ServiceIdentifier
{
    public class ServiceIdentifierEnricher : ILogEventEnricher
    {
        private readonly IConfiguration _configuration;
        public static string PropertyName { get; } = "ServiceIdentifier";

        public ServiceIdentifierEnricher(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var source = _configuration.GetSection("Messaging")?["Source"];
            if (string.IsNullOrEmpty(source))
            {
                source = Assembly.GetCallingAssembly().GetName().Name;
            }
            logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty(PropertyName, source));
        }
    }
}
