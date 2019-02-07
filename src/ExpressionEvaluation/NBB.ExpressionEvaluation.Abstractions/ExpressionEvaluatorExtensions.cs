using System.Collections.Generic;

namespace NBB.Tools.ExpressionEvaluation.Abstractions
{
    /// <summary>
    /// Extension methods for IExpressionEvaluator
    /// </summary>
    public static class ExpressionEvaluatorExtensions
    {
        /// <summary>
        /// A simple expression evaluator without context. Works for simple expressions like "1+1"
        /// </summary>
        /// <param name="evaluator">IExpressionEvaluator instance</param>
        /// <param name="expression">Expression to be evaluated</param>
        /// <typeparam name="T">Return type</typeparam>
        /// <example>
        /// var s = evaluator.Evaluate("1+1");
        /// </example>
        /// <returns>The result. Warning: this can be null. </returns>
        public static T Evaluate<T>(this IExpressionEvaluator evaluator, string expression) 
            => evaluator.Evaluate<T>(expression, new Dictionary<string, object>());
    }
}
