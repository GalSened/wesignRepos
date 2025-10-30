using System;
using System.ServiceModel;
using System.Threading.Tasks;
using UserSoapService.HttpClientLogic;
using UserSoapService.Responses;

namespace UserSoapService
{
    [ServiceContract]
    public interface IUserSoapService
    {
        #region Users

        [OperationContract]
        Task<SignUpResponse> SignUpAsync(CreateUserDTO body);

        [OperationContract]
        Task<LoginResponse> LoginAsync(LoginRequestDTO body);

        [OperationContract]
        Task<GetCurrentUserDetailsResponse> GetCurrentUserDetailsAsync(string jwt);

        [OperationContract]
        Task<BaseResult> UpdateUserAsync(string jwt, UpdateUserDTO body);

        #endregion

        #region Contacts

        [OperationContract]
        Task<GetContactAsyncResponse> GetContactAsync(string jwt, Guid id);

        [OperationContract]
        Task<GetContactsAsyncResponse> GetContactsAsync(string jwt, string key, int? offset, int? limit, bool? popular, bool? recent, bool? includeTabletMode);

        [OperationContract]
        Task<CreateContactResponse> CreateContactAsync(string jwt, ContactDTO body);

        [OperationContract]
        Task<BaseResult> UpdateContactAsync(string jwt, Guid id, ContactDTO body);

        #endregion

        #region Templates

        [OperationContract]
        Task<GetTemplatesResponse> GetTemplatesAsync(string jwt, string key, string from, string to, int? offset, int? limit, bool? popular, bool? recent);

        [OperationContract]
        Task<DownloadTemplateResponse> DownloadTemplateAsync(string jwt, Guid id);

        [OperationContract]
        Task<GetTemplatePagesCountResponse> GetTemplatePagesCountAsync(string jwt, Guid id);

        [OperationContract]
        Task<GetTemplatesPagesInfoResponse> GetTemplatesPagesInfoAsync(string jwt, Guid id, int? offset, int? limit);

        [OperationContract]
        Task<CreateTemplateResponse> CreateTemplateAsync(string jwt, CreateTemplateDTO body);

        [OperationContract]
        Task<DuplicateTemplateResponse> DuplicateTemplateAsync(string jwt, Guid id, DuplicateTemplateDTO body);

        [OperationContract]
        Task<BaseResult> UpdateTemplateAsync(string jwt, Guid id, UpdateTemplateDTO body);

        [OperationContract]
        Task<BaseResult> DeleteTemplateAsync(string jwt, Guid id);

        #endregion

        #region DocumentCollections

        [OperationContract]
        Task<GetDocumentCollectionsResponse> GetDocumentCollectionsAsync(string jwt, string key, bool? sent, bool? viewed, bool? signed, bool? declined, bool? sendingFailed, bool? canceled, string userId, string from, string to, int? offset, int? limit);
        
        [OperationContract]
        Task<GetDocumentCollectionDataResponse> GetDocumentCollectionInfoAsync(string jwt, Guid id);

        [OperationContract]
        Task<DownloadDocumentCollectionResponse> DownloadDocumentCollectionAsync(string jwt, Guid id);

        [OperationContract]
        Task<DownloadDocumentCollectionAttchmentResponse> DownloadDocumentCollectionAttchmentAsync(string jwt, Guid id, Guid signerId);

        [OperationContract]
        Task<DownloadDocumentCollectionTraceResponse> DownloadDocumentCollectionTraceAsync(string jwt, Guid id, int offset);

        [OperationContract]
        Task<GetDocumentCollectionPagesCountResponse> GetDocumentCollectionPagesCountAsync(string jwt, Guid id, Guid documentId);

        [OperationContract]
        Task<GetDocumentCollectionPagesInfoResponse> GetDocumentCollectionsPagesInfoAsync(string jwt, Guid id, Guid documentId, int? offset, int? limit);

        [OperationContract]
        Task<CreateDocumentCollectionResponse> CreateDocumentCollectionAsync(string jwt, CreateDocumentCollectionDTO body);

        [OperationContract]
        Task<BaseResult> DeleteDocumentCollectionAsync(string jwt, Guid id);

        [OperationContract]
        Task<ResendDocumentCollectionResponse> ResendDocumentCollectionAsync(string jwt, Guid id, Guid signerId, SendingMethod sendingMethod, bool? shouldSend);

        [OperationContract]
        Task<BaseResult> ShareDocumentCollectionAsync(string jwt, ShareDTO body);

        [OperationContract]
        Task<ExportDocumentCollectionResponse> ExportDocumentCollectionAsync(string jwt, bool? sent, bool? viewed, bool? signed, bool? declined, bool? sendingFailed, bool? canceled);

        [OperationContract]
        Task<ExportDocumentCollectionPdfFieldsResponse> ExportDocumentCollectionPdfFieldsAsync(string jwt, Guid id);

        [OperationContract]
        Task<BaseResult> CancelDocumentCollectionAsync(string jwt, Guid id);
        
        [OperationContract]
        Task<BaseResult> ReplaceSignerAsync(string jwt, Guid id, Guid signerId, ReplaceSignerDTO body);
        #endregion
    }
}