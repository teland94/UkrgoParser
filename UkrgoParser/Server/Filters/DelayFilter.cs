using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;

namespace UkrgoParser.Server.Filters
{
    public class DelayFilter : ActionFilterAttribute, IAsyncActionFilter
    {
        private readonly int _delayInMs;

        public DelayFilter(IConfiguration configuration)
        {
            _delayInMs = configuration.GetValue("ApiDelayDuration", 0);
        }

        async Task IAsyncActionFilter.OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            await Task.Delay(_delayInMs);
            await next();
        }
    }
}
