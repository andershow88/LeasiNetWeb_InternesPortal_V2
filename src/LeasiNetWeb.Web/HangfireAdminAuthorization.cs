using Hangfire.Dashboard;

namespace LeasiNetWeb.Web;

public class HangfireAdminAuthorization : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var http = context.GetHttpContext();
        return http.User.Identity?.IsAuthenticated == true
            && http.User.HasClaim("Rolle", "Administrator");
    }
}
