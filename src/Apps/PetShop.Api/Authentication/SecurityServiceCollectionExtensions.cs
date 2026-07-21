using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

using PetShop.Api.HttpApi;
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
                if (authentication.ResolvedMetadataAddress is not null)
                {
                    options.MetadataAddress = authentication.ResolvedMetadataAddress;
                }

                options.Audience = authentication.ResolvedAudience;
                options.MapInboundClaims = false;
                options.RequireHttpsMetadata = authentication.ResolveRequireHttpsMetadata(environment);
                options.IncludeErrorDetails = false;
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
                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();
                        context.Response.Headers.WWWAuthenticate = "Bearer";

                        await ApiProblemDetailsWriter.WriteAsync(
                            context.HttpContext,
                            StatusCodes.Status401Unauthorized,
                            "Authentication is required.",
                            "A valid Bearer access credential is required to access this resource.",
                            ApiErrorCodes.Unauthenticated);
                    },
                    OnForbidden = context => ApiProblemDetailsWriter.WriteAsync(
                        context.HttpContext,
                        StatusCodes.Status403Forbidden,
                        "Access is forbidden.",
                        "The authenticated principal is not authorized to access this resource.",
                        ApiErrorCodes.Forbidden)
                };
            });

        services
            .AddAuthorizationBuilder()
            .AddPolicy(
                PetShopAuthorizationPolicies.DiagnosticsAccess,
                policy => ConfigureTenantScopedPolicy(policy, authentication))
            .AddPolicy(
                PetShopAuthorizationPolicies.TutoresAccess,
                policy => ConfigureTenantScopedPolicy(policy, authentication));

        return services;
    }

    private static void ConfigureTenantScopedPolicy(
        AuthorizationPolicyBuilder policy,
        PetShopAuthenticationOptions authentication)
    {
        policy.RequireAuthenticatedUser();
        policy.RequireAssertion(context =>
            TenantIdParser.TryGetTenantId(context.User, out _) == TenantClaimValidationError.None &&
            KeycloakResourceAccess.HasClientRole(
                context.User,
                authentication.ResolvedRoleClientId,
                authentication.ResolvedRequiredRole));
    }
}
