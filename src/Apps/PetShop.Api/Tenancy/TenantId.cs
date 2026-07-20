namespace PetShop.Api.Tenancy;

public readonly record struct TenantId
{
    public TenantId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Tenant id must not be empty.", nameof(value));
        }

        Value = value;
    }

    public Guid Value { get; }

    public static bool TryParse(string? value, out TenantId tenantId)
    {
        tenantId = default;

        if (string.IsNullOrWhiteSpace(value) ||
            !Guid.TryParse(value, out Guid parsed) ||
            parsed == Guid.Empty)
        {
            return false;
        }

        tenantId = new TenantId(parsed);
        return true;
    }

    public override string ToString() => Value.ToString("D");
}
