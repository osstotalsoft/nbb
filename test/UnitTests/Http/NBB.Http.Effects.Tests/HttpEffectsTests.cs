// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NBB.Core.Effects;
using Xunit;

namespace NBB.Http.Effects.Tests
{
    public class HttpEffectsTests
    {
        [Fact]
        public void AddHttpEffects_should_register_HttpGet_SideEffectHandler()
        {
            //Arrange
            var services = new ServiceCollection();

            //Act
            services.AddHttpEffects();

            //Assert
            using var container = services.BuildServiceProvider();
            var handler = container.GetService(typeof(ISideEffectHandler<HttpGet.SideEffect, HttpResponseMessage>));
            handler.Should().NotBeNull();
        }


        [Fact]
        public async Task HttpGet_SideEffectHandler_should_perform_a_http_get()
        {
            //Arrange
            var url = "http://test.com";
            var httpHandler = new MockHttpMessageHandler();
            var httpClient = new HttpClient(httpHandler);
            var httpClientFactory = new Mock<IHttpClientFactory>();
            httpClientFactory.Setup(mock => mock.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var sideEffect = new HttpGet.SideEffect(url);

            //Act
            var sut = new HttpGet.Handler(httpClientFactory.Object);
            await sut.Handle(sideEffect);

            //Assert
            httpHandler.Verify(m=> m.Method == HttpMethod.Get && m.RequestUri.OriginalString == url);
        }
    }

    public class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly List<HttpRequestMessage> _requests = new();

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            _requests.Add(request);

            return Task.FromResult(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("my string, that needs to be returned")
            });
        }

        public void Verify(Predicate<HttpRequestMessage> predicate)
        {
            Assert.Contains(_requests, predicate);
        }
    }
}
