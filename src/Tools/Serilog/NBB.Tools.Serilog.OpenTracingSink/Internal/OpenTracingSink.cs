// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using OpenTracing;
using OpenTracing.Util;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;

namespace NBB.Tools.Serilog.OpenTracingSink.Internal
{
    internal class OpenTracingSink : ILogEventSink
    {
        private readonly ITracer _tracer;
        private readonly Func<LogEvent, bool> _filter;

        public OpenTracingSink(Func<LogEvent, bool> filter = null)
        {
            _tracer = GlobalTracer.Instance;
            _filter = filter ?? (logEvent => false);
        }

        public void Emit(LogEvent logEvent)
        {
            ISpan span = _tracer.ActiveSpan;

            if (span == null)
            {
                // Creating a new span for a log message seems brutal so we ignore messages if we can't attach it to an active span.
                return;
            }

            if (_tracer.IsNoopTracer())
            {
                return;
            }

            if (_filter(logEvent))
            {
                return;
            }

            var fields = new Dictionary<string, object>();

            try
            {
                fields[LogFields.Event] = "log";
                fields[LogFields.Message] = logEvent.RenderMessage();
                fields["level"] = logEvent.Level;

                if (logEvent.Exception != null)
                {
                    fields[LogFields.ErrorKind] = logEvent.Exception.GetType().FullName;
                    fields[LogFields.ErrorObject] = logEvent.Exception;
                }

                foreach (var property in logEvent.Properties)
                {
                    fields[property.Key] = property.Value.ToString();
                }
            }
            catch (Exception logException)
            {
                fields["opentracing.contrib.netcore.error"] = logException.ToString();
            }

            span.Log(fields);
        }
    }
}
