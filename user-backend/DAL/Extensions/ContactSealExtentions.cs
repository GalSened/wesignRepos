using DAL.DAOs.Contacts;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Extensions
{
    public static class ContactSealExtentions
    {
        public static Seal ToSeal(this ContactSealsDAO contactSealsDAO)
        {
            return contactSealsDAO == null ? null : new Seal()
            {
                Id = contactSealsDAO.Id,
                Name = contactSealsDAO.Name,
                Base64Image = $"data:image/{contactSealsDAO.Type.ToString().ToLower()};base64,"
            };
        }
    }
}
