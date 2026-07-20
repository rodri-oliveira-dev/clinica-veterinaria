namespace PetShop.Tutores.Application;

internal interface IContextoTenantAtual
{
    Guid? TenantId { get; }
}
