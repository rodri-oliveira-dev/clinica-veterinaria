namespace PetShop.Tutores.Domain;

internal readonly record struct TenantId
{
    private TenantId(Guid valor)
    {
        Valor = valor;
    }

    public Guid Valor { get; }

    public bool EstaVazio => Valor == Guid.Empty;

    public static TenantId Criar(Guid valor)
    {
        if (valor == Guid.Empty)
        {
            throw new ArgumentException("O tenant do tutor deve ser informado.", nameof(valor));
        }

        return new TenantId(valor);
    }

    public override string ToString() => Valor.ToString();
}
