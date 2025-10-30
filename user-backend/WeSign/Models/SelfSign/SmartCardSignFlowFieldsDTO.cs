using System;

namespace WeSign.Models.SelfSign
{
    public class SmartCardSignFlowFieldsDTO
    {
        public Guid DocumentId { get; set; }
        public string FieldName { get; set; }
        public string Image { get; set; }
    }
}
