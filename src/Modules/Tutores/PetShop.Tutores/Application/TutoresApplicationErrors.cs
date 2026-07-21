namespace PetShop.Tutores.Application;

internal sealed class EntradaInvalidaException : Exception
{
    public EntradaInvalidaException(IReadOnlyDictionary<string, string[]> erros)
        : base("A entrada informada para o caso de uso de tutores e invalida.")
    {
        Erros = erros;
    }

    public IReadOnlyDictionary<string, string[]> Erros { get; }
}

internal sealed class TutorNaoEncontradoException : Exception
{
    public TutorNaoEncontradoException()
        : base("O tutor solicitado nao foi encontrado.")
    {
    }
}

internal sealed class DocumentoDoTutorDuplicadoException : Exception
{
    public DocumentoDoTutorDuplicadoException()
        : base("Ja existe tutor cadastrado com este CPF no tenant atual.")
    {
    }
}

internal sealed class TutorJaInativoException : Exception
{
    public TutorJaInativoException()
        : base("O tutor ja esta inativo.")
    {
    }
}

internal sealed class TutorComAnimaisAtivosVinculadosException : Exception
{
    public TutorComAnimaisAtivosVinculadosException()
        : base("O tutor possui animais ativos vinculados.")
    {
    }
}
