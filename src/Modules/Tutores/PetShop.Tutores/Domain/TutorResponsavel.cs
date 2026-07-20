namespace PetShop.Tutores.Domain;

internal readonly record struct TutorResponsavel
{
    private TutorResponsavel(Guid tutorId)
    {
        TutorId = tutorId;
    }

    public Guid TutorId { get; }

    public bool EstaVazio => TutorId == Guid.Empty;

    public static TutorResponsavel Criar(Guid tutorId)
    {
        if (tutorId == Guid.Empty)
        {
            throw new ArgumentException("O tutor responsavel pelo animal deve ser informado.", nameof(tutorId));
        }

        return new TutorResponsavel(tutorId);
    }

    public override string ToString() => TutorId.ToString();
}
