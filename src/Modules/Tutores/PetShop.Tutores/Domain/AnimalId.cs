namespace PetShop.Tutores.Domain;

internal readonly record struct AnimalId
{
    private AnimalId(Guid valor)
    {
        Valor = valor;
    }

    public Guid Valor { get; }

    public bool EstaVazio => Valor == Guid.Empty;

    public static AnimalId Criar(Guid valor)
    {
        if (valor == Guid.Empty)
        {
            throw new ArgumentException("O identificador do animal deve ser informado.", nameof(valor));
        }

        return new AnimalId(valor);
    }

    public static AnimalId Novo() => Criar(Guid.NewGuid());

    public override string ToString() => Valor.ToString();
}
