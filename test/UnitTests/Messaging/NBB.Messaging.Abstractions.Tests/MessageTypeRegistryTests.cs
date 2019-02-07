using System;
using System.Reflection;
using FluentAssertions;
using NBB.Messaging.DataContracts;
using Xunit;

namespace NBB.Messaging.Abstractions.Tests
{
    public class MessageTypeRegistryTests
    {
        [Fact]
        public void Should_resolve_type_by_attribute()
        {
            //Arrange
            var sut = new DefaultMessageTypeRegistry();

            //Act
            var messageType = sut.ResolveType("TestMessageType", new[] { Assembly.GetExecutingAssembly() });

            //Assert
            messageType.Should().Be(typeof(MyEvent));
        }

        [Fact]
        public void Should_resolve_type_by_name()
        {
            //Arrange
            var sut = new DefaultMessageTypeRegistry();

            //Act
            var messageType = sut.ResolveType("MySecondEvent", new[] { Assembly.GetExecutingAssembly() });

            //Assert
            messageType.Should().Be(typeof(MySecondEvent));
        }

        [Fact]
        public void Should_resolve_nested_type_by_name()
        {
            //Arrange
            var sut = new DefaultMessageTypeRegistry();

            //Act
            var messageType = sut.ResolveType("MySecondEvent+MyNestedClass", new[] { Assembly.GetExecutingAssembly() });

            //Assert
            messageType.Should().Be(typeof(MySecondEvent.MyNestedClass));
        }

        [Fact]
        public void Should_handle_nested_type()
        {
            //Arrange
            var sut = new DefaultMessageTypeRegistry();

            //Act
            var messageType = sut.ResolveType(sut.GetTypeId(typeof(MySecondEvent.MyNestedClass)), new[] { Assembly.GetExecutingAssembly() });

            //Assert
            messageType.Should().Be(typeof(MySecondEvent.MyNestedClass));
        }

        [Fact]
        public void Should_handle_nested_generics()
        {
            //Arrange
            var sut = new DefaultMessageTypeRegistry();

            //Act
            var messageType = sut.ResolveType(sut.GetTypeId(typeof(MyParentGeneric<Int32,bool>.MyNestedGeneric<string>)), new[] { Assembly.GetExecutingAssembly() });

            //Assert
            messageType.Should().Be(typeof(MyParentGeneric<Int32,bool>.MyNestedGeneric<string>));
        }

        [Fact]
        public void Should_resolve_nested_generics()
        {
            //Arrange
            var sut = new DefaultMessageTypeRegistry();

            //Act
            var messageType = sut.ResolveType("MyParentGeneric<Int32, bool>+MyNestedGeneric<string>", new[] { Assembly.GetExecutingAssembly() });

            //Assert
            messageType.Should().Be(typeof(MyParentGeneric<Int32, bool>.MyNestedGeneric<string>));
        }

        [Fact]
        public void Should_resolve_array_type_by_name()
        {
            //Arrange
            var sut = new DefaultMessageTypeRegistry();

            //Act
            var messageType = sut.ResolveType("MySecondEvent[]", new[] { Assembly.GetExecutingAssembly() });

            //Assert
            messageType.Should().Be(typeof(MySecondEvent[]));
        }

        [Fact]
        public void Should_handle_array_type()
        {
            //Arrange
            var sut = new DefaultMessageTypeRegistry();

            //Act
            var messageType = sut.ResolveType(sut.GetTypeId(typeof(MySecondEvent[])), new[] { Assembly.GetExecutingAssembly() });

            //Assert
            messageType.Should().Be(typeof(MySecondEvent[]));
        }

        [Fact]
        public void Should_resolve_generic_type_by_name()
        {
            //Arrange
            var sut = new DefaultMessageTypeRegistry();

            //Act
            var messageType = sut.ResolveType("MyGenericType<string>", new[] { Assembly.GetExecutingAssembly() });

            //Assert
            messageType.Should().Be(typeof(MyGenericType<string>));
        }

