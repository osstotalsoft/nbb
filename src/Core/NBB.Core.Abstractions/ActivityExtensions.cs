// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;

namespace NBB.Core.Abstractions
{
    public static class ActivityExtensions
    {
        /// <summary>
        /// Sets the status of activity execution.
        /// Activity class in .NET does not support 'Status'.
        /// This extension provides a workaround to store Status as special tags with key name of otel.status_code and otel.status_description.
        /// Read more about SetStatus here https://github.com/open-telemetry/opentelemetry-specification/blob/main/specification/trace/api.md#set-status.
        /// </summary>
        /// <param name="activity">Activity instance.</param>
        /// <param name="status">Activity execution status.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetStatus(this Activity activity, Status status)
        {
            Debug.Assert(activity != null, "Activity should not be null");

            activity.SetTag(SpanAttributeConstants.StatusCodeKey, StatusHelper.GetTagValueForStatusCode(status.StatusCode));
            activity.SetTag(SpanAttributeConstants.StatusDescriptionKey, status.Description);
        }

        /// <summary>
        /// Adds an activity event containing information from the specified exception.
        /// </summary>
        /// <param name="activity">Activity instance.</param>
        /// <param name="ex">Exception to be recorded.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetException(this Activity activity, Exception ex)
        {
            activity?.SetException(ex, default);
        }

        /// <summary>
        /// Adds an activity event containing information from the specified exception and additional tags.
        /// </summary>
        /// <param name="activity">Activity instance.</param>
        /// <param name="ex">Exception to be recorded.</param>
        /// <param name="tags">Additional tags to record on the event.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetException(this Activity activity, Exception ex, in TagList tags)
        {
            if (ex == null || activity == null)
            {
                return;
            }

            var tagsCollection = new ActivityTagsCollection
            {
                { SemanticConventions.AttributeExceptionType, ex.GetType().FullName },
                { SemanticConventions.AttributeExceptionStacktrace, ex.ToInvariantString() },
            };

            if (!string.IsNullOrWhiteSpace(ex.Message))
            {
                tagsCollection.Add(SemanticConventions.AttributeExceptionMessage, ex.Message);
            }

            foreach (var tag in tags)
            {
                tagsCollection[tag.Key] = tag.Value;
            }

            activity.AddEvent(new ActivityEvent(SemanticConventions.AttributeExceptionEventName, default, tagsCollection));
        }
    }


    /// <summary>
    /// Canonical result code of span execution.
    /// </summary>
    public enum StatusCode
    {
        /// <summary>
        /// The default status.
        /// </summary>
        Unset = 0,

        /// <summary>
        /// The operation completed successfully.
        /// </summary>
        Ok = 1,

        /// <summary>
        /// The operation contains an error.
        /// </summary>
        Error = 2,
    }

    /// <summary>
    /// Defines well-known span attribute keys.
    /// </summary>
    internal static class SpanAttributeConstants
    {
        public const string StatusCodeKey = "otel.status_code";
        public const string StatusDescriptionKey = "otel.status_description";
        public const string DatabaseStatementTypeKey = "db.statement_type";
    }

    /// <summary>
    /// Span execution status.
    /// </summary>
    public readonly record struct Status(StatusCode StatusCode, string Description = null)
    {
        /// <summary>
        /// The operation completed successfully.
        /// </summary>
        public static readonly Status Ok = new(StatusCode.Ok);

        /// <summary>
        /// The default status.
        /// </summary>
        public static readonly Status Unset = new(StatusCode.Unset);

        /// <summary>
        /// The operation contains an error.
        /// </summary>
        public static readonly Status Error = new(StatusCode.Error);
    }
    internal static class StatusHelper
    {
        public const string UnsetStatusCodeTagValue = "UNSET";
        public const string OkStatusCodeTagValue = "OK";
        public const string ErrorStatusCodeTagValue = "ERROR";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetTagValueForStatusCode(StatusCode statusCode)
        {
            return statusCode switch
            {
                /*
                 * Note: Order here does matter for perf. Unset is
                 * first because assumption is most spans will be
                 * Unset, then Error. Ok is not set by the SDK.
                 */
                StatusCode.Unset => UnsetStatusCodeTagValue,
                StatusCode.Error => ErrorStatusCodeTagValue,
                StatusCode.Ok => OkStatusCodeTagValue,
                _ => null,
            };
        }
    }

    internal static class SemanticConventions {
        public const string AttributeExceptionEventName = "exception";
        public const string AttributeExceptionType = "exception.type";
        public const string AttributeExceptionMessage = "exception.message";
        public const string AttributeExceptionStacktrace = "exception.stacktrace";
    }

    internal static class ExceptionExtensions
    {
        /// <summary>
        /// Returns a culture-independent string representation of the given <paramref name="exception"/> object,
        /// appropriate for diagnostics tracing.
        /// </summary>
        /// <param name="exception">Exception to convert to string.</param>
        /// <returns>Exception as string with no culture.</returns>
        public static string ToInvariantString(this Exception exception)
        {
            var originalUICulture = Thread.CurrentThread.CurrentUICulture;

            try
            {
                Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
                return exception.ToString();
            }
            finally
            {
                Thread.CurrentThread.CurrentUICulture = originalUICulture;
            }
        }
    }
}
