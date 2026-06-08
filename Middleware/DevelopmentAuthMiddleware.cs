using System.Security.Claims;

namespace MedTrack.Middleware
{
    public class DevelopmentAuthMiddleware
    {
        private readonly RequestDelegate _next;
        public DevelopmentAuthMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            var devRole = context.Request.Query["devRole"].ToString();
            if (!string.IsNullOrEmpty(devRole))
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, $"Dev {devRole}"),
                    new Claim(ClaimTypes.NameIdentifier, $"dev-{devRole.ToLower()}"),
                    new Claim(ClaimTypes.Email, $"dev@{devRole.ToLower()}.jo")
                };
                string roleName = devRole switch
                {
                    "SystemAdmin" => "System Admin",
                    "MOHAdmin" => "MOH Admin",
                    "Distributor" => "Distributor",
                    "PharmacyManager" => "Pharmacy Manager",
                    "PharmacyStaff" => "Pharmacy Staff",
                    _ => "Pharmacy Staff"
                };
                claims.Add(new Claim(ClaimTypes.Role, roleName));
                claims.Add(new Claim("PharmacyId", "1"));
                context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Development"));
            }
            await _next(context);
        }
    }
}
