// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System.Collections.Generic;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace NBB.Todo.Api
{
    public class SwaggerTenantHeaderFilter : IOperationFilter
    { 
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null)
                operation.Parameters = new List<OpenApiParameter>();

            operation.Parameters.Add(new OpenApiParameter()
            {
                Name = "tenantid",
                In = ParameterLocation.Header,
                Schema = new OpenApiSchema { Type = "string", Format = "uuid" },
                Required = true // set to false if this is optional
            });
        }
    }
}
