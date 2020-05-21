using System;
using System.Collections.Generic;
using System.Reflection;
using FluentAssertions;
using Moq;
using NBB.Application.DataContracts;
using NBB.Core.Abstractions;
using NBB.Messaging.DataContracts;
using Xunit;

namespace NBB.Messaging.Abstractions.Tests
{
    public class MessageSerDesTests
    {
        public class TestMessage : Event
        {
            public Guid EventId { get; private set; }
            public DateTime CreationDate { get; private set; }
            public long ContractId { get; private set; }
            public long PartnerId { get; private set; }
            public string Details { get; private set; }
            public int SequenceNumber { get; set; }

            public bool ConstructedWithPrivateConstructor() => false;

            public TestMessage(long partnerId, long contractId, string details, EventMetadata metadata = null) : base(metadata)
            {
                PartnerId = partnerId;
                ContractId = contractId;
                Details = details;
            }
        }

        [Fact]
        public void Should_deserialize_messages_using_constructor_with_optional_params()
        {
            //Arrange
            var sut = new NewtonsoftJsonMessageSerDes(Mock.Of<IMessageTypeRegistry>(x => x.ResolveType(It.IsAny<string>(), It.IsAny<IEnumerable<Assembly>>()) == typeof(TestMessage)));
            var @event = new TestMessage(11232, 1122312, "something");
            var json = sut.SerializeMessageEnvelope(new MessagingEnvelope(new Dictionary<string, string>(), @event));

            //Act
            var deserialized = sut.DeserializeMessageEnvelope<TestMessage>(json);


            //Assert
            deserialized.Should().NotBeNull();
            deserialized.Payload.Should().NotBeNull();
            deserialized.Payload.Should().BeEquivalentTo(@event);
        }

        [Fact]
        public void Should_deserialize_messages_using_header_type_info()
        {
            //Arrange
            var sut = new NewtonsoftJsonMessageSerDes(
                Mock.Of<IMessageTypeRegistry>(x => 
                    x.ResolveType(It.IsAny<string>(), It.IsAny<IEnumerable<Assembly>>()) == typeof(TestMessage) &&
                    x.GetTypeId(It.IsAny<Type>()) == "TestMessage"));

            var @event = new TestMessage(11232, 1122312, "something");
            var envelope = new MessagingEnvelope(new Dictionary<string, string>
            {
                [MessagingHeaders.MessageType] = "TestMessage"
            }, @event);

            var json = sut.SerializeMessageEnvelope(envelope);

            //Act
            var deserialized = sut.DeserializeMessageEnvelope(json,
                new MessageSerDesOptions()
                {
                    DeserializationType = DeserializationType.Dynamic,
                    DynamicDeserializationScannedAssemblies = new[] {Assembly.GetExecutingAssembly()}
                });

            //Assert
            deserialized.Should().NotBeNull();
            deserialized.Payload.Should().NotBeNull();
            deserialized.Payload.GetType().Should().Be(typeof(TestMessage));
        }

        [Fact]
        public void Should_not_deserialize_messages_using_wrong_interface()
        {
            //Arrange
            var sut = new NewtonsoftJsonMessageSerDes(Mock.Of<IMessageTypeRegistry>(x => x.ResolveType(It.IsAny<string>(), It.IsAny<IEnumerable<Assembly>>()) == typeof(TestMessage)));
            var @event = new TestMessage(11232, 1122312, "something");
            var envelope = new MessagingEnvelope(new Dictionary<string, string>
            {
                [MessagingHeaders.MessageType] = typeof(TestMessage).AssemblyQualifiedName
            }, @event);

            var json = sut.SerializeMessageEnvelope(envelope);

            //Act
            void Action() => sut.DeserializeMessageEnvelope<ICommand>(json);

            //Assert
            ((Action)Action).Should().Throw<Exception>();
        }

