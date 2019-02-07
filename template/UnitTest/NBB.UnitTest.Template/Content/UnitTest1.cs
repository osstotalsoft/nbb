using System;
using FluentAssertions;
using Moq;
using Xunit;

namespace NBB.UnitTestProject
{
    /// <summary>
    /// For more examples follow the link: 
    /// https://github.com/Moq/moq4/wiki/Quickstart
    /// </summary>
    public class UnitTest1
    {
        // Create local mock to be used by system under test (_sut)
        private readonly Mock<IFakeRepository> _repository;
        // The system under test
        private readonly SystemUnderTest _sut;

        // This constructor will be called for each test. The instances here are not common between tests.
        public UnitTest1()
        {
            // Initialize the mocked elements
            _repository = new Mock<IFakeRepository>();

            // Initialize the test object and pass in the mock object as parameter
            _sut = new SystemUnderTest(_repository.Object);
        }

        /// <summary>
        /// Tests should be unitary and test for one thing only! 
        /// Do not use the same test to check for multiple things even if they are available for testing in your current unit test. 
        /// Create a test to check if the parameter is passed to the repository without modifications.
        /// </summary>
        [Fact]
        public void GoodMethod_calls_repository_with_given_parram()
        {
            //Arrange
            _repository.Setup(r => r.DoSomething(It.IsAny<string>())).Returns(1);
            var param = "custom value";

            //Act
            _sut.GoodMethod(param);

            //Assert
            _repository.Verify(r => r.DoSomething(param), Times.Once);
        }

        /// <summary>
        /// Create a test to check the false result from the expression inside GoodMethod
        /// </summary>
        [Fact]
        public void GoodMethod_returns_false_when_the_repository_returns_zero()
        {
            //Arrange
            _repository.Setup(r => r.DoSomething(It.IsAny<string>())).Returns(0);

            //Act
            var result = _sut.GoodMethod("this is irrelevant");

            //Assert
            result.Should().BeFalse();
        }

        /// <summary>
        /// We want to make sure this method will not call our repository.
        /// </summary>
        [Fact]
        public void BadMethod_does_not_call_the_repository()
        {
            //Arrange
            // not necessary but you can setup the repository

            //Act
            // the code below is part of Fluent Assertion and does an assertion.
            // but because the scope of this test is to assert that the repository is not called we will keep it like this.
            // this could have been inside a try/catch block to stop the throw and not make assertions.
            _sut.Invoking(s => s.BadMethod(DateTime.Now)).Should().Throw<NotImplementedException>().WithMessage("Custom message.");

            //Assert
            _repository.Verify(r => r.DoSomething(It.IsAny<string>()), Times.Never);
        }
    }

    public class SystemUnderTest
    {
        private readonly IFakeRepository _repository;

        public SystemUnderTest(IFakeRepository repository)
        {
            _repository = repository;
        }

        public bool GoodMethod(string param)
        {
            return 0 != _repository.DoSomething(param);
        }

        public void BadMethod(DateTime date)
        {
            throw new NotImplementedException("Custom message.");
        }
    }

    public interface IFakeRepository
    {
        int DoSomething(string something);
    }
}
