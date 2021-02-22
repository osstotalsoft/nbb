using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace NBB.Core.Pipeline.Tests
{
    public class PipelineTests
    {
        [Fact]
        public async void Should_execute_nesting_in_order_class_middleware()
        {
            //Arrange
            var mockedServiceProvider = Mock.Of<IServiceProvider>();
            var executePipeline = new PipelineBuilder<IContext>()
                .UseMiddleware<FirstMiddleware, IContext>()
                .UseMiddleware<SecondMiddleware, IContext>()
                .Pipeline;

            var sentContext = new TestContext(mockedServiceProvider);

            //Act
            await executePipeline(sentContext, default);

            //Assert
            sentContext.Log.Should().Equal(new[] { "FirstBefore", "SecondBefore", "SecondAfter", "FirstAfter" });
        }

        [Fact]
        public async void Should_execute_nesting_in_order_inline()
        {
            //Arrange
            var mockedServiceProvider = Mock.Of<IServiceProvider>();
            var sentContext = new TestContext(mockedServiceProvider);
            var pipeline = new PipelineBuilder<IContext>()
                .Use(async (data, cancellationToken, next) =>
                {
                    (data as TestContext)?.Log.Add("FirstBefore");
                    await next();
                    Thread.Sleep(100);
                    (data as TestContext)?.Log.Add("FirstAfter");
                })
                .Use(async (data, cancellationToken, next) =>
                {
                    (data as TestContext)?.Log.Add("SecondBefore");
                    await next();
                    (data as TestContext)?.Log.Add("SecondAfter");
                })
                .Pipeline;


            //Act
            await pipeline(sentContext, default);

            //Assert
            sentContext.Log.Should().Equal(new[] { "FirstBefore", "SecondBefore", "SecondAfter", "FirstAfter" });
        }



        [Fact]
        public async void Should_support_constructor_injection_in_middleware()
        {
            //Arrange
            var mockedServiceProvider = Mock.Of<IServiceProvider>(sp =>
                sp.GetService(typeof(ILogger<MiddlewareWithDI>)) == Mock.Of<ILogger<MiddlewareWithDI>>());

            var pipeline = new PipelineBuilder<IContext>()
                .UseMiddleware<MiddlewareWithDI, IContext>()
                .Pipeline;

            var sentContext = new TestContext(mockedServiceProvider);

            //Act
            await pipeline(sentContext, default);

            //Assert
            Mock.Get(mockedServiceProvider).Verify(x => x.GetService(typeof(ILogger<MiddlewareWithDI>)));
        }

        [Fact]
        public async void Should_pass_execution_parameters_to_middleware()
        {
            //Arrange
            var mockedServiceProvider = Mock.Of<IServiceProvider>();

            IContext receivedData1 = null, receivedData2 = null;
            CancellationToken receivedCancellationToken1 = default, receivedCancellationToken2 = default;

            var pipeline = new PipelineBuilder<IContext>()
                .Use(async (data, cancellationToken, next) =>
                {
                    receivedData1 = data;
                    receivedCancellationToken1 = cancellationToken;
                    await next();
                })
                 .Use(async (data, cancellationToken, next) =>
                 {
                     receivedData2 = data;
                     receivedCancellationToken2 = cancellationToken;
                     await Task.Yield();
                 })
                .Pipeline;

            var sentContext = Mock.Of<IContext>(x => x.Services == mockedServiceProvider);
            using (var ts = new CancellationTokenSource())
            {
                var sentCancellationToken = ts.Token;

                //Act
                await pipeline(sentContext, sentCancellationToken);

                //Assert
                receivedCancellationToken1.Should().Be(sentCancellationToken);
                receivedCancellationToken2.Should().Be(sentCancellationToken);
                receivedData1.Should().Be(sentContext);
                receivedData2.Should().Be(sentContext);
            }
        }

        [Fact]
        public void Should_not_swallow_exception_async()
        {
            //Arrange
            var mockedServiceProvider = Mock.Of<IServiceProvider>();
            bool hasMiddleware1Finished = false;

            var pipeline = new PipelineBuilder<IContext>()
                .Use(async (data, cancellationToken, next) =>
                {
                    await next();
                    hasMiddleware1Finished = true;
                })
                 .Use(async (data, cancellationToken, next) =>
                 {
                     await Task.Yield();
                     throw new ApplicationException();
                 })
                .Pipeline;

            var sentContext = Mock.Of<IContext>(x => x.Services == mockedServiceProvider);

            //Act
            async Task Action() => await pipeline(sentContext, default);

            //Assert
            ((Func<Task>)Action).Should().Throw<ApplicationException>();
            hasMiddleware1Finished.Should().BeFalse();
        }

        [Fact]
        public void Should_not_burry_exception_sync()
        {
            //Arrange
            var mockedServiceProvider = Mock.Of<IServiceProvider>();
            bool hasMiddleware1Finished = false;

            var pipeline = new PipelineBuilder<IContext>()
                .Use((data, cancellationToken, next) =>
                {
                    var nextTask = next();

                    hasMiddleware1Finished = true;
                    return nextTask;
                })
                .Use((data, cancellationToken, next) => throw new ApplicationException())
                .Pipeline;

            var sentContext = Mock.Of<IContext>(x => x.Services == mockedServiceProvider);

            //Act
            void Action() => pipeline(sentContext, default).Wait();

            //Assert
            
            ((Action)Action).Should().Throw<ApplicationException>();

            hasMiddleware1Finished.Should().BeFalse();
        }

        [Fact]
        public async void Should_build_pipeline_in_container_scope()
        {
            //Arrange
            var services = new ServiceCollection();
            ILogger<MiddlewareWithDI> scopedDependency;
            services.AddScoped(_ => Mock.Of<ILogger<MiddlewareWithDI>>());
            services.AddScoped(sp => 
                new PipelineBuilder<IContext>()
                    .UseMiddleware<MiddlewareWithDI, IContext>()
                    .Pipeline
            );

            TestContext sentContext;
            var container = services.BuildServiceProvider();
            var rootDependency = container.GetService<ILogger<MiddlewareWithDI>>();

            //Act
            using (var scope = container.CreateScope())
            {
                sentContext = new TestContext(scope.ServiceProvider);
                var pipeline = scope.ServiceProvider.GetRequiredService<PipelineDelegate<IContext>>();
                await pipeline(sentContext, default);
                scopedDependency = scope.ServiceProvider.GetService<ILogger<MiddlewareWithDI>>();
            }

            //Assert
            sentContext.Data.Should().NotBe(rootDependency);
            sentContext.Data.Should().Be(scopedDependency);
        }

        // ReSharper Disable All 
        public interface IContext : IPipelineContext { }

        private class FirstMiddleware : IPipelineMiddleware<IContext>
        {
            public async Task Invoke(IContext message, CancellationToken cancellationToken, Func<Task> next)
            {
                (message as TestContext)?.Log.Add("FirstBefore");
                await next();
                (message as TestContext)?.Log.Add("FirstAfter");
            }
        }

        private class SecondMiddleware : IPipelineMiddleware<IContext>
        {
            public async Task Invoke(IContext message, CancellationToken cancellationToken, Func<Task> next)
            {
                (message as TestContext)?.Log.Add("SecondBefore");
                await next();
                (message as TestContext)?.Log.Add("SecondAfter");
            }
        }

        public class MiddlewareWithDI : IPipelineMiddleware<IContext>
        {
            private readonly ILogger<MiddlewareWithDI> _logger;

            public MiddlewareWithDI(ILogger<MiddlewareWithDI> logger)
            {
                this._logger = logger;
            }

            public async Task Invoke(IContext message, CancellationToken cancellationToken, Func<Task> next)
            {
                if (message is TestContext dataAsTestData)
                    dataAsTestData.Data = _logger;

                _logger.LogDebug("MiddlewareWithDI:Before");
                await next();
                _logger.LogDebug("MiddlewareWithDI:After");
            }
        }

        private interface IGuidProvider
        {
            Guid Value { get; set; }
        }

        private class MiddlewareWithDI2 : IPipelineMiddleware<IContext>
        {
            private readonly IGuidProvider dependency;

            public MiddlewareWithDI2(IGuidProvider dependency)
            {
                this.dependency = dependency;
            }

            public async Task Invoke(IContext message, CancellationToken cancellationToken, Func<Task> next)
            {
                await next();
            }
        }

        private class TestMiddleware : IPipelineMiddleware<IContext>
        {
            public IContext ResolvedObject { get; }

            public TestMiddleware(IContext resolvedObject)
            {
                ResolvedObject = resolvedObject;
            }

            public async Task Invoke(IContext message, CancellationToken cancellationToken, Func<Task> next)
            {
                (message as TestContext)?.Log.Add("FirstBefore");
                await next();
                (message as TestContext)?.Log.Add("FirstBefore");
            }
        }

        private class TestContext : IContext
        {
            public Guid EventId { get; private set; }
            public Guid DataId => EventId;
            public List<string> Log { get; set; }
            public Object Data { get; set; }

            private TestContext(Guid eventId)
            {
                EventId = eventId;
                Log = new List<string>();
            }

            public TestContext(IServiceProvider services)
                : this(Guid.NewGuid())
            {
                Services = services;
            }

            public IServiceProvider Services { get; }
        }

        // ReSharper Enable All 
    }
}
