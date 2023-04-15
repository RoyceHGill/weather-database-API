using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MongoWeatherAPI.Models;
using MongoWeatherAPI.Repository.Interfaces;

namespace MongoWeatherAPI.Attributes
{
    [AttributeUsage(validOn: AttributeTargets.Method | AttributeTargets.Class)]
    public class ApiKeyAttribute : Attribute, IAsyncActionFilter
    {
        private string requiredRole;
         
        public string RequiredRole
        {
            get { return requiredRole; } 
        }

        public ApiKeyAttribute(string role = "Admin")
        {
            requiredRole = role;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.HttpContext.Request.Headers.TryGetValue("ApiKey", out var key))
            {
                context.Result = new ContentResult
                {
                    StatusCode = 401,
                    Content = "No API Key provided"
                };
                return;
            }

            var betterKey = key.ToString().Trim('{', '}');

            var userRepo = context.HttpContext.RequestServices.GetRequiredService<IApiUserRepository>();

            if (!Enum.TryParse(RequiredRole, out Roles specifiedRole))
            {
                return;
            }

            if (userRepo.AuthenticateUser(betterKey, specifiedRole) == null) 
            {
                context.Result = new ContentResult
                {
                    StatusCode = 403,
                    Content = "User does not exist or is not authorized."
                };
                return;
            }

            await next();
        }
    }
}
