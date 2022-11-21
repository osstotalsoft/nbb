using FluentAssertions;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace NBB.Core.Configuration.Tests
{
    public class DotEnvConfigurationTests : IDisposable
    {
        // setup
        public DotEnvConfigurationTests()
        {
            Directory.CreateDirectory("config");
        }

        // teardown
        public void Dispose()
        {
            Directory.Delete("config", recursive: true);
        }

        [Fact]
        public void Should_read_simple_value()
        {
            // Arrange
            var configurationManager = new ConfigurationManager();
            File.WriteAllText("config/env.txt", @"JAEGER_DISABLED=false");

            // Act
            configurationManager.AddDotEnvFile("config/env.txt");

            // Assert
            configurationManager["JAEGER_DISABLED"].Should().Be("false");
        }

        [Fact]
        public void Should_read_hierarchic_value()
        {
            // Arrange
            var configurationManager = new ConfigurationManager();
            File.WriteAllText("config/env.txt", @"MultiTenancy__Tenants__BCR__TenentId=test1");

            // Act
            configurationManager.AddDotEnvFile("config/env.txt");

            // Assert
            configurationManager.GetSection("MultiTenancy")["Tenants:BCR:TenentId"].Should().Be("test1");
        }

        [Fact]
        public void Should_throw_when_file_not_found()
        {
            // Arrange
            var configurationManager = new ConfigurationManager();

            // Act          
            Action act = () => configurationManager.AddDotEnvFile("config/wrong.txt", optional: false);

            // Assert
            act.Should().Throw<FileNotFoundException>();
        }

        [Fact]
        public async Task Shoud_reload_after_file_changed()
        {
            // Arrange
            var configurationManager = new ConfigurationManager();
            File.WriteAllText("config/env.txt", @"JAEGER_DISABLED=false");

            configurationManager.AddDotEnvFile("config/env.txt", optional: false, reloadOnChange: true);
            var before = configurationManager["JAEGER_DISABLED"];

            // Act
            File.WriteAllText("config/env.txt", @"JAEGER_DISABLED=true");
            await Task.Delay(500);
            var after = configurationManager["JAEGER_DISABLED"];

            // Assert
            before.Should().Be("false");
            after.Should().Be("true");
        }

        [Fact]
        public void Should_remove_quotes()
        {
            // Arrange
            var configurationManager = new ConfigurationManager();
            File.WriteAllText("config/env.txt", @"Key1=""mystring""");

            // Act
            configurationManager.AddDotEnvFile("config/env.txt");

            // Assert
            configurationManager["Key1"].Should().Be("mystring");
        }

        [Fact]
        public void Should_ignore_white_space()
        {
            // Arrange
            var configurationManager = new ConfigurationManager();
            File.WriteAllText("config/env.txt",
                @"
                    Key1 =      Value1

                     Key2    =Value2
                ");

            // Act          
            configurationManager.AddDotEnvFile("config/env.txt");

            // Assert
            configurationManager.GetChildren().Should().HaveCount(2);
            configurationManager["Key1"].Should().Be("Value1");
            configurationManager["Key2"].Should().Be("Value2");
        }

        [Fact]
        public void Should_ignore_comments()
        {
            // Arrange
            var configurationManager = new ConfigurationManager();
            File.WriteAllText("config/env.txt",
                @"
                    # My comment
                    Key1=Value1
                ");

            // Act          
            configurationManager.AddDotEnvFile("config/env.txt");

            // Assert
            configurationManager.GetChildren().Should().HaveCount(1);
            configurationManager["Key1"].Should().Be("Value1");
        }
    }
}
