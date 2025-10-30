namespace Common.Interfaces.Emails
{
    using Common.Models;
    using Common.Models.Documents.Signers;
    using System.Threading.Tasks;

    public interface IEmail
    {
        Task<string> Activation(User user, bool sendEmail = true);
        Task<string> ResetPassword(User user, string token);
    }
}
