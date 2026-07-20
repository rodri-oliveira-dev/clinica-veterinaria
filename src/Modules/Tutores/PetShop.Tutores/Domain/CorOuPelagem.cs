namespace PetShop.Tutores.Domain;

internal readonly record struct CorOuPelagem
{
    private CorOuPelagem(string valor)
    {
        Valor = valor;
    }

    public string Valor { get; }

    public static CorOuPelagem Criar(string? valor)
    {
        string corOuPelagem = valor?.Trim() ?? string.Empty;

        if (corOuPelagem.Length == 0)
        {
            throw new ArgumentException("A cor ou pelagem do animal deve ser informada quando usada.", nameof(valor));
        }

        return new CorOuPelagem(corOuPelagem);
    }

    public override string ToString() => Valor;
}