        [Fact]
        public void Should_deserialize_messages_using_interface()
        {
            //Arrange
            var sut = new NewtonsoftJsonMessageSerDes(
                Mock.Of<IMessageTypeRegistry>(x =>
                    x.ResolveType(It.IsAny<string>(), It.IsAny<IEnumerable<Assembly>>()) == typeof(TestMessage) &&
                    x.GetTypeId(It.IsAny<Type>()) == "TestMessage"));

            var @event = new TestMessage(11232, 1122312, "something");
            var iEvent = (IEvent) @event;
            var envelope = new MessagingEnvelope(new Dictionary<string, string>()
           , iEvent);

            var json = sut.SerializeMessageEnvelope(envelope);

            //Act
            var deserialized = sut.DeserializeMessageEnvelope<IEvent>(json, new MessageSerDesOptions()
            {
                DeserializationType = DeserializationType.Dynamic,
                DynamicDeserializationScannedAssemblies = new[] { Assembly.GetExecutingAssembly() }
            });

            //Assert
            deserialized.Should().NotBeNull();
            deserialized.Payload.GetType().Should().Be(typeof(TestMessage));
        }

        [Fact]
        public void Should_deserialize_with_json_payload()
        {
            //Arrange
            var sut = new NewtonsoftJsonMessageSerDes(Mock.Of<IMessageTypeRegistry>(x => x.ResolveType(It.IsAny<string>(), It.IsAny<IEnumerable<Assembly>>()) == typeof(TestMessage)));
            var @event = new TestMessage(11232, 1122312, "something");
            var envelope = new MessagingEnvelope(new Dictionary<string, string>
            {
                [MessagingHeaders.MessageType] = @event.GetType().Name
            }, @event);

            var json = sut.SerializeMessageEnvelope(envelope);

            //Act
            var deserialized = sut.DeserializeMessageEnvelope(json,
                new MessageSerDesOptions {DeserializationType = DeserializationType.HeadersOnly});

            //Assert
            deserialized.Should().NotBeNull();
            deserialized.Payload.GetType().Should().Be(typeof(string));
            (deserialized.Payload as string).Should().ContainAll(@event.ContractId.ToString(), @event.PartnerId.ToString(), @event.Details);
        }

        [Fact]
        public void Should_throw_when_typed_deserialization_with_incompatible_options()
        {
            //Arrange
            var sut = new NewtonsoftJsonMessageSerDes(Mock.Of<IMessageTypeRegistry>(x => x.ResolveType(It.IsAny<string>(), It.IsAny<IEnumerable<Assembly>>()) == typeof(TestMessage)));
            var @event = new TestMessage(11232, 1122312, "something");
            var envelope = new MessagingEnvelope(new Dictionary<string, string>
            {
                [MessagingHeaders.MessageType] = @event.GetType().Name
            }, @event);

            var json = sut.SerializeMessageEnvelope(envelope);

            //Act
            void Action() => sut.DeserializeMessageEnvelope<MessagingEnvelope<TestMessage>>(json, new MessageSerDesOptions { DeserializationType = DeserializationType.HeadersOnly});

            //Assert

            ((Action)Action).Should().Throw<Exception>().WithMessage("*HeadersOnly*");
        }

        [Fact]
        public void Should_throw_when_untyped_deserialization_with_incompatible_options()
        {
            //Arrange
            var sut = new NewtonsoftJsonMessageSerDes(Mock.Of<IMessageTypeRegistry>(x => x.ResolveType(It.IsAny<string>(), It.IsAny<IEnumerable<Assembly>>()) == typeof(TestMessage)));
            var @event = new TestMessage(11232, 1122312, "something");
            var envelope = new MessagingEnvelope(new Dictionary<string, string>
            {
                [MessagingHeaders.MessageType] = @event.GetType().Name
            }, @event);

            var json = sut.SerializeMessageEnvelope(envelope);

            //Act
            void Action() => sut.DeserializeMessageEnvelope(json, new MessageSerDesOptions {DeserializationType = DeserializationType.TypeSafe});

            //Assert

            ((Action)Action).Should().Throw<Exception>().WithMessage("*TypeSafe*");
        }
    }
}
