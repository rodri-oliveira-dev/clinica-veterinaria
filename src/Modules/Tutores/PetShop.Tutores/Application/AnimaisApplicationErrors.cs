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

internal sealed class TutorResponsavelInativoException : Exception
{
    public TutorResponsavelInativoException()
        : base("O tutor responsavel informado esta inativo.")
    {
    }
}

internal sealed class MesmoTutorResponsavelException : Exception
{
    public MesmoTutorResponsavelException()
        : base("O novo tutor responsavel deve ser diferente do tutor atual.")
    {
    }
}

internal sealed class ConflitoDeConcorrenciaDoAnimalException : Exception
{
    public ConflitoDeConcorrenciaDoAnimalException()
        : base("A versao informada para o animal esta desatualizada.")
    {
    }
}

internal sealed class SubjectAutenticadoNaoEncontradoException : Exception
{
    public SubjectAutenticadoNaoEncontradoException()
        : base("O subject autenticado nao foi encontrado.")
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

internal sealed class AnimalInativoException : Exception
{
    public AnimalInativoException()
        : base("O animal informado esta inativo.")
    {
    }
}
