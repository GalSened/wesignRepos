using Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interfaces
{
    public interface IContactSignatures
    {
        Task UpdateSignaturesImages(Contact contact, List<string> signaturesImages);
        List<string> GetContactSavedSignatures(Contact contact);
    }
}