        [Fact]
        public void Should_handle_generic_type()
        {
            //Arrange
            var sut = new DefaultMessageTypeRegistry();

            //Act
            var messageType = sut.ResolveType(sut.GetTypeId(typeof(MyGenericType<string>)), new[] { Assembly.GetExecutingAssembly() });

            //Assert
            messageType.Should().Be(typeof(MyGenericType<string>));
        }

        [Fact]
        public void Should_resolve_compound_generic_type_by_name()
        {
            //Arrange
            var sut = new DefaultMessageTypeRegistry();

            //Act
            var messageType = sut.ResolveType("MyGenericType<string, MyGenericType<MySecondEvent>>", new[] { Assembly.GetExecutingAssembly() });

            //Assert
            messageType.Should().Be(typeof(MyGenericType<string, MyGenericType<MySecondEvent>>));
        }

        [Fact]
        public void Should_handle_compound_generic_type()
        {
            //Arrange
            var sut = new DefaultMessageTypeRegistry();

            //Act
            var messageType = sut.ResolveType(sut.GetTypeId(typeof(MyGenericType<string, MyGenericType<MySecondEvent>>)), new[] { Assembly.GetExecutingAssembly() });

            //Assert
            messageType.Should().Be(typeof(MyGenericType<string, MyGenericType<MySecondEvent>>));
        }

        [Fact]
        public void Should_resolve_compound_generic_array()
        {
            //Arrange
            var sut = new DefaultMessageTypeRegistry();

            //Act
            var messageType = sut.ResolveType("MyGenericType<MySecondEvent>[,]", new[] { Assembly.GetExecutingAssembly() });

            //Assert
            messageType.Should().Be(typeof(MyGenericType<MySecondEvent>[,]));
        }

        [Fact]
        public void Should_handle_compound_generic_array()
        {
            //Arrange
            var sut = new DefaultMessageTypeRegistry();

            //Act
            var messageType = sut.ResolveType(sut.GetTypeId(typeof(MyGenericType<MySecondEvent>[,])), new[] { Assembly.GetExecutingAssembly() });

            //Assert
            messageType.Should().Be(typeof(MyGenericType<MySecondEvent>[,]));
        }

        [Fact]
        public void Should_resolve_multi_dimensional_array_by_name()
        {
            //Arrange
            var sut = new DefaultMessageTypeRegistry();

            //Act
            var messageType = sut.ResolveType("MySecondEvent[][,]", new[] { Assembly.GetExecutingAssembly() });

            //Assert
            messageType.Should().Be(typeof(MySecondEvent[][,]));
        }

        [Fact]
        public void Should_handle_multi_dimensional_array()
        {
            //Arrange
            var sut = new DefaultMessageTypeRegistry();

            //Act
            var messageType = sut.ResolveType(sut.GetTypeId(typeof(MySecondEvent[][,])), new[] { Assembly.GetExecutingAssembly() });

            //Assert
            messageType.Should().Be(typeof(MySecondEvent[][,]));
        }

        [Fact]
        public void Should_resolve_type_id_by_name()
        {
            //Arrange
            var sut = new DefaultMessageTypeRegistry();

            //Act
            var messageTypeId = sut.GetTypeId(typeof(MySecondEvent));

            //Assert
            messageTypeId.Should().Be("MySecondEvent");
        }

        [Fact]
        public void Should_resolve_nested_type_id_by_name()
        {
            //Arrange
            var sut = new DefaultMessageTypeRegistry();

            //Act
            var messageTypeId = sut.GetTypeId(typeof(MySecondEvent.MyNestedClass));

            //Assert
            messageTypeId.Should().Be("MySecondEvent+MyNestedClass");
        }

