// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using OpenTelemetry.Trace;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Diagnostics;

namespace NBB.Tools.Serilog.OpenTelemetryTracingSink.Internal
{
    internal class OpenTelemetryTracingSink : ILogEventSink
    {
        private readonly Func<LogEvent, bool> _filter;

        public OpenTelemetryTracingSink(Func<LogEvent, bool> filter = null)
        {

            _filter = filter ?? (logEvent => false);
        }

        public void Emit(LogEvent logEvent)
        {
            var activity = Activity.Current;

            if (!(activity?.IsAllDataRequested ?? false))
            {
                // Creating a new activity for a log message seems brutal so we ignore messages if we can't attach it to the current span.
                return;
            }

            if (_filter(logEvent))
            {
                return;
            }

            try
            {
                var tags = new ActivityTagsCollection
                {
                    { "Message", logEvent.RenderMessage() },
                    { "LogLevel", logEvent.Level },
                };

                foreach (var property in logEvent.Properties)
                {
                    tags[property.Key] = property.Value.ToString();
                }

                var activityEvent = new ActivityEvent("log", logEvent.Timestamp, tags);
                activity.AddEvent(activityEvent);

                if (logEvent.Exception != null)
                {
                    activity.AddException(logEvent.Exception);
                }
            }
            catch (Exception logException)
            {
                activity.AddException(logException);
            }
        }
    }
}
