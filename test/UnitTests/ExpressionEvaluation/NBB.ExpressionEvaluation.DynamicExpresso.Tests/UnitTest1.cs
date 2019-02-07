using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using FluentAssertions;
using NBB.Tools.ExpressionEvaluation.Abstractions;
using NBB.Tools.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Xunit;

namespace NBB.Tools.ExpressionEvaluation.DynamicExpresso.Tests
{
    /// <summary>
    /// For more examples follow the link: 
    /// https://github.com/Moq/moq4/wiki/Quickstart
    /// </summary>
    public class UnitTest1
    {
        private readonly IExpressionEvaluator _expressionEvaluator;

        // This constructor will be called for each test. The instances here are not common between tests.
        public UnitTest1()
        {
            _expressionEvaluator = new ExpressionEvaluator();
        }

        /// <summary>
        /// Tests should be unitary and test for one thing only! 
        /// Do not use the same test to check for multiple things even if they are available for testing in your current unit test. 
        /// Create a test to check if the parameter is passed to the repository without modifications.
        /// </summary>
        [Fact]
        public void Simple_expression_evaluation_works()
        {
            //Arrange
            var expression = "1+1";

            //Act
            var result = _expressionEvaluator.Evaluate<int>(expression);

            //Assert
            result.Should().Be(2);
        }

        /// <summary>
        /// Create a test to check the false result from the expression inside GoodMethod
        /// </summary>
        [Fact]
        public void Context_less_expression_works()
        {
            //Arrange
            var expression = "x+1";
            var parameters = new Dictionary<string, object>
            {
                { "x", 1}
            };


            ////Act
            var result = _expressionEvaluator.Evaluate<int>(expression, parameters);

            ////Assert
            result.Should().Be(2);
        }

        /// <summary>
        /// We want to make sure this method will not call our repository.
        /// </summary>
        [Fact]
        public void Multiple_context_and_parameters_works()
        {
            //Arrange
            var expression = "document.Count + paymentPlan.Months + x";
            var parameters = new Dictionary<string, object>
            {
                { "x", 1},
                { "document", new Document()},
                { "paymentPlan", new PaymentPlan()}
            };
            

            //Act
            var result = _expressionEvaluator.Evaluate<int>(expression, parameters);

            //Assert
            result.Should().Be(26);
        }

        [Fact]
        public void BusinessFlow_works()
        {
            //Arrange
            var json = "{ name: \"John\", age: 31, city: \"New York\"}";
            var account = JsonConvert.DeserializeObject<Account>(json);
            var expression = "account.Age > 10";
            var parameters = new Dictionary<string, object>
            {
                { "account", account},
            };
            

            //Act
            var result = _expressionEvaluator.Evaluate<bool>(expression, parameters);

            //Assert
            result.Should().Be(true);
        }

        // 
        [Fact]
        public void BusinessFlowAnonymousType_works()
        {
            //Arrange
            var definition = new { Name = "", Age = 0, City = "" };
            var json = "{ name: \"John\", age: 31, city: \"New York\"}";
            var account = JsonConvert.DeserializeAnonymousType(json, definition);

            var expression = "account.Age > 10";
            var parameters = new Dictionary<string, object>
            {
                { "account", account},
            };           

            //Act
            var result = _expressionEvaluator.Evaluate<bool>(expression, parameters);

            //Assert
            result.Should().Be(true);
        }

       


        [Fact]
        public void Test_dynamic()
        {
            //Arrange
            var json = "{ Name: \"John\", Age: 31, City: \"New York\"}";
            JObject stuff = JObject.Parse(json);
            
            
            var properties = stuff.Properties();
            var listOfFields = properties.Select(p => new CustomFieldDefinition
            { 
                FieldName = p.Name,
                FieldType = GetType(p.Value.Type)
            }).ToList();            

            var type = CustomTypeBuilder.CompileResultType(listOfFields);

            var prop = type.GetProperties();

            var account = JsonConvert.DeserializeObject(json, type);
            var expression = "account.Age > 10";
            var parameters = new Dictionary<string, object>
            {
                { "account", account},
            };


            //Act
            var result = _expressionEvaluator.Evaluate<bool>(expression, parameters);

            //Assert
            result.Should().Be(true);
        }

