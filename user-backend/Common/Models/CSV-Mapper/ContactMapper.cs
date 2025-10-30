using Common.Enums.Documents;

namespace Common.Models.CSV_Mapper
{
    public class ContactMapper
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public SendingMethod SendingMethod { get; set; }
    }
}
