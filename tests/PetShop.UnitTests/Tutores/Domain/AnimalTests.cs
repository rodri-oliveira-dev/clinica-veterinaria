using PetShop.Tutores.Domain;

namespace PetShop.UnitTests.Tutores.Domain;

public sealed class AnimalTests
{
    private static readonly AnimalId AnimalId = AnimalId.Criar(Guid.Parse("bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb"));
    private static readonly Guid TutorId = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa");
    private static readonly Guid OutroTutorId = Guid.Parse("cccccccc-cccc-4ccc-8ccc-cccccccccccc");
    private static readonly TenantId TenantA = TenantId.Criar(Guid.Parse("11111111-1111-4111-8111-111111111111"));
    private static readonly TenantId TenantB = TenantId.Criar(Guid.Parse("22222222-2222-4222-8222-222222222222"));
    private static readonly DateOnly Hoje = new(2026, 7, 20);
    private static readonly DateTimeOffset CriadoEm = new(2026, 7, 20, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Cadastrar_ComDadosValidos_CriaAnimalAtivo()
    {
        Animal animal = CriarAnimal();

        Assert.Equal(AnimalId, animal.Id);
        Assert.Equal(TenantA, animal.TenantId);
        Assert.Equal("Luna", animal.Nome.Valor);
        Assert.Equal("Canina", animal.Especie.Valor);
        Assert.Equal("SRD", animal.Raca?.Valor);
        Assert.Equal(SexoDoAnimal.Femea, animal.Sexo);
        Assert.Equal(new DateOnly(2020, 5, 2), animal.DataDeNascimento?.Valor);
        Assert.Null(animal.DataDoFalecimento);
        Assert.Equal("Caramelo", animal.CorOuPelagem?.Valor);
        Assert.Equal("Resgata com medo de fogos", animal.ObservacaoCadastral?.Valor);
        Assert.Equal(TutorId, animal.TutorResponsavel.TutorId);
        Assert.Equal(SituacaoDoAnimal.Ativo, animal.Situacao);
        Assert.True(animal.EstaAtivo);
        Assert.Equal(CriadoEm, animal.CriadoEm);
        Assert.Equal(CriadoEm, animal.AtualizadoEm);
        Assert.Equal(1, animal.Versao);
        Assert.Null(animal.InativadoEm);
    }

    [Fact]
    public void Cadastrar_ComCamposOpcionaisAusentes_CriaAnimal()
    {
        Animal animal = Animal.Cadastrar(
            AnimalId,
            TenantA,
            NomeDoAnimal.Criar("Luna"),
            Especie.Criar("Canina"),
            raca: null,
            SexoDoAnimal.NaoInformado,
            dataDeNascimento: null,
            corOuPelagem: null,
            observacaoCadastral: null,
            TutorResponsavel.Criar(TutorId),
            CriadoEm);

        Assert.Null(animal.Raca);
        Assert.Equal(SexoDoAnimal.NaoInformado, animal.Sexo);
        Assert.Null(animal.DataDeNascimento);
        Assert.Null(animal.DataDoFalecimento);
        Assert.Null(animal.CorOuPelagem);
        Assert.Null(animal.ObservacaoCadastral);
    }

    [Fact]
    public void Cadastrar_ComIdentificadorVazio_Rejeita()
    {
        ArgumentException exception = Assert.Throws<ArgumentException>(() => Animal.Cadastrar(
            default,
            TenantA,
            NomeDoAnimal.Criar("Luna"),
            Especie.Criar("Canina"),
            raca: null,
            SexoDoAnimal.Femea,
            dataDeNascimento: null,
            corOuPelagem: null,
            observacaoCadastral: null,
            TutorResponsavel.Criar(TutorId),
            CriadoEm));

        Assert.Equal("id", exception.ParamName);
    }

    [Fact]
    public void Cadastrar_ComTenantVazio_Rejeita()
    {
        ArgumentException exception = Assert.Throws<ArgumentException>(() => Animal.Cadastrar(
            AnimalId,
            default,
            NomeDoAnimal.Criar("Luna"),
            Especie.Criar("Canina"),
            raca: null,
            SexoDoAnimal.Femea,
            dataDeNascimento: null,
            corOuPelagem: null,
            observacaoCadastral: null,
            TutorResponsavel.Criar(TutorId),
            CriadoEm));

        Assert.Equal("tenantId", exception.ParamName);
    }

    [Fact]
    public void Cadastrar_ComSexoInvalido_Rejeita()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Animal.Cadastrar(
            AnimalId,
            TenantA,
            NomeDoAnimal.Criar("Luna"),
            Especie.Criar("Canina"),
            raca: null,
            (SexoDoAnimal)99,
            dataDeNascimento: null,
            corOuPelagem: null,
            observacaoCadastral: null,
            TutorResponsavel.Criar(TutorId),
            CriadoEm));
    }

    [Fact]
    public void AlterarCadastro_ComDadosValidos_AtualizaCamposETimestamp()
    {
        Animal animal = CriarAnimal();
        DateTimeOffset atualizadoEm = CriadoEm.AddHours(2);

        animal.AlterarCadastro(
            NomeDoAnimal.Criar("Sol"),
            Especie.Criar("Felina"),
            Raca.Criar("Siamese"),
            SexoDoAnimal.Macho,
            DataDeNascimento.Criar(new DateOnly(2021, 1, 3), Hoje),
            CorOuPelagem.Criar("Cinza"),
            ObservacaoCadastral.Criar("Animal docil"),
            atualizadoEm);

        Assert.Equal("Sol", animal.Nome.Valor);
        Assert.Equal("Felina", animal.Especie.Valor);
        Assert.Equal("Siamese", animal.Raca?.Valor);
        Assert.Equal(SexoDoAnimal.Macho, animal.Sexo);
        Assert.Equal(new DateOnly(2021, 1, 3), animal.DataDeNascimento?.Valor);
        Assert.Equal("Cinza", animal.CorOuPelagem?.Valor);
        Assert.Equal("Animal docil", animal.ObservacaoCadastral?.Valor);
        Assert.Equal(TutorId, animal.TutorResponsavel.TutorId);
        Assert.Equal(atualizadoEm, animal.AtualizadoEm);
        Assert.Equal(CriadoEm, animal.CriadoEm);
        Assert.Equal(AnimalId, animal.Id);
        Assert.Equal(TenantA, animal.TenantId);
        Assert.Equal(2, animal.Versao);
    }

    [Fact]
    public void AlterarCadastro_ComCamposOpcionaisAusentes_RemoveCamposOpcionais()
    {
        Animal animal = CriarAnimal();

        animal.AlterarCadastro(
            NomeDoAnimal.Criar("Luna"),
            Especie.Criar("Canina"),
            raca: null,
            SexoDoAnimal.NaoInformado,
            dataDeNascimento: null,
            corOuPelagem: null,
            observacaoCadastral: null,
            CriadoEm.AddHours(1));

        Assert.Null(animal.Raca);
        Assert.Equal(SexoDoAnimal.NaoInformado, animal.Sexo);
        Assert.Null(animal.DataDeNascimento);
        Assert.Null(animal.CorOuPelagem);
        Assert.Null(animal.ObservacaoCadastral);
    }

    [Fact]
    public void Inativar_AnimalAtivo_MarcaInativoEAtualizaTimestamp()
    {
        Animal animal = CriarAnimal();
        DateTimeOffset inativadoEm = CriadoEm.AddDays(1);

        animal.Inativar(inativadoEm);

        Assert.False(animal.EstaAtivo);
        Assert.Equal(SituacaoDoAnimal.Inativo, animal.Situacao);
        Assert.Equal(inativadoEm, animal.InativadoEm);
        Assert.Equal(inativadoEm, animal.AtualizadoEm);
        Assert.Equal(2, animal.Versao);
    }

    [Fact]
    public void Inativar_AnimalJaInativo_RejeitaNovaInativacao()
    {
        Animal animal = CriarAnimal();
        animal.Inativar(CriadoEm.AddDays(1));

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            animal.Inativar(CriadoEm.AddDays(2)));

        Assert.Contains("ja esta inativo", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void RegistrarFalecimento_AnimalAtivo_MarcaFalecidoEAtualizaTimestamp()
    {
        Animal animal = CriarAnimal();
        DateTimeOffset registradoEm = CriadoEm.AddDays(1);
        DataDoFalecimento dataDoFalecimento = DataDoFalecimento.Criar(new DateOnly(2026, 7, 19), Hoje);

        animal.RegistrarFalecimento(dataDoFalecimento, registradoEm);

        Assert.False(animal.EstaAtivo);
        Assert.Equal(SituacaoDoAnimal.Falecido, animal.Situacao);
        Assert.Equal(new DateOnly(2026, 7, 19), animal.DataDoFalecimento?.Valor);
        Assert.Equal(registradoEm, animal.AtualizadoEm);
        Assert.Equal(2, animal.Versao);
        Assert.Null(animal.InativadoEm);
    }

    [Fact]
    public void RegistrarFalecimento_AnimalJaFalecido_RejeitaEPreservaEstado()
    {
        Animal animal = CriarAnimal();
        animal.RegistrarFalecimento(
            DataDoFalecimento.Criar(new DateOnly(2026, 7, 19), Hoje),
            CriadoEm.AddDays(1));
        int versaoAposFalecimento = animal.Versao;

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            animal.RegistrarFalecimento(
                DataDoFalecimento.Criar(new DateOnly(2026, 7, 20), Hoje),
                CriadoEm.AddDays(2)));

        Assert.Contains("ja foi registrado", exception.Message, StringComparison.Ordinal);
        Assert.Equal(new DateOnly(2026, 7, 19), animal.DataDoFalecimento?.Valor);
        Assert.Equal(versaoAposFalecimento, animal.Versao);
    }

    [Fact]
    public void AlterarCadastro_AnimalFalecido_RejeitaEPreservaEstado()
    {
        Animal animal = CriarAnimal();
        animal.RegistrarFalecimento(
            DataDoFalecimento.Criar(new DateOnly(2026, 7, 19), Hoje),
            CriadoEm.AddDays(1));
        int versaoAposFalecimento = animal.Versao;

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            animal.AlterarCadastro(
                NomeDoAnimal.Criar("Sol"),
                Especie.Criar("Felina"),
                raca: null,
                SexoDoAnimal.Macho,
                dataDeNascimento: null,
                corOuPelagem: null,
                observacaoCadastral: null,
                CriadoEm.AddDays(2)));

        Assert.Contains("falecido", exception.Message, StringComparison.Ordinal);
        Assert.Equal("Luna", animal.Nome.Valor);
        Assert.Equal(SituacaoDoAnimal.Falecido, animal.Situacao);
        Assert.Equal(versaoAposFalecimento, animal.Versao);
    }

    [Fact]
    public void Inativar_AnimalFalecido_Rejeita()
    {
        Animal animal = CriarAnimal();
        animal.RegistrarFalecimento(
            DataDoFalecimento.Criar(new DateOnly(2026, 7, 19), Hoje),
            CriadoEm.AddDays(1));

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            animal.Inativar(CriadoEm.AddDays(2)));

        Assert.Contains("falecido", exception.Message, StringComparison.Ordinal);
        Assert.Equal(SituacaoDoAnimal.Falecido, animal.Situacao);
    }

    [Fact]
    public void TransferirResponsabilidade_ComNovoTutor_AtualizaTutorTimestampEVersao()
    {
        Animal animal = CriarAnimal();
        DateTimeOffset transferidoEm = CriadoEm.AddHours(3);

        animal.TransferirResponsabilidade(TutorResponsavel.Criar(OutroTutorId), transferidoEm);

        Assert.Equal(OutroTutorId, animal.TutorResponsavel.TutorId);
        Assert.Equal(transferidoEm, animal.AtualizadoEm);
        Assert.Equal(2, animal.Versao);
    }

    [Fact]
    public void TransferirResponsabilidade_ComMesmoTutor_Rejeita()
    {
        Animal animal = CriarAnimal();

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            animal.TransferirResponsabilidade(TutorResponsavel.Criar(TutorId), CriadoEm.AddHours(3)));

        Assert.Contains("diferente", exception.Message, StringComparison.Ordinal);
        Assert.Equal(TutorId, animal.TutorResponsavel.TutorId);
        Assert.Equal(1, animal.Versao);
    }

    [Fact]
    public void TransferirResponsabilidade_ComAnimalInativo_RejeitaEPreservaEstado()
    {
        Animal animal = CriarAnimal();
        animal.Inativar(CriadoEm.AddHours(1));
        int versaoAposInativacao = animal.Versao;

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            animal.TransferirResponsabilidade(TutorResponsavel.Criar(OutroTutorId), CriadoEm.AddHours(3)));

        Assert.Contains("ativo", exception.Message, StringComparison.Ordinal);
        Assert.Equal(TutorId, animal.TutorResponsavel.TutorId);
        Assert.Equal(versaoAposInativacao, animal.Versao);
        Assert.Equal(SituacaoDoAnimal.Inativo, animal.Situacao);
    }

    [Fact]
    public void TransferirResponsabilidade_ComAnimalFalecido_RejeitaEPreservaEstado()
    {
        Animal animal = CriarAnimal();
        animal.RegistrarFalecimento(
            DataDoFalecimento.Criar(new DateOnly(2026, 7, 19), Hoje),
            CriadoEm.AddHours(1));
        int versaoAposFalecimento = animal.Versao;

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            animal.TransferirResponsabilidade(TutorResponsavel.Criar(OutroTutorId), CriadoEm.AddHours(3)));

        Assert.Contains("ativo", exception.Message, StringComparison.Ordinal);
        Assert.Equal(TutorId, animal.TutorResponsavel.TutorId);
        Assert.Equal(versaoAposFalecimento, animal.Versao);
        Assert.Equal(SituacaoDoAnimal.Falecido, animal.Situacao);
    }

    [Fact]
    public void Equals_ComMesmoIdETenant_ConsideraMesmoAnimal()
    {
        Animal animal = CriarAnimal();
        Animal mesmoAnimal = CriarAnimal();

        Assert.Equal(animal, mesmoAnimal);
        Assert.Equal(animal.GetHashCode(), mesmoAnimal.GetHashCode());
    }

    [Fact]
    public void Equals_ComMesmoIdEmTenantDiferente_NaoConsideraMesmoAnimal()
    {
        Animal animalTenantA = CriarAnimal();
        Animal animalTenantB = Animal.Cadastrar(
            AnimalId,
            TenantB,
            NomeDoAnimal.Criar("Luna"),
            Especie.Criar("Canina"),
            raca: null,
            SexoDoAnimal.Femea,
            dataDeNascimento: null,
            corOuPelagem: null,
            observacaoCadastral: null,
            TutorResponsavel.Criar(OutroTutorId),
            CriadoEm);

        Assert.NotEqual(animalTenantA, animalTenantB);
        Assert.Equal(TenantA, animalTenantA.TenantId);
        Assert.Equal(TenantB, animalTenantB.TenantId);
    }

    private static Animal CriarAnimal() =>
        Animal.Cadastrar(
            AnimalId,
            TenantA,
            NomeDoAnimal.Criar("Luna"),
            Especie.Criar("Canina"),
            Raca.Criar("SRD"),
            SexoDoAnimal.Femea,
            DataDeNascimento.Criar(new DateOnly(2020, 5, 2), Hoje),
            CorOuPelagem.Criar("Caramelo"),
            ObservacaoCadastral.Criar("Resgata com medo de fogos"),
            TutorResponsavel.Criar(TutorId),
            CriadoEm);
}
