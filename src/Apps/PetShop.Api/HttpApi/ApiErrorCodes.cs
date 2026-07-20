namespace PetShop.Api.HttpApi;

public static class ApiErrorCodes
{
    public const string InvalidRequest = "request.invalid";
    public const string Unauthenticated = "auth.unauthenticated";
    public const string Forbidden = "auth.forbidden";
    public const string TenantRequired = "identity.tenant_required";
    public const string NotFound = "resource.not_found";
    public const string Conflict = "resource.conflict";
    public const string Unexpected = "server.unexpected";
}
