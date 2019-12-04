using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;

namespace NBB.Core.Effects.Tests
{
    public class EffectCompositionTests
    {
        [Fact]
        public async Task Composition_of_pure_effects_should_have_no_side_effects()
        {
            var expected = 10;
            var effect = Effect.Pure(expected)
                .Then(x => x.ToString())
                .Then(Effect.Pure)
                .Then(int.Parse);

            var interpreter = new Interpreter(Mock.Of<ISideEffectHandlerFactory>());
            var actual = await interpreter.Interpret(effect);
            actual.Should().Be(expected);
        }

        [Fact]
        public async Task Composition_of_pure_effects_should_have_no_side_effects_2()
        {
            var expected = 10;
            var effect = Effect.Pure(5)
                .Then(x => x.ToString())
                .Then(Effect.Pure)
                .Then(int.Parse)
                .ToUnit()
                .Then(Effect.Pure(expected));

            var interpreter = new Interpreter(Mock.Of<ISideEffectHandlerFactory>());
            var actual = await interpreter.Interpret(effect);
            actual.Should().Be(expected);
        }
    }
}
