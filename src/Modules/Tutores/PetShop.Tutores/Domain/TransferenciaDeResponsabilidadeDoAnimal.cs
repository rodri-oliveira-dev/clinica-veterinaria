namespace PetShop.Tutores.Domain;

internal sealed class TransferenciaDeResponsabilidadeDoAnimal
{
    private TransferenciaDeResponsabilidadeDoAnimal(
        Guid id,
        TenantId tenantId,
        AnimalId animalId,
        TutorId tutorAnteriorId,
        TutorId tutorNovoId,
        DateTimeOffset realizadaEm,
        string subject,
        string? motivo)
    {
        Id = id;
        TenantId = tenantId;
        AnimalId = animalId;
        TutorAnteriorId = tutorAnteriorId;
        TutorNovoId = tutorNovoId;
        RealizadaEm = realizadaEm;
        Subject = subject;
        Motivo = motivo;
    }

    public Guid Id { get; }

    public TenantId TenantId { get; }

    public AnimalId AnimalId { get; }

    public TutorId TutorAnteriorId { get; }

    public TutorId TutorNovoId { get; }

    public DateTimeOffset RealizadaEm { get; }

    public string Subject { get; }

    public string? Motivo { get; }

    public static TransferenciaDeResponsabilidadeDoAnimal Registrar(
        Guid id,
        TenantId tenantId,
        AnimalId animalId,
        TutorId tutorAnteriorId,
        TutorId tutorNovoId,
        DateTimeOffset realizadaEm,
        string subject,
        string? motivo)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("O identificador da transferencia deve ser informado.", nameof(id));
        }

        if (tenantId.EstaVazio)
        {
            throw new ArgumentException("O tenant da transferencia deve ser informado.", nameof(tenantId));
        }

        if (animalId.EstaVazio)
        {
            throw new ArgumentException("O animal da transferencia deve ser informado.", nameof(animalId));
        }

        if (tutorAnteriorId.EstaVazio)
        {
            throw new ArgumentException("O tutor anterior da transferencia deve ser informado.", nameof(tutorAnteriorId));
        }

        if (tutorNovoId.EstaVazio)
        {
            throw new ArgumentException("O tutor novo da transferencia deve ser informado.", nameof(tutorNovoId));
        }

        if (tutorAnteriorId == tutorNovoId)
        {
            throw new ArgumentException("O tutor novo deve ser diferente do tutor anterior.", nameof(tutorNovoId));
        }

        if (string.IsNullOrWhiteSpace(subject))
        {
            throw new ArgumentException("O subject autenticado deve ser informado.", nameof(subject));
        }

        string? motivoNormalizado = string.IsNullOrWhiteSpace(motivo) ? null : motivo.Trim();

        return new TransferenciaDeResponsabilidadeDoAnimal(
            id,
            tenantId,
            animalId,
            tutorAnteriorId,
            tutorNovoId,
            realizadaEm,
            subject.Trim(),
            motivoNormalizado);
    }
}
