using System.Threading.Tasks;
using FluentAssertions;
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
            var sideEffectHandlerFactory = new Mock<ISideEffectHandlerFactory>();
            var sut = new Interpreter(sideEffectHandlerFactory.Object);
            var value = "test";
            var effect = new PureEffect<string>(value);

            //Act
            var result = await sut.Interpret(effect);

            //Assert
            result.Should().Be(value);
        }

        [Fact]
        public async Task Interpret_parallel_effect_should_interpret_both_effects()
        {
            //Arrange
            var sideEffectHandlerFactory = new Mock<ISideEffectHandlerFactory>();
            var sut = new Interpreter(sideEffectHandlerFactory.Object);
            var expectedInt = 5;
            var expectedString = "test";
            var effect = Effect.Parallel(new PureEffect<int>(expectedInt), new PureEffect<string>(expectedString));

            //Act
            var (receivedInt, receivedString) = await sut.Interpret(effect);

            //Assert
            receivedInt.Should().Be(expectedInt);
            receivedString.Should().Be(expectedString);
        }

        [Fact]
        public async Task Interpret_free_effect_should_execute_side_effect_handler()
        {
            //Arrange
            var sideEffectValue = 5;
            var sideEffect = Mock.Of<ISideEffect<int>>();
            var sideEffectHandler = new Mock<ISideEffectHandler<ISideEffect<int>, int>>();
            sideEffectHandler.Setup(x => x.Handle(sideEffect, default)).Returns(Task.FromResult(sideEffectValue));
            var sideEffectHandlerFactory = new Mock<ISideEffectHandlerFactory>();
            sideEffectHandlerFactory.Setup(x => x.GetSideEffectHandlerFor(sideEffect))
                .Returns(sideEffectHandler.Object);

            var sut = new Interpreter(sideEffectHandlerFactory.Object);
            var effect = Effect.Of(sideEffect);

            //Act
            var result = await sut.Interpret(effect);

            //Assert
            sideEffectHandler.Verify(x=> x.Handle(sideEffect, default), Times.Once);
            result.Should().Be(sideEffectValue);
        }
    }
}
