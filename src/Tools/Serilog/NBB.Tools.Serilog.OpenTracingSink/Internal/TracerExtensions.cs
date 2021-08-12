// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using OpenTracing;
using OpenTracing.Noop;
using OpenTracing.Util;

namespace NBB.Tools.Serilog.OpenTracingSink.Internal
{
    internal static class TracerExtensions
    {
        public static bool IsNoopTracer(this ITracer tracer)
        {
            if (tracer is NoopTracer)
                return true;

            if (tracer is GlobalTracer && !GlobalTracer.IsRegistered())
                return true;

            return false;
        }
    }
}
