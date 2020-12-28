using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Xunit;

namespace NBB.Core.Effects.Tests
{
    public class ThunkSideEffectTests
    {
        [Fact]
        public async Task Interpret_Thunk_SideEffect()
        {
            //Arrange
            var services = new ServiceCollection();
            services.AddEffects();
            await using var container = services.BuildServiceProvider();
            var interpreter = container.GetRequiredService<IInterpreter>();

            
            var sideEffectExecuted = false;
            var eff = Effect.From(() =>
            {
                Console.WriteLine("Effect executed");
                sideEffectExecuted = true;
            });
            //Act
            sideEffectExecuted.Should().Be(false);
            var actual = await interpreter.Interpret(eff);

            //Assert
            sideEffectExecuted.Should().Be(true);
            actual.Should().Be(Unit.Value);
        }
    }
}
