using Common.Enums.PDF;

namespace PdfHandler.Signing
{
    public interface ISigningTypeHandler
    {
        ISigning ExecuteCreation(SignatureFieldType signatureType);
    }
}
