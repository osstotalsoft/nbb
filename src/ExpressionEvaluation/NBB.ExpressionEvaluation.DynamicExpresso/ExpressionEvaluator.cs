using DynamicExpresso;
using NBB.Tools.ExpressionEvaluation.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NBB.Tools.ExpressionEvaluation.DynamicExpresso
{
    /// <summary>
    /// DynamicExpresso implementation of IExpressionEvaluator. <see cref="NBB.Tools.ExpressionEvaluation.Abstractions.IExpressionEvaluator"/>
    /// <see cref="https://github.com/davideicardi/DynamicExpresso/"/>
    /// </summary>
    public class ExpressionEvaluator: IExpressionEvaluator
    {
        /// <summary>
        /// <para>Evaluates an expression.</para>
        /// <para>Warning: the method can have side effects because the expression can contain C# methods that can be executed</para>
        /// </summary>
        /// <param name="expression">The actual expression. Methods and properties can be used</param>
        /// <param name="parameters">Values that will be replaced and evaluated</param>
        /// <returns>The value of the expression. Warning: this can be null.</returns>
        /// <typeparam name="T">Return type</typeparam>
        public T Evaluate<T>(string expression, Dictionary<string, object> parameters)
        {

            if (string.IsNullOrWhiteSpace(expression))
                throw new ArgumentException(nameof(expression));

            if (parameters == null)
                throw new ArgumentException(nameof(parameters));

            var interpreter = new Interpreter();

            var expressionParams = parameters.Select(x => new Parameter(x.Key, x.Value)).ToArray();
            var parsedExpression = interpreter.Parse(expression, expressionParams);
            var values = expressionParams.Select(x => x.Value).ToArray();

            var result = parsedExpression.Invoke(values);
            return (T)result;
        }        
    }
}