using System.Security.Claims;

using PetShop.Api.Tenancy;

namespace PetShop.UnitTests;

public sealed class TenantIdParserTests
{
    private const string TenantA = "11111111-1111-4111-8111-111111111111";
    private const string TenantB = "22222222-2222-4222-8222-222222222222";

    [Fact]
    public void TryGetTenantId_WithSingleValidGuidClaim_ReturnsTenantId()
    {
        ClaimsPrincipal principal = CreatePrincipal(new Claim(TenantIdParser.ClaimType, TenantA));

        TenantClaimValidationError error = TenantIdParser.TryGetTenantId(principal, out TenantId tenantId);

        Assert.Equal(TenantClaimValidationError.None, error);
        Assert.Equal(TenantA, tenantId.ToString());
    }

    [Fact]
    public void TryGetTenantId_WithoutTenantClaim_ReturnsMissing()
    {
        ClaimsPrincipal principal = CreatePrincipal();

        TenantClaimValidationError error = TenantIdParser.TryGetTenantId(principal, out _);

        Assert.Equal(TenantClaimValidationError.Missing, error);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void TryGetTenantId_WithBlankTenantClaim_ReturnsEmpty(string value)
    {
        ClaimsPrincipal principal = CreatePrincipal(new Claim(TenantIdParser.ClaimType, value));

        TenantClaimValidationError error = TenantIdParser.TryGetTenantId(principal, out _);

        Assert.Equal(TenantClaimValidationError.Empty, error);
    }

    [Theory]
    [InlineData("not-a-guid")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public void TryGetTenantId_WithInvalidTenantClaim_ReturnsInvalid(string value)
    {
        ClaimsPrincipal principal = CreatePrincipal(new Claim(TenantIdParser.ClaimType, value));

        TenantClaimValidationError error = TenantIdParser.TryGetTenantId(principal, out _);

        Assert.Equal(TenantClaimValidationError.Invalid, error);
    }

    [Fact]
    public void TryGetTenantId_WithMultipleTenantClaims_ReturnsMultiple()
    {
        ClaimsPrincipal principal = CreatePrincipal(
            new Claim(TenantIdParser.ClaimType, TenantA),
            new Claim(TenantIdParser.ClaimType, TenantB));

        TenantClaimValidationError error = TenantIdParser.TryGetTenantId(principal, out _);

        Assert.Equal(TenantClaimValidationError.Multiple, error);
    }

    private static ClaimsPrincipal CreatePrincipal(params Claim[] claims)
    {
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }
}
