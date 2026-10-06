// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Http;
using NBB.Correlation;
using System;
using ProblemDetailsOptions = Hellang.Middleware.ProblemDetails.ProblemDetailsOptions;

namespace NBB.Todo.Api
{
    public static class ProblemDetailsConfiguration
    {
        //TODO refactor ProblemDetails with AspNet.Core implementations 
        public static ProblemDetailsOptions Configure(ProblemDetailsOptions options, bool includeExceptionDetails = true)
        {
            includeExceptionDetails = includeExceptionDetails && Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

            options.IncludeExceptionDetails = (_context, _exception) => includeExceptionDetails;
            options.MapStatusCode = context => new StatusCodeProblemDetails(context.Response.StatusCode);

            options.Map<NotImplementedException>(_ex => new StatusCodeProblemDetails(StatusCodes.Status501NotImplemented));

            options.Map<Exception>(ex =>
            {
                var det = new ProblemDetailsException
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Unexpected error",
                    CorrelationId = CorrelationManager.GetCorrelationId()?.ToString(),
                    Detail = includeExceptionDetails ? ex.Message : "Unexpected error",
                    Type = ex.GetType().FullName
                };
                return det;
            });

            return options;
        }
    }

    public class ProblemDetailsException : Microsoft.AspNetCore.Mvc.ProblemDetails
    {
        public ProblemDetailsException()
        {
        }

        public string CorrelationId { get; set; }
    }
}
