namespace PetShop.Tutores.Domain;

internal readonly record struct Raca
{
    private Raca(string valor)
    {
        Valor = valor;
    }

    public string Valor { get; }

    public static Raca Criar(string? valor)
    {
        string raca = valor?.Trim() ?? string.Empty;

        if (raca.Length == 0)
        {
            throw new ArgumentException("A raca do animal deve ser informada quando usada.", nameof(valor));
        }

        return new Raca(raca);
    }

    public override string ToString() => Valor;
}
