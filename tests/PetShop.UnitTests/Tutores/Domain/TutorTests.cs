using PetShop.Tutores.Domain;

namespace PetShop.UnitTests.Tutores.Domain;

public sealed class TutorTests
{
    private static readonly TutorId TutorId = TutorId.Criar(Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"));
    private static readonly TenantId TenantA = TenantId.Criar(Guid.Parse("11111111-1111-4111-8111-111111111111"));
    private static readonly TenantId TenantB = TenantId.Criar(Guid.Parse("22222222-2222-4222-8222-222222222222"));
    private static readonly DateTimeOffset CriadoEm = new(2026, 7, 20, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Cadastrar_ComDadosValidos_CriaTutorAtivo()
    {
        Tutor tutor = CriarTutor();

        Assert.Equal(TutorId, tutor.Id);
        Assert.Equal(TenantA, tutor.TenantId);
        Assert.Equal("Maria Oliveira", tutor.Nome.Valor);
        Assert.Equal("52998224725", tutor.Documento?.Valor);
        Assert.Equal("maria@example.com", tutor.Email?.Valor);
        Assert.Equal("11987654321", tutor.Telefone?.Valor);
        Assert.Equal(SituacaoDoTutor.Ativo, tutor.Situacao);
        Assert.True(tutor.EstaAtivo);
        Assert.Equal(CriadoEm, tutor.CriadoEm);
        Assert.Equal(CriadoEm, tutor.AtualizadoEm);
        Assert.Null(tutor.InativadoEm);
    }

    [Fact]
    public void Cadastrar_SemDocumento_AceitaCpfOpcional()
    {
        Tutor tutor = Tutor.Cadastrar(
            TutorId,
            TenantA,
            NomeDoTutor.Criar("Maria Oliveira"),
            documento: null,
            Email.Criar("maria@example.com"),
            telefone: null,
            CriadoEm);

        Assert.Null(tutor.Documento);
        Assert.Equal("maria@example.com", tutor.Email?.Valor);
        Assert.Null(tutor.Telefone);
    }

    [Theory]
    [InlineData(null, "(11) 98765-4321")]
    [InlineData("maria@example.com", null)]
    public void Cadastrar_ComAoMenosUmContato_AceitaCamposDeContatoOpcionais(
        string? email,
        string? telefone)
    {
        Tutor tutor = Tutor.Cadastrar(
            TutorId,
            TenantA,
            NomeDoTutor.Criar("Maria Oliveira"),
            documento: null,
            email is null ? null : Email.Criar(email),
            telefone is null ? null : Telefone.Criar(telefone),
            CriadoEm);

        Assert.True(tutor.Email is not null || tutor.Telefone is not null);
    }

    [Fact]
    public void Cadastrar_SemContato_RejeitaTutor()
    {
        ArgumentException exception = Assert.Throws<ArgumentException>(() => Tutor.Cadastrar(
            TutorId,
            TenantA,
            NomeDoTutor.Criar("Maria Oliveira"),
            documento: null,
            email: null,
            telefone: null,
            CriadoEm));

        Assert.Contains("ao menos um contato", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Cadastrar_ComIdentificadorVazio_Rejeita()
    {
        ArgumentException exception = Assert.Throws<ArgumentException>(() => Tutor.Cadastrar(
            default,
            TenantA,
            NomeDoTutor.Criar("Maria Oliveira"),
            documento: null,
            Email.Criar("maria@example.com"),
            telefone: null,
            CriadoEm));

        Assert.Equal("id", exception.ParamName);
    }

    [Fact]
    public void Cadastrar_ComTenantVazio_Rejeita()
    {
        ArgumentException exception = Assert.Throws<ArgumentException>(() => Tutor.Cadastrar(
            TutorId,
            default,
            NomeDoTutor.Criar("Maria Oliveira"),
            documento: null,
            Email.Criar("maria@example.com"),
            telefone: null,
            CriadoEm));

        Assert.Equal("tenantId", exception.ParamName);
    }

    [Fact]
    public void AlterarCadastro_ComDadosValidos_AtualizaCamposETimestamp()
    {
        Tutor tutor = CriarTutor();
        DateTimeOffset atualizadoEm = CriadoEm.AddHours(2);

        tutor.AlterarCadastro(
            NomeDoTutor.Criar("Maria Santos"),
            documento: null,
            Email.Criar("maria.santos@example.com"),
            telefone: null,
            atualizadoEm);

        Assert.Equal("Maria Santos", tutor.Nome.Valor);
        Assert.Null(tutor.Documento);
        Assert.Equal("maria.santos@example.com", tutor.Email?.Valor);
        Assert.Null(tutor.Telefone);
        Assert.Equal(atualizadoEm, tutor.AtualizadoEm);
        Assert.Equal(CriadoEm, tutor.CriadoEm);
        Assert.Equal(TutorId, tutor.Id);
        Assert.Equal(TenantA, tutor.TenantId);
    }

    [Fact]
    public void AlterarCadastro_SemContato_RejeitaAlteracao()
    {
        Tutor tutor = CriarTutor();

        Assert.Throws<ArgumentException>(() => tutor.AlterarCadastro(
            NomeDoTutor.Criar("Maria Santos"),
            documento: null,
            email: null,
            telefone: null,
            CriadoEm.AddHours(1)));
    }

    [Fact]
    public void Inativar_TutorAtivo_MarcaInativoEAtualizaTimestamp()
    {
        Tutor tutor = CriarTutor();
        DateTimeOffset inativadoEm = CriadoEm.AddDays(1);

        tutor.Inativar(inativadoEm);

        Assert.False(tutor.EstaAtivo);
        Assert.Equal(SituacaoDoTutor.Inativo, tutor.Situacao);
        Assert.Equal(inativadoEm, tutor.InativadoEm);
        Assert.Equal(inativadoEm, tutor.AtualizadoEm);
    }

    [Fact]
    public void Inativar_TutorJaInativo_RejeitaNovaInativacao()
    {
        Tutor tutor = CriarTutor();
        tutor.Inativar(CriadoEm.AddDays(1));

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            tutor.Inativar(CriadoEm.AddDays(2)));

        Assert.Contains("ja esta inativo", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Equals_ComMesmoIdETenant_ConsideraMesmoTutor()
    {
        Tutor tutor = CriarTutor();
        Tutor mesmoTutor = CriarTutor();

        Assert.Equal(tutor, mesmoTutor);
        Assert.Equal(tutor.GetHashCode(), mesmoTutor.GetHashCode());
    }

    [Fact]
    public void Equals_ComMesmoIdEmTenantDiferente_NaoConsideraMesmoTutor()
    {
        Tutor tutorTenantA = CriarTutor();
        Tutor tutorTenantB = Tutor.Cadastrar(
            TutorId,
            TenantB,
            NomeDoTutor.Criar("Maria Oliveira"),
            documento: null,
            Email.Criar("maria@example.com"),
            telefone: null,
            CriadoEm);

        Assert.NotEqual(tutorTenantA, tutorTenantB);
        Assert.Equal(TenantA, tutorTenantA.TenantId);
        Assert.Equal(TenantB, tutorTenantB.TenantId);
    }

    private static Tutor CriarTutor() =>
        Tutor.Cadastrar(
            TutorId,
            TenantA,
            NomeDoTutor.Criar("Maria Oliveira"),
            Cpf.Criar("529.982.247-25"),
            Email.Criar("Maria@Example.com"),
            Telefone.Criar("(11) 98765-4321"),
            CriadoEm);
}
