namespace PetShop.Tutores.Domain;

internal sealed class Animal : IEquatable<Animal>
{
    private Animal(
        AnimalId id,
        TenantId tenantId,
        NomeDoAnimal nome,
        Especie especie,
        Raca? raca,
        SexoDoAnimal sexo,
        DataDeNascimento? dataDeNascimento,
        CorOuPelagem? corOuPelagem,
        ObservacaoCadastral? observacaoCadastral,
        SituacaoDoAnimal situacao,
        TutorId tutorResponsavelId,
        int versao,
        DateTimeOffset criadoEm,
        DateTimeOffset atualizadoEm,
        DateTimeOffset? inativadoEm)
    {
        Id = id;
        TenantId = tenantId;
        Nome = nome;
        Especie = especie;
        Raca = raca;
        Sexo = sexo;
        DataDeNascimento = dataDeNascimento;
        CorOuPelagem = corOuPelagem;
        ObservacaoCadastral = observacaoCadastral;
        Situacao = situacao;
        TutorResponsavelId = tutorResponsavelId;
        Versao = versao;
        CriadoEm = criadoEm;
        AtualizadoEm = atualizadoEm;
        InativadoEm = inativadoEm;
    }

    public AnimalId Id { get; }

    public TenantId TenantId { get; }

    public NomeDoAnimal Nome { get; private set; }

    public Especie Especie { get; private set; }

    public Raca? Raca { get; private set; }

    public SexoDoAnimal Sexo { get; private set; }

    public DataDeNascimento? DataDeNascimento { get; private set; }

    public CorOuPelagem? CorOuPelagem { get; private set; }

    public ObservacaoCadastral? ObservacaoCadastral { get; private set; }

    public SituacaoDoAnimal Situacao { get; private set; }

    public TutorResponsavel TutorResponsavel => TutorResponsavel.Criar(TutorResponsavelId.Valor);

    internal TutorId TutorResponsavelId { get; private set; }

    public int Versao { get; private set; }

    public DateTimeOffset CriadoEm { get; }

    public DateTimeOffset AtualizadoEm { get; private set; }

    public DateTimeOffset? InativadoEm { get; private set; }

    public bool EstaAtivo => Situacao == SituacaoDoAnimal.Ativo;

    public static Animal Cadastrar(
        AnimalId id,
        TenantId tenantId,
        NomeDoAnimal nome,
        Especie especie,
        Raca? raca,
        SexoDoAnimal sexo,
        DataDeNascimento? dataDeNascimento,
        CorOuPelagem? corOuPelagem,
        ObservacaoCadastral? observacaoCadastral,
        TutorResponsavel tutorResponsavel,
        DateTimeOffset cadastradoEm)
    {
        ValidarIdentidade(id, tenantId);
        ValidarSexo(sexo);

        return new Animal(
            id,
            tenantId,
            nome,
            especie,
            raca,
            sexo,
            dataDeNascimento,
            corOuPelagem,
            observacaoCadastral,
            SituacaoDoAnimal.Ativo,
            TutorId.Criar(tutorResponsavel.TutorId),
            versao: 1,
            cadastradoEm,
            cadastradoEm,
            inativadoEm: null);
    }

    public void AlterarCadastro(
        NomeDoAnimal nome,
        Especie especie,
        Raca? raca,
        SexoDoAnimal sexo,
        DataDeNascimento? dataDeNascimento,
        CorOuPelagem? corOuPelagem,
        ObservacaoCadastral? observacaoCadastral,
        DateTimeOffset atualizadoEm)
    {
        ValidarSexo(sexo);

        Nome = nome;
        Especie = especie;
        Raca = raca;
        Sexo = sexo;
        DataDeNascimento = dataDeNascimento;
        CorOuPelagem = corOuPelagem;
        ObservacaoCadastral = observacaoCadastral;
        AtualizadoEm = atualizadoEm;
        Versao++;
    }

    public void Inativar(DateTimeOffset inativadoEm)
    {
        if (Situacao == SituacaoDoAnimal.Inativo)
        {
            throw new InvalidOperationException("O animal ja esta inativo.");
        }

        Situacao = SituacaoDoAnimal.Inativo;
        InativadoEm = inativadoEm;
        AtualizadoEm = inativadoEm;
        Versao++;
    }

    public void TransferirResponsabilidade(
        TutorResponsavel novoTutorResponsavel,
        DateTimeOffset transferidoEm)
    {
        TutorId novoTutorId = TutorId.Criar(novoTutorResponsavel.TutorId);

        if (TutorResponsavelId == novoTutorId)
        {
            throw new InvalidOperationException("O novo tutor responsavel deve ser diferente do tutor atual.");
        }

        TutorResponsavelId = novoTutorId;
        AtualizadoEm = transferidoEm;
        Versao++;
    }

    public bool Equals(Animal? other) =>
        other is not null &&
        Id == other.Id &&
        TenantId == other.TenantId;

    public override bool Equals(object? obj) => Equals(obj as Animal);

    public override int GetHashCode() => HashCode.Combine(Id, TenantId);

    private static void ValidarIdentidade(AnimalId id, TenantId tenantId)
    {
        if (id.EstaVazio)
        {
            throw new ArgumentException("O identificador do animal deve ser informado.", nameof(id));
        }

        if (tenantId.EstaVazio)
        {
            throw new ArgumentException("O tenant do animal deve ser informado.", nameof(tenantId));
        }
    }

    private static void ValidarSexo(SexoDoAnimal sexo)
    {
        if (!Enum.IsDefined(sexo))
        {
            throw new ArgumentOutOfRangeException(nameof(sexo), sexo, "O sexo do animal deve ser valido.");
        }
    }
}
