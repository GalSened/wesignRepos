namespace DAL.DAOs.Templates
{
    using Common.Enums.PDF;
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("TemplateTextFields")]
    public class TemplateTextFieldDAO
    {
        public Guid Id{ get; set; }
        public Guid TemplateId { get; set; }
        public string Name { get; set; }
        public TextFieldType TextFieldType { get; set; }
        public string Regex { get; set; }
        public virtual TemplateDAO Template { get; set; }

    }
}
