namespace PetShop.Tutores.Domain;

internal readonly record struct DataDoFalecimento
{
    private DataDoFalecimento(DateOnly valor)
    {
        Valor = valor;
    }

    public DateOnly Valor { get; }

    public static DataDoFalecimento Criar(DateOnly valor, DateOnly dataAtual)
    {
        if (valor > dataAtual)
        {
            throw new ArgumentException("A data do falecimento do animal nao pode estar no futuro.", nameof(valor));
        }

        return new DataDoFalecimento(valor);
    }

    public override string ToString() => Valor.ToString("O", System.Globalization.CultureInfo.InvariantCulture);
}
