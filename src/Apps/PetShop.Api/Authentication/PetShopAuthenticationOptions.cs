namespace PetShop.Api.Authentication;

internal sealed class PetShopAuthenticationOptions
{
    public const string SectionName = "Authentication";

    public string? Authority { get; set; }

    public string? Audience { get; set; }

    public string? RoleClientId { get; set; }

    public string? RequiredRole { get; set; }

    public bool? RequireHttpsMetadata { get; set; }

    public string ResolvedAuthority => Authority!.TrimEnd('/');

    public string ResolvedAudience => Audience!.Trim();

    public string ResolvedRoleClientId => string.IsNullOrWhiteSpace(RoleClientId)
        ? ResolvedAudience
        : RoleClientId.Trim();

    public string ResolvedRequiredRole => RequiredRole!.Trim();

    public bool ResolveRequireHttpsMetadata(IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(environment);

        return RequireHttpsMetadata ?? !environment.IsDevelopment();
    }

    public void Validate(IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(environment);

        RequireNonEmpty(Authority, nameof(Authority));
        RequireNonEmpty(Audience, nameof(Audience));
        RequireNonEmpty(RequiredRole, nameof(RequiredRole));

        if (!Uri.TryCreate(ResolvedAuthority, UriKind.Absolute, out Uri? authorityUri))
        {
            throw new InvalidOperationException(
                $"Authentication authority '{ResolvedAuthority}' must be an absolute URI.");
        }

        bool requireHttpsMetadata = ResolveRequireHttpsMetadata(environment);

        if (requireHttpsMetadata && authorityUri.Scheme != Uri.UriSchemeHttps)
        {
            throw new InvalidOperationException(
                "Authentication authority must use HTTPS when HTTPS metadata is required.");
        }

        if (authorityUri.Scheme != Uri.UriSchemeHttps && authorityUri.Scheme != Uri.UriSchemeHttp)
        {
            throw new InvalidOperationException(
                "Authentication authority must use HTTP or HTTPS.");
        }
    }

    private static void RequireNonEmpty(string? value, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(
                $"Authentication option '{SectionName}:{name}' is required.");
        }
    }
}
