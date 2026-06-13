using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace MedTrack.Middleware
{
    public class AppSessionMiddleware
    {
        private static readonly string InstanceId = Guid.NewGuid().ToString("N");
        public const string InstanceClaimType = "MedTrackInstance";

        private readonly RequestDelegate _next;

        public AppSessionMiddleware(RequestDelegate next) => _next = next;

        public static string CurrentInstanceId => InstanceId;

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var instanceClaim = context.User.FindFirst(InstanceClaimType);
                if (instanceClaim?.Value != InstanceId)
                {
                    await context.SignOutAsync(IdentityConstants.ApplicationScheme);
                    context.Response.Redirect("/");
                    return;
                }
            }

            await _next(context);
        }
    }
}