        Type GetType(JTokenType jTokenType)
        {
            if (jTokenType == JTokenType.String)
                return typeof(string);
            if (jTokenType == JTokenType.None)
                return typeof(object);
            if (jTokenType == JTokenType.Object)
                return typeof(object);
            if (jTokenType == JTokenType.Array)
                return typeof(Array);
            if (jTokenType == JTokenType.Constructor)
                return typeof(object);
            if (jTokenType == JTokenType.Integer)
                return typeof(int);
            if (jTokenType == JTokenType.Float)
                return typeof(long);
            if (jTokenType == JTokenType.Boolean)
                return typeof(bool);
            if (jTokenType == JTokenType.Comment)
                return typeof(string);
            if (jTokenType == JTokenType.Date)
                return typeof(DateTime);
            if (jTokenType == JTokenType.Raw)
                return typeof(string);
            if (jTokenType == JTokenType.Bytes)
                return typeof(byte[]);
            if (jTokenType == JTokenType.Guid)
                return typeof(Guid);
            if (jTokenType == JTokenType.Uri)
                return typeof(Uri);
            if (jTokenType == JTokenType.TimeSpan)
                return typeof(TimeSpan);

            return typeof(object);
        }

        //[Fact]
        //public void BusinessFlowJson_dynamic_works()
        //{
        //    //Arrange

        //    var json = "{ name: \"John\", Age: 31, city: \"New York\"}";
        //    var converter = new ExpandoObjectConverter();
        //    dynamic account = JsonConvert.DeserializeObject<ExpandoObject>(json, converter);
        //    var valueToTest = _expressionEvaluator.Evaluate<Int64>("account.Age", new Dictionary<string, object> { { "account", account } });

        //    var expression = "valueToTest > 10";
        //    //expression = "account.Age > 10";
        //    var parameters = new Dictionary<string, object>
        //    {
        //        { "account", account},
        //        { "valueToTest", valueToTest }
        //    };
            

        //    //Act
        //    var result = _expressionEvaluator.Evaluate<bool>(expression, parameters);

        //    //Assert
        //    result.Should().Be(true);
        //}

        #region tests that do not work. 
        //[Fact]
        //public void BusinessFlowAnonymousType_works22()
        //{
        //    //Arrange
        //    //var definition = new { Name = "", Age = 0, City = "" };
        //    var json = "{ name: \"John\", age: 31, city: \"New York\"}";
        //    //var account = JsonConvert.DeserializeAnonymousType(json, definition);
        //    Func<string, object> func = (x) => Newtonsoft.Json.JsonConvert.DeserializeObject(x);
        //    var expression = "((NBB.ExpressionEvaluation.DynamicExpresso.Tests.Account)func(json)).Age > 10;";
        //    var parameters = new Dictionary<string, object>
        //    {
        //        { "json", json},
        //        { "func", func},
        //    };


        //    //Act
        //    var result = _expressionEvaluator.Evaluate<bool>(expression, parameters);

        //    //Assert
        //    result.Should().Be(true);
        //}

        //[Fact]
        //public void BusinessFlowJson_dictionary_works()
        //{
        //    //Arrange            
        //    var json = "{ name: \"John\", Age: 31, city: \"New York\"}";

        //    var values = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        //    var x = values["Age"];
        //    var name = "Age";

        //    var expression = @"values.Get<string, object, int>(name) > 10";
        //    var parameters = new Dictionary<string, object>
        //    {
        //        { "name", name},
        //        { "values", values}
        //    };


        //    //Act
        //    var result = _expressionEvaluator.Evaluate<bool>(expression, parameters);

        //    //Assert
        //    result.Should().Be(true);
        //}

        //[Fact]
        //public void BusinessFlowJson_jobject_works()
        //{
        //    //Arrange            
        //    var json = "{ name: \"John\", Age: 31, city: \"New York\"}";

        //    JObject account = JObject.Parse(json);
        //    var x = account["Age"];

        //    var expression = "account[\"Age\"].GetValue<int>() > 10";
        //    var parameters = new Dictionary<string, object>
        //    {
        //        { "account", account},
        //    };


        //    //Act
        //    var result = _expressionEvaluator.Evaluate<bool>(expression, parameters);

        //    //Assert
        //    result.Should().Be(true);
        //}

        #endregion

    }

    public static class DictionaryExtensions
    {
        public static object Get<T1, T2, T3>(this Dictionary<T1, T2> dictionary, T1 key)
        {
            return  Convert.ChangeType(dictionary[key], typeof(T3));
        }
    }

    class Account
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public string City { get; set; }
    }

    class Document
    {
        public int Count { get; set; } = 10;
        public int Calculate()
        {
            Console.WriteLine("Calculate");
            return 1;
        }
        public int Save()
        {
            Console.WriteLine("Save");
            return 2;
        }
    }

    class PaymentPlan
    {
        public int Months { get; set; } = 15;
        public int Refresh()
        {
            Console.WriteLine("PaymentPlanRefresh");
            return 1;
        }
        public int Save()
        {
            Console.WriteLine("PaymentPlanSave");
            return 2;
        }
    }

    
}
