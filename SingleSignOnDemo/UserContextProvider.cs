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
                TenantId = claims.FirstOrDefault(c => c.Type == "v_id")?.Value,
                VerticalId = claims.FirstOrDefault(c => c.Type == "t_id")?.Value,
                ClientId = claims.FirstOrDefault(c => c.Type == "client_id")?.Value
            };
        }
    }
}
