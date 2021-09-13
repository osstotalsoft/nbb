// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Mvc.Testing;

namespace NBB.Messaging.Rusi.Tests
{
    public sealed class TestServerFixture : IDisposable
    {
        private readonly WebApplicationFactory<Startup> _factory;

        public TestServerFixture()
        {
            _factory = new WebApplicationFactory<Startup>();
            var client = _factory.CreateDefaultClient();
            GrpcChannel = GrpcChannel.ForAddress(client.BaseAddress, new GrpcChannelOptions
            {
                HttpClient = client
            });
        }

        public GrpcChannel GrpcChannel { get; }

        public void Dispose()
        {
            _factory.Dispose();
        }

      
    }
}
