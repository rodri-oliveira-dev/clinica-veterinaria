namespace PetShop.Tutores.Domain;

internal readonly record struct DataDeNascimento
{
    private DataDeNascimento(DateOnly valor)
    {
        Valor = valor;
    }

    public DateOnly Valor { get; }

    public static DataDeNascimento Criar(DateOnly valor, DateOnly dataAtual)
    {
        if (valor > dataAtual)
        {
            throw new ArgumentException("A data de nascimento do animal nao pode estar no futuro.", nameof(valor));
        }

        return new DataDeNascimento(valor);
    }

    public override string ToString() => Valor.ToString("O", System.Globalization.CultureInfo.InvariantCulture);
}
