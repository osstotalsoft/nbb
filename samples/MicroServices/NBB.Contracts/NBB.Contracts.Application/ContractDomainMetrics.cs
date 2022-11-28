// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using System.Diagnostics.Metrics;
using System.Reflection;

namespace NBB.Contracts.Application
{
    public class ContractDomainMetrics : IDisposable
    {
        private static readonly AssemblyName AssemblyName = Assembly.GetCallingAssembly().GetName();
        public static readonly string InstrumentationName = AssemblyName.Name;
        private readonly Counter<int> _contracts;
        private readonly Counter<int> _validatedContracts;
        private readonly Meter _meter;

        public ContractDomainMetrics()
        {
            _meter = new Meter(AssemblyName.Name, AssemblyName.Version.ToString());
            _contracts = _meter.CreateCounter<int>("nbb.contracts.created.count");
            _validatedContracts = _meter.CreateCounter<int>("nbb.contracts.validated.count");

            _contracts.Add(1);
            _contracts.Add(1);

        }

        public void ContractCreated()
        {
            _contracts.Add(1);
        }
        public void ContractValidated()
        {
            _validatedContracts.Add(1);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _meter?.Dispose();
        }
    }
}
