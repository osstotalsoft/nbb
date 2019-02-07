using System.Collections.Generic;

namespace NBB.Tools.ExpressionEvaluation.Abstractions
{
    /// <summary>
    /// Allows evaluation of an expression
    /// </summary>
    public interface IExpressionEvaluator
    {
        /// <summary>
        /// <para>Evaluates an expression.</para>
        /// <para>Warning: the method can have side effects because the expression can contain C# methods that can be executed</para>
        /// </summary>
        /// <param name="expression">The actual expression. Methods and properties can be used</param>
        /// <param name="parameters">Values that will be replaced and evaluated</param>
        /// <returns>The value of the expression. Warning: this can be null.</returns>
        /// <typeparam name="T">Return type</typeparam>
        T Evaluate<T>(string expression, Dictionary<string, object> parameters);
    }
}