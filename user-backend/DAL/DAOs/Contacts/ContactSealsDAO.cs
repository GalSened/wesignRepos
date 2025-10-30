namespace DAL.DAOs.Contacts
{
    using Common.Enums;
    using System;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    [Table("ContactSeals")]
    public class ContactSealsDAO
    {
        public Guid Id { get; set; }
        public Guid ContactId { get; set; }
        public string Name { get; set; }
        public ImageType Type { get; set; }
        public virtual ContactDAO Contact { get; set; }

        public ContactSealsDAO() { }

        public ContactSealsDAO(Seal seal)
        {
            Id = seal.Id == Guid.Empty ? default : seal.Id;
            Name = seal.Name;
            Type = GetImageType(seal.Base64Image);
        }

        private ImageType GetImageType(string base64Image)
        {
            var dataType = base64Image.Split(new char[] { ',' }).FirstOrDefault();
            int length = dataType.IndexOf(';') - dataType.IndexOf('/') - 1;
            string imageType = dataType?.Substring(dataType.IndexOf('/') + 1, length);
            Enum.TryParse(imageType.ToUpper(), out ImageType type);
            return type;
        }
    }
}
