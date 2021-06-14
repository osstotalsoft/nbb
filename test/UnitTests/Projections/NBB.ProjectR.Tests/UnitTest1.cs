using System;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace NBB.ProjectR.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            //Arrange
            var services = new ServiceCollection();
            services.AddProjectR(typeof(ContractProjection).Assembly);
            using var container = services.BuildServiceProvider();
            using var scope = container.CreateScope();
            var projector = scope.ServiceProvider.GetRequiredService<IProjector>();
            var contractId = Guid.NewGuid();
            //Act
            var eff = projector.Project(new ContractCreated(contractId, 100));

            //Assert
            eff.Should().NotBeNull();
        }
    }
}
