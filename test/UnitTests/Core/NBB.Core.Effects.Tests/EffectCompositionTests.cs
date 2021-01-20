using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
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

            var interpreter = new Interpreter(Mock.Of<ISideEffectBroker>());
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

            var interpreter = new Interpreter(Mock.Of<ISideEffectBroker>());
            var actual = await interpreter.Interpret(effect);
            actual.Should().Be(expected);
        }

        [Fact]
        public async Task Composition_of_pure_effects_should_have_no_side_effects_3()
        {
            int Add1(int x) => x + 1;
            int Double(int x) => x * 2;

            var effect = Effect.Parallel(
                    Effect.Pure(5).Then(Add1),
                    Effect.Pure(6).Then(Double))
                .Then(((int first, int second) t) => t.first + t.second);

            await using var interpreter = Interpreter.CreateDefault(services=> services.AddEffects());
            
            var actual = await interpreter.Interpret(effect);
            actual.Should().Be(Add1(5)+Double(6));
        }
        

    }
}
