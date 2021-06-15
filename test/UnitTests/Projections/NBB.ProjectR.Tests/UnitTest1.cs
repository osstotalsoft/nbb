using System;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace NBB.ProjectR.Tests
{
    public class UnitTest1
    {
        [Fact]
        public async Task Test1()
        {
            //Arrange
            var services = new ServiceCollection();
            services.AddProjectR(GetType().Assembly);
            await using var container = services.BuildServiceProvider();
            using var scope = container.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var contractId = Guid.NewGuid();
            
            //Act
            await mediator.Publish(new ContractCreated(contractId, 100));

            //Assert
        }
    }
}
