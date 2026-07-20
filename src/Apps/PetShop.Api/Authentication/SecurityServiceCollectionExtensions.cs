using System.Security.Claims;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

using PetShop.Observability.Propagation;

namespace PetShop.Api.Authentication;

internal static class SecurityServiceCollectionExtensions
{
    public static IServiceCollection AddPetShopApiSecurity(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(environment);

        var authentication = new PetShopAuthenticationOptions();
        configuration.GetSection(PetShopAuthenticationOptions.SectionName).Bind(authentication);
        authentication.Validate(environment);

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = authentication.ResolvedAuthority;
                options.Audience = authentication.ResolvedAudience;
                options.MapInboundClaims = false;
                options.RequireHttpsMetadata = authentication.ResolveRequireHttpsMetadata(environment);
                options.IncludeErrorDetails = environment.IsDevelopment();
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = authentication.ResolvedAuthority,
                    ValidateAudience = true,
                    ValidAudience = authentication.ResolvedAudience,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1),
                    NameClaimType = "preferred_username"
                };
            });

        services
            .AddAuthorizationBuilder()
            .AddPolicy(
                PetShopAuthorizationPolicies.DiagnosticsAccess,
                policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireAssertion(context =>
                        HasTenantClaim(context.User) &&
                        KeycloakResourceAccess.HasClientRole(
                            context.User,
                            authentication.ResolvedRoleClientId,
                            authentication.ResolvedRequiredRole));
                });

        return services;
    }

    private static bool HasTenantClaim(ClaimsPrincipal user)
    {
        string? tenantId = user.FindFirst(PropagationHeaderNames.TenantId)?.Value;

        return !string.IsNullOrWhiteSpace(tenantId);
    }
}
