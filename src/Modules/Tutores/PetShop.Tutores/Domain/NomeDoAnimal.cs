namespace PetShop.Tutores.Domain;

internal readonly record struct NomeDoAnimal
{
    private NomeDoAnimal(string valor)
    {
        Valor = valor;
    }

    public string Valor { get; }

    public static NomeDoAnimal Criar(string? valor)
    {
        string nome = valor?.Trim() ?? string.Empty;

        if (nome.Length == 0)
        {
            throw new ArgumentException("O nome do animal deve ser informado.", nameof(valor));
        }

        return new NomeDoAnimal(nome);
    }

    public override string ToString() => Valor;
}
