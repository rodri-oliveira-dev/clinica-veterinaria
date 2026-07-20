namespace PetShop.Tutores.Application;

internal sealed class AnimalNaoEncontradoException : Exception
{
    public AnimalNaoEncontradoException()
        : base("O animal solicitado nao foi encontrado.")
    {
    }
}

internal sealed class TutorResponsavelNaoEncontradoException : Exception
{
    public TutorResponsavelNaoEncontradoException()
        : base("O tutor responsavel pelo animal nao foi encontrado.")
    {
    }
}

internal sealed class AnimalJaInativoException : Exception
{
    public AnimalJaInativoException()
        : base("O animal ja esta inativo.")
    {
    }
}
