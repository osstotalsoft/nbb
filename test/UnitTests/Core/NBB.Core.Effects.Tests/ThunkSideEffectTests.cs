// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace NBB.Core.Effects.Tests
{
    public class ThunkSideEffectTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public ThunkSideEffectTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async Task Interpret_Thunk_SideEffect()
        {
            //Arrange
            await using var interpreter = Interpreter.CreateDefault();

            var sideEffectExecuted = false;
            var eff = Effect.From(() =>
            {
                _testOutputHelper.WriteLine("Effect executed");
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
