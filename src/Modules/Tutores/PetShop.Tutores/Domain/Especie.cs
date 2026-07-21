namespace PetShop.Tutores.Domain;

internal readonly record struct Especie
{
    private Especie(string valor)
    {
        Valor = valor;
    }

    public string Valor { get; }

    public static Especie Criar(string? valor)
    {
        string especie = valor?.Trim() ?? string.Empty;

        if (especie.Length == 0)
        {
            throw new ArgumentException("A especie do animal deve ser informada.", nameof(valor));
        }

        return new Especie(especie);
    }

    public override string ToString() => Valor;
}
