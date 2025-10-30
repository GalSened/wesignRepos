namespace Common.Models.Files.PDF
{
    using Common.Enums.PDF;

    public class SignatureField : BaseField
    {
        public string Image { get; set; }
        public SignatureFieldType SigningType { get; set; }
        public SignatureFieldKind SignatureKind { get; set; }
        public SignatureField()
        {
            SigningType = SignatureFieldType.Graphic;
            SignatureKind = SignatureFieldKind.Simple;
        }

        public SignatureField(BaseField baseField)
        {
            foreach (var prop in baseField.GetType().GetProperties())
            {
                var derivedClassProp = GetType().GetProperty(prop.Name);
                derivedClassProp.SetValue(this, prop.GetValue(baseField));
            }

            SigningType = SignatureFieldType.Graphic;
            SignatureKind = SignatureFieldKind.Simple;
        }

    }
}
