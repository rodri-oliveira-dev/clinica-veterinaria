namespace PetShop.Tutores.Domain;

internal sealed class Tutor : IEquatable<Tutor>
{
    private Tutor(
        TutorId id,
        TenantId tenantId,
        NomeDoTutor nome,
        Cpf? documento,
        Email? email,
        Telefone? telefone,
        SituacaoDoTutor situacao,
        DateTimeOffset criadoEm,
        DateTimeOffset atualizadoEm,
        DateTimeOffset? inativadoEm)
    {
        Id = id;
        TenantId = tenantId;
        Nome = nome;
        Documento = documento;
        Email = email;
        Telefone = telefone;
        Situacao = situacao;
        CriadoEm = criadoEm;
        AtualizadoEm = atualizadoEm;
        InativadoEm = inativadoEm;
    }

    public TutorId Id { get; }

    public TenantId TenantId { get; }

    public NomeDoTutor Nome { get; private set; }

    public Cpf? Documento { get; private set; }

    public Email? Email { get; private set; }

    public Telefone? Telefone { get; private set; }

    public SituacaoDoTutor Situacao { get; private set; }

    public DateTimeOffset CriadoEm { get; }

    public DateTimeOffset AtualizadoEm { get; private set; }

    public DateTimeOffset? InativadoEm { get; private set; }

    public bool EstaAtivo => Situacao == SituacaoDoTutor.Ativo;

    public static Tutor Cadastrar(
        TutorId id,
        TenantId tenantId,
        NomeDoTutor nome,
        Cpf? documento,
        Email? email,
        Telefone? telefone,
        DateTimeOffset cadastradoEm)
    {
        ValidarIdentidade(id, tenantId);
        ValidarContato(email, telefone);

        return new Tutor(
            id,
            tenantId,
            nome,
            documento,
            email,
            telefone,
            SituacaoDoTutor.Ativo,
            cadastradoEm,
            cadastradoEm,
            inativadoEm: null);
    }

    public void AlterarCadastro(
        NomeDoTutor nome,
        Cpf? documento,
        Email? email,
        Telefone? telefone,
        DateTimeOffset atualizadoEm)
    {
        ValidarContato(email, telefone);

        Nome = nome;
        Documento = documento;
        Email = email;
        Telefone = telefone;
        AtualizadoEm = atualizadoEm;
    }

    public void Inativar(DateTimeOffset inativadoEm)
    {
        if (Situacao == SituacaoDoTutor.Inativo)
        {
            throw new InvalidOperationException("O tutor ja esta inativo.");
        }

        Situacao = SituacaoDoTutor.Inativo;
        InativadoEm = inativadoEm;
        AtualizadoEm = inativadoEm;
    }

    public bool Equals(Tutor? other) =>
        other is not null &&
        Id == other.Id &&
        TenantId == other.TenantId;

    public override bool Equals(object? obj) => Equals(obj as Tutor);

    public override int GetHashCode() => HashCode.Combine(Id, TenantId);

    private static void ValidarIdentidade(TutorId id, TenantId tenantId)
    {
        if (id.EstaVazio)
        {
            throw new ArgumentException("O identificador do tutor deve ser informado.", nameof(id));
        }

        if (tenantId.EstaVazio)
        {
            throw new ArgumentException("O tenant do tutor deve ser informado.", nameof(tenantId));
        }
    }

    private static void ValidarContato(Email? email, Telefone? telefone)
    {
        if (email is null && telefone is null)
        {
            throw new ArgumentException("O tutor deve possuir ao menos um contato.");
        }
    }
}
