using System.Net.Mail;

namespace PetShop.Tutores.Domain;

internal readonly record struct Email
{
    private Email(string valor)
    {
        Valor = valor;
    }

    public string Valor { get; }

    public static Email Criar(string? valor)
    {
        string email = valor?.Trim() ?? string.Empty;

        if (email.Length == 0)
        {
            throw new ArgumentException("O e-mail deve ser informado quando usado.", nameof(valor));
        }

        try
        {
            var endereco = new MailAddress(email);

            if (!string.Equals(endereco.Address, email, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("O e-mail informado deve ser valido.", nameof(valor));
            }
        }
        catch (FormatException ex)
        {
            throw new ArgumentException("O e-mail informado deve ser valido.", nameof(valor), ex);
        }

        return new Email(email.ToLowerInvariant());
    }

    public override string ToString() => Valor;
}
