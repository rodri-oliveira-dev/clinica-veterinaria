namespace PetShop.Tutores.Domain;

internal readonly record struct NomeDoTutor
{
    private NomeDoTutor(string valor)
    {
        Valor = valor;
    }

    public string Valor { get; }

    public static NomeDoTutor Criar(string? valor)
    {
        string nome = valor?.Trim() ?? string.Empty;

        if (nome.Length == 0)
        {
            throw new ArgumentException("O nome do tutor deve ser informado.", nameof(valor));
        }

        return new NomeDoTutor(nome);
    }

    public override string ToString() => Valor;
}
