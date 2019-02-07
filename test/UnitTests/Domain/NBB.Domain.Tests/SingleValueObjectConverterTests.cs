using System;
using System.Collections.Generic;
using FluentAssertions;
using Newtonsoft.Json;
using Xunit;

namespace NBB.Domain.Tests
{
    public class SingleValueObjectConverterTests
    {
        public JsonSerializerSettings SerializerSettings => new JsonSerializerSettings()
        {
            Converters = new List<JsonConverter> {new SingleValueObjectConverter()}
        };

        [Fact]
        public void Should_serialize_identity_using_converter()
        {
            //Arrange
            var identity = new TaskId();
            var identityValueJson = JsonConvert.SerializeObject(identity.Value, SerializerSettings);

            //Act
            var identityJson = JsonConvert.SerializeObject(identity, SerializerSettings);

            //Assert
            identityJson.Should().Be(identityValueJson);
        }

        [Fact]
        public void Should_deserialize_identity_using_converter()
        {
            //Arrange
            var identityValue = Guid.NewGuid();
            var identityValueJson = JsonConvert.SerializeObject(identityValue, SerializerSettings);

            //Act
            var identity = JsonConvert.DeserializeObject<TaskId>(identityValueJson, SerializerSettings);

            //Assert
            identity.Value.Should().Be(identityValue);
        }

        [Fact]
        public void Should_serialize_value_object_with_null_value()
        {
            //Arrange
            var identity = new StringId(null);
            var identityValueJson = JsonConvert.SerializeObject(identity.Value, SerializerSettings);

            //Act
            var identityJson = JsonConvert.SerializeObject(identity, SerializerSettings);

            //Assert
            identityJson.Should().Be(identityValueJson);
        }

        [Fact]
        public void Should_deserialize_value_object_with_null_value()
        {
            //Arrange
            var identityValueJson = JsonConvert.SerializeObject(null, SerializerSettings);

            //Act
            var identity = JsonConvert.DeserializeObject<StringId>(identityValueJson, SerializerSettings);

            //Assert
            identity.Should().NotBeNull();
            identity.Value.Should().Be(null);
        }
    }

    public class TaskId : Identity<Guid>
    {
        [JsonConstructor]
        private TaskId(Guid value)
            : base(value)
        {
        }

        public TaskId()
            : this(Guid.NewGuid())
        {
        }
    }

    public class StringId : SingleValueObject<string>
    {
        public StringId(string value) : base(value)
        {

        }
    }
}
