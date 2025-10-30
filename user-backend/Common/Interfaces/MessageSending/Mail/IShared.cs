using Common.Enums;
using Common.Models;
using Common.Models.Emails;
using System.Threading.Tasks;

namespace Common.Interfaces.MessageSending.Mail
{
    public interface IShared
    {
        string GetContactNameFormat(Contact contact);
        void RemoveButton(Email email);
        Task<Resource> InitEmail(Email email, User user, MessageType messageType);
        void LoadEmailAttachments(DocumentCollection documentCollection, Email email, bool shouldSendSignedDocument);
    }
}
