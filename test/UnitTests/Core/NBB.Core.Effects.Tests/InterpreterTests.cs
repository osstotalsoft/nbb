using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace NBB.Core.Effects.Tests
{
    public class InterpreterTests
    {

        [Fact]
        public async Task Interpret_pure_effect_should_return_inner_value()
        {
            //Arrange
            var sideEffectHandlerFactory = new Mock<ISideEffectBroker>();
            var sut = new Interpreter(sideEffectHandlerFactory.Object);
            var value = "test";
            var effect = Effect.Pure(value);

            //Act
            var result = await sut.Interpret(effect);

            //Assert
            result.Should().Be(value);
        }

        [Fact]
        public async Task Interpret_parallel_effect_should_interpret_both_effects()
        {
            //Arrange
            var services = new ServiceCollection();
            services.AddEffects();
            await using var container = services.BuildServiceProvider();
            var sut = container.GetRequiredService<IInterpreter>();

            var expectedInt = 5;
            var expectedString = "test";
            var effect = Effect.Parallel(Effect.Pure(expectedInt), Effect.Pure(expectedString));

            //Act
            var (receivedInt, receivedString) = await sut.Interpret(effect);

            //Assert
            receivedInt.Should().Be(expectedInt);
            receivedString.Should().Be(expectedString);
        }

        [Fact]
        public async Task Interpret_impure_effect_should_execute_side_effect_broker()
        {
            //Arrange
            var sideEffect = Mock.Of<ISideEffect<int>>();
            var sideEffectBroker = new Mock<ISideEffectBroker>();
            var sut = new Interpreter(sideEffectBroker.Object);
            var effect = Effect.Of(sideEffect);

            //Act
            var result = await sut.Interpret(effect);

            //Assert
            sideEffectBroker.Verify(x=> x.Run(sideEffect, default), Times.Once);
        }

        [Fact]
        public async Task Interpret_unit_effect_should_return_unit_value()
        {
            //Arrange
            var sideEffectBroker = new Mock<ISideEffectBroker>();
            var sut = new Interpreter(sideEffectBroker.Object);
            var value = "test";
            var effect = Effect.Pure(value).ToUnit();

            //Act
            var result = await sut.Interpret(effect);

            //Assert
            result.Should().Be(Unit.Value);
        }
    }
}
