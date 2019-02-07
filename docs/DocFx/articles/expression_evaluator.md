Expression evaluator
===============

Allows simple C# expressions to be evaluated.
It also allows object methods and properties to be called, so the evaluation can have side effects.

Source: NBB.Tools.ExpressionEvaluation.Abstractions and NBB.Tools.ExpressionEvaluation.DynamicExpresso

Current implementation is based on DynamicExpresso - https://github.com/davideicardi/DynamicExpresso



Simple usage
----------------

```csharp

public class Test
{

	public void DoTest()
	{
		var evaluator = new ExpressionEvaluator();
		var s = evaluator.Evaluate<int>("1+1");

		string expression = "x > 4 ? service.OneMethod() : service.AnotherMethod()";
		var parameters = new Dictionary<string, object>
		{
			{ "x", 5 },
			{ "service", new Service() }
		};

		var r = evaluator.Evaluate<int>(expression, parameters);
		// calls OneMethod and returns 1

		parameters = new Dictionary<string, object>
		{
			{ "document", new Service() },
			{ "paymentPlan", new Service1() },
			{ "x", 0 }
		};

		var t = evaluator.Evaluate<int>("x> 1? document.OneMethod(): paymentPlan.OneMethod()", parameters);
		// will call OneMethod from the paymentPlan document


	}
}

class Service
{
    public int Prop { get; set; } = 10;
    public int OneMethod()
    {
        Console.WriteLine("One");
        return 1;
    }
    public int AnotherMethod()
    {
        Console.WriteLine("Two");
        return 2;
    }
}

class Service1
{

    public int OneMethod()
    {
        Console.WriteLine("One Service2");
        return 1;
    }
    public int AnotherMethod()
    {
        Console.WriteLine("Two Service2");
        return 2;
    }
}

```



Examples
----------------
```csharp
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using FluentAssertions;
using NBB.Tools.ExpressionEvaluation.Abstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Xunit;

public class UnitTest1
{
	private readonly IExpressionEvaluator _expressionEvaluator;
	public UnitTest1()
	{
		_expressionEvaluator = new ExpressionEvaluator();
	}


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
	public void BusinessFlowJson_dynamic_works()
	{
		//Arrange

		var json = "{ name: \"John\", Age: 31, city: \"New York\"}";
		var converter = new ExpandoObjectConverter();
		dynamic account = JsonConvert.DeserializeObject<ExpandoObject>(json, converter);
		var valueToTest = _expressionEvaluator.Evaluate<Int64>("account.Age", new Dictionary<string, object> { { "account", account } });

		var expression = "valueToTest > 10";
		//expression = "account.Age > 10";
		var parameters = new Dictionary<string, object>
		{
			{ "account", account},
			{ "valueToTest", valueToTest }
		};
            

		//Act
		var result = _expressionEvaluator.Evaluate<bool>(expression, parameters);

		//Assert
		result.Should().Be(true);
	}
}
```