using Common.Enums.Users;

namespace WeSignSigner.Models.Responses
{
    public class SingleLinkDataResponseDTO
    {
        public bool IsSmsProviderSupportGloballySend { get; set; }
        public Language Language { get; set; }
    }
}