        [Fact]
        public void Should_resolve_generic_type_id_by_name()
        {
            //Arrange
            var sut = new DefaultMessageTypeRegistry();

            //Act
            var messageTypeId = sut.GetTypeId(typeof(MyGenericType<string>));

            //Assert
            messageTypeId.Should().Be("MyGenericType<String>");
        }

        [Fact]
        public void Should_resolve_nestedType_as_generic_param_type_id_by_name()
        {
            //Arrange
            var sut = new DefaultMessageTypeRegistry();

            //Act
            var messageTypeId = sut.GetTypeId(typeof(MyGenericType<MySecondEvent.MyNestedClass>));

            //Assert
            messageTypeId.Should().Be("MyGenericType<MySecondEvent+MyNestedClass>");
        }

        [Fact]
        public void Should_resolve_nestedType_as_generic_param_type_by_name()
        {
            //Arrange
            var sut = new DefaultMessageTypeRegistry();

            //Act
            var messageType = sut.ResolveType("MyGenericType<MySecondEvent+MyNestedClass>", new[] { Assembly.GetExecutingAssembly() });

            //Assert
            messageType.Should().Be(typeof(MyGenericType<MySecondEvent.MyNestedClass>));
        }

        [Fact]
        public void Should_handle_nestedType_array()
        {
            //Arrange
            var sut = new DefaultMessageTypeRegistry();

            //Act
            var messageType = sut.ResolveType(sut.GetTypeId(typeof(MySecondEvent.MyNestedClass[])), new[] { Assembly.GetExecutingAssembly() });

            //Assert
            messageType.Should().Be(typeof(MySecondEvent.MyNestedClass[]));
        }

        [Fact]
        public void Should_resolve_type_id_by_attribute()
        {
            //Arrange
            var sut = new DefaultMessageTypeRegistry();

            //Act
            var messageTypeId = sut.GetTypeId(typeof(MyEvent));

            //Assert
            messageTypeId.Should().Be("TestMessageType");
        }

        [Fact]
        public void Should_fail_resolve_when_type_not_found()
        {
            //Arrange
            var sut = new DefaultMessageTypeRegistry();

            //Act
            void Action()
            {
                sut.ResolveType("MyNotFoundEvent", new[] { Assembly.GetExecutingAssembly() });
            }

            //Assert
            ((Action)Action).Should().Throw<ApplicationException>().WithMessage("*could not be resolved*");

        }

        [Fact]
        public void Should_fail_resolve_when_multiple_types_by_attribute()
        {
            //Arrange
            var sut = new DefaultMessageTypeRegistry();

            //Act
            void Action()
            {
                sut.ResolveType("SameTypeIdAttribute", new[] { Assembly.GetExecutingAssembly() });
            }

            //Assert
            ((Action) Action).Should().Throw<ApplicationException>().WithMessage("*multiple*");
        }

        [Fact]
        public void Should_fail_resolve_when_multiple_types_by_name()
        {
            //Arrange
            var sut = new DefaultMessageTypeRegistry();

            //Act
            void Action()
            {
                sut.ResolveType("SameName", new[] { Assembly.GetExecutingAssembly() });
            }

            //Assert
            ((Action)Action).Should().Throw<ApplicationException>().WithMessage("*multiple*");
        }
    }

    // ReSharper Disable All 
    namespace Namespace1
    {
        public class SameName
        {
        }
    }

    namespace Namespace2
    {
        public class SameName
        {
        }
    }

    [MessageTypeId("TestMessageType")]
    public class MyEvent
    {
    }

    public class MySecondEvent
    {
        public class MyNestedClass
        {
        }
    }

    public class MyGenericType<T>
    {
    }
    
    public class MyGenericType<T,TVal>
    {
    }

    public class MyParentGeneric<TParent, T2>
    {
        public class MyNestedGeneric<TNested> { }
    }

    [MessageTypeId("SameTypeIdAttribute")]
    public class SameType1
    {
    }

    [MessageTypeId("SameTypeIdAttribute")]
    public class SameType2
    {
    }

    // ReSharper Restore All 
}