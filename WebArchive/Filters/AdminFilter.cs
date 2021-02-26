using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;

namespace WebArchive.Filters
{
    public class AdminFilter : IAsyncActionFilter
    {
        private AdminConfig adminConfig;

        public AdminFilter(IConfiguration configuration)
        {
            adminConfig = new AdminConfig();
            configuration.Bind(adminConfig);
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            ((Microsoft.AspNetCore.Mvc.Controller) context.Controller).ViewBag.AdminConfig = adminConfig;
            await next();
        }
    }
}
