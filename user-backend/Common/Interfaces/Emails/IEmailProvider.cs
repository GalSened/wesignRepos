namespace Common.Interfaces.Emails
{
    using Common.Models.Configurations;
    using Common.Models.Emails;
    using System.Threading.Tasks;

    public interface IEmailProvider
    {
        Task Send(Email email, SmtpConfiguration configuration);
    }
}
