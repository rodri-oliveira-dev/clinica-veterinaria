namespace PetShop.Tutores.Domain;

internal readonly record struct ObservacaoCadastral
{
    private ObservacaoCadastral(string valor)
    {
        Valor = valor;
    }

    public string Valor { get; }

    public static ObservacaoCadastral Criar(string? valor)
    {
        string observacao = valor?.Trim() ?? string.Empty;

        if (observacao.Length == 0)
        {
            throw new ArgumentException("A observacao cadastral do animal deve ser informada quando usada.", nameof(valor));
        }

        return new ObservacaoCadastral(observacao);
    }

    public override string ToString() => Valor;
}
