using api.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace api.Attributes
{
    // Bo cung cap Policy dong dua tren MaQuyen trong Attribute
    public class QuyenHanPolicyProvider : IAuthorizationPolicyProvider
    {
        public DefaultAuthorizationPolicyProvider BacalProvider { get; }

        public QuyenHanPolicyProvider(IOptions<AuthorizationOptions> options)
        {
            BacalProvider = new DefaultAuthorizationPolicyProvider(options);
        }

        public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => BacalProvider.GetDefaultPolicyAsync();

        public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => BacalProvider.GetFallbackPolicyAsync();

        public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            // Neu policyName kien kieu MaQuyen (vi du: USER_CREATE)
            var policy = new AuthorizationPolicyBuilder();
            policy.RequireAuthenticatedUser(); // Bat buoc phai dang nhap
            policy.AddRequirements(new QuyenHanRequirement(policyName));
            return Task.FromResult<AuthorizationPolicy?>(policy.Build());
        }
    }
}
