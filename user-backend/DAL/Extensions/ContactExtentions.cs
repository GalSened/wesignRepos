namespace DAL.Extensions
{
    using Common.Enums.Documents;
    using Common.Extensions;
    using Common.Models;
    using DAL.DAOs.Contacts;
    using System.Collections.Generic;
    using System.Linq;

    public static class ContactExtentions
    {
        public static Contact ToContact(this ContactDAO contactDAO)
        {
            return contactDAO == null ? null : new Contact()
            {
                Id = contactDAO.Id,
                UserId = contactDAO.UserId,
                GroupId = contactDAO.GroupId,
                Name = contactDAO.Name,
                Email = contactDAO.Email,
                Phone = contactDAO.Phone,
                PhoneExtension = contactDAO.PhoneExtension,
                DefaultSendingMethod = contactDAO.DefaultSendingMethod != 0 ? contactDAO.DefaultSendingMethod :
                                       contactDAO.DefaultSendingMethod == 0 && ContactsExtenstions.IsValidPhone(contactDAO.Phone) && ContactsExtenstions.IsValidPhoneExtension(contactDAO.PhoneExtension) ?
                                                SendingMethod.SMS : SendingMethod.Email,

                Status = contactDAO.Status,
                LastUsedTime = contactDAO.LastUsedTime,

                Seals = ToSeals(contactDAO.Seals),
                CreationSource = contactDAO.CreationSource,
                SearchTag = contactDAO.SearchTag

            };
        }

        private static IEnumerable<Seal> ToSeals(ICollection<ContactSealsDAO> seals)
        {
            var result = new List<Seal>();
            if (seals != null)
            {
                foreach (var sealDAO in seals)
                {
                    result.Add(sealDAO.ToSeal());
                }
            }
            return result;
        }
    }
}
