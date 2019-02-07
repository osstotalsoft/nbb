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
            var executePipeline = new PipelineBuilder<IData>(mockedServiceProvider)
                .UseMiddleware<FirstMiddleware, IData>()
                .UseMiddleware<SecondMiddleware, IData>()
                .Pipeline;

            var sentData = new TestData();

            //Act
            await executePipeline(sentData, default(CancellationToken));

            //Assert
            sentData.Log.Should().Equal(new[] { "FirstBefore", "SecondBefore", "SecondAfter", "FirstAfter" });
        }

        [Fact]
        public async void Should_execute_nesting_in_order_inline()
        {
            //Arrange
            var mockedServiceProvider = Mock.Of<IServiceProvider>();
            var sentData = new TestData();
            var pipeline = new PipelineBuilder<IData>(mockedServiceProvider)
                .Use(async (data, cancellationToken, next) =>
                {
                    (data as TestData)?.Log.Add("FirstBefore");
                    await next();
                    Thread.Sleep(100);
                    (data as TestData)?.Log.Add("FirstAfter");
                })
                .Use(async (data, cancellationToken, next) =>
                {
                    (data as TestData)?.Log.Add("SecondBefore");
                    await next();
                    (data as TestData)?.Log.Add("SecondAfter");
                })
                .Pipeline;


            //Act
            await pipeline(sentData, default(CancellationToken));

            //Assert
            sentData.Log.Should().Equal(new[] { "FirstBefore", "SecondBefore", "SecondAfter", "FirstAfter" });
        }



        [Fact]
        public async void Should_support_constructor_injection_in_middleware()
        {
            //Arrange
            var mockedServiceProvider = Mock.Of<IServiceProvider>(sp =>
                sp.GetService(typeof(ILogger<MiddlewareWithDI>)) == Mock.Of<ILogger<MiddlewareWithDI>>());

            var pipeline = new PipelineBuilder<IData>(mockedServiceProvider)
                .UseMiddleware<MiddlewareWithDI, IData>()
                .Pipeline;

            var sentData = new TestData();

            //Act
            await pipeline(sentData, default(CancellationToken));

            //Assert
            Mock.Get(mockedServiceProvider).Verify(x => x.GetService(typeof(ILogger<MiddlewareWithDI>)));
        }

        [Fact]
        public async void Should_pass_execution_parameters_to_middleware()
        {
            //Arrange
            var mockedServiceProvider = Mock.Of<IServiceProvider>();

            IData receivedData1 = null, receivedData2 = null;
            CancellationToken receivedCancellationToken1, receivedCancellationToken2;

            var pipeline = new PipelineBuilder<IData>(mockedServiceProvider)
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

            var sentData = Mock.Of<IData>();
            using (var ts = new CancellationTokenSource())
            {
                var sentCancellationToken = ts.Token;

                //Act
                await pipeline(sentData, sentCancellationToken);

                //Assert
                receivedCancellationToken1.Should().Be(sentCancellationToken);
                receivedCancellationToken2.Should().Be(sentCancellationToken);
                receivedData1.Should().Be(sentData);
                receivedData2.Should().Be(sentData);
            }
        }

        [Fact]
        public void Should_not_swallow_exception_async()
        {
            //Arrange
            var mockedServiceProvider = Mock.Of<IServiceProvider>();
            bool hasMiddleware1Finished = false;

            var pipeline = new PipelineBuilder<IData>(mockedServiceProvider)
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

            var sentData = Mock.Of<IData>();

            //Act
            async Task Action() => await pipeline(sentData, default(CancellationToken));

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

            var pipeline = new PipelineBuilder<IData>(mockedServiceProvider)
                .Use((data, cancellationToken, next) =>
                {
                    var nextTask = next();

                    hasMiddleware1Finished = true;
                    return nextTask;
                })
                .Use((data, cancellationToken, next) => throw new ApplicationException())
                .Pipeline;

            var sentData = Mock.Of<IData>();

            //Act
            void Action() => pipeline(sentData, default(CancellationToken)).Wait();

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
                new PipelineBuilder<IData>(sp)
                    .UseMiddleware<MiddlewareWithDI, IData>()
                    .Pipeline
            );

            var sentData = new TestData();
            var container = services.BuildServiceProvider();
            var rootDependency = container.GetService<ILogger<MiddlewareWithDI>>();

            //Act
            using (var scope = container.CreateScope())
            {
                var pipeline = scope.ServiceProvider.GetService<PipelineDelegate<IData>>();
                await pipeline(sentData, default(CancellationToken));
                scopedDependency = scope.ServiceProvider.GetService<ILogger<MiddlewareWithDI>>();
            }

            //Assert
            sentData.Data.Should().NotBe(rootDependency);
            sentData.Data.Should().Be(scopedDependency);
        }

        // ReSharper Disable All 
        public interface IData { }

        private class FirstMiddleware : IPipelineMiddleware<IData>
        {
            public async Task Invoke(IData data, CancellationToken cancellationToken, Func<Task> next)
            {
                (data as TestData)?.Log.Add("FirstBefore");
                await next();
                (data as TestData)?.Log.Add("FirstAfter");
            }
        }

        private class SecondMiddleware : IPipelineMiddleware<IData>
        {
            public async Task Invoke(IData data, CancellationToken cancellationToken, Func<Task> next)
            {
                (data as TestData)?.Log.Add("SecondBefore");
                await next();
                (data as TestData)?.Log.Add("SecondAfter");
            }
        }

        public class MiddlewareWithDI : IPipelineMiddleware<IData>
        {
            private readonly ILogger<MiddlewareWithDI> _logger;

            public MiddlewareWithDI(ILogger<MiddlewareWithDI> logger)
            {
                this._logger = logger;
            }

            public async Task Invoke(IData data, CancellationToken cancellationToken, Func<Task> next)
            {
                if (data is TestData dataAsTestData)
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

        private class MiddlewareWithDI2 : IPipelineMiddleware<IData>
        {
            private readonly IGuidProvider dependency;

            public MiddlewareWithDI2(IGuidProvider dependency)
            {
                this.dependency = dependency;
            }

            public async Task Invoke(IData data, CancellationToken cancellationToken, Func<Task> next)
            {
                await next();
            }
        }

        private class TestMiddleware : IPipelineMiddleware<IData>
        {
            public IData ResolvedObject { get; }

            public TestMiddleware(IData resolvedObject)
            {
                ResolvedObject = resolvedObject;
            }

            public async Task Invoke(IData data, CancellationToken cancellationToken, Func<Task> next)
            {
                (data as TestData)?.Log.Add("FirstBefore");
                await next();
                (data as TestData)?.Log.Add("FirstBefore");
            }
        }

        private class TestData : IData
        {
            public Guid EventId { get; private set; }
            public Guid DataId => EventId;
            public List<string> Log { get; set; }
            public Object Data { get; set; }

            private TestData(Guid eventId)
            {
                EventId = eventId;
                Log = new List<string>();
            }

            public TestData()
                : this(Guid.NewGuid())
            {
            }
        }

        // ReSharper Enable All 
    }
}
