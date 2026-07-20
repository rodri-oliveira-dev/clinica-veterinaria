namespace PetShop.Tutores.Domain;

internal readonly record struct TutorId
{
    private TutorId(Guid valor)
    {
        Valor = valor;
    }

    public Guid Valor { get; }

    public bool EstaVazio => Valor == Guid.Empty;

    public static TutorId Criar(Guid valor)
    {
        if (valor == Guid.Empty)
        {
            throw new ArgumentException("O identificador do tutor deve ser informado.", nameof(valor));
        }

        return new TutorId(valor);
    }

    public static TutorId Novo() => Criar(Guid.NewGuid());

    public override string ToString() => Valor.ToString();
}
