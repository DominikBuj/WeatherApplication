using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ApiKeyAuth : Attribute, IAsyncActionFilter
    {
        private readonly string apiKeyHeaderName = "ApiKey";

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.HttpContext.Request.Headers.TryGetValue(apiKeyHeaderName, out var apiKeyFromHeader))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            IConfiguration configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            string apiKey = configuration.GetValue<string>("ClientApiKey");

            if (!String.Equals(apiKey, apiKeyFromHeader))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            await next();
        }
    }
}
