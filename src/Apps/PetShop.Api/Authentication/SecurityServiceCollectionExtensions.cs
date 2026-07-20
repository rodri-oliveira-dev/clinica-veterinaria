using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

using PetShop.Api.Tenancy;

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
                        TenantIdParser.TryGetTenantId(context.User, out _) == TenantClaimValidationError.None &&
                        KeycloakResourceAccess.HasClientRole(
                            context.User,
                            authentication.ResolvedRoleClientId,
                            authentication.ResolvedRequiredRole));
                });

        return services;
    }
}
