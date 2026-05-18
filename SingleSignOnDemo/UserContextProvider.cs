using SaaSDataLayer;

namespace SingleSignOnDemo
{
    public class UserContextProvider(IHttpContextAccessor httpContextAccessor) : IUserContextProvider
    {
        public UserContext GetUserContext()
        {
            var claims = httpContextAccessor.HttpContext.User.Claims;

            return new UserContext
            {
                TenantId = claims.First(c => c.Type == "v_id")?.Value,
                VerticalId = claims.First(c => c.Type == "t_id")?.Value,
                ClientId = claims.First(c => c.Type == "client_id")?.Value,
                Email = claims.First(c => c.Type == "email")?.Value
            };
        }
    }
}
