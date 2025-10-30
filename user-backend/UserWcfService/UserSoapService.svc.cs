using System;
using System.Configuration;
using System.Net;
using System.Threading.Tasks;
using UserSoapService.HttpClientLogic;
using UserSoapService.Responses;

namespace UserSoapService
{
    public class UserSoapService : IUserSoapService
    {
        private readonly UserApiClient _userApiClient;        

        public UserSoapService()
        {
            _userApiClient = new UserApiClient(new System.Net.Http.HttpClient())
            {
                BaseUrl = ConfigurationManager.AppSettings["BaseUrl"]
            };
        }
        
        #region Users
   
        public async Task<SignUpResponse> SignUpAsync(CreateUserDTO body)
        {
            try
            {
                return new SignUpResponse
                {
                    StatusCode = HttpStatusCode.OK, 
                    Link = await _userApiClient.UsersPOST2Async(body)
                };
            }
            catch (ApiException<GeneralError> ex)
            {
                return new SignUpResponse
                {
                    StatusCode = (HttpStatusCode)ex.StatusCode,
                    GeneralError = ex.Result
                };
            }
            catch (Exception ex)
            {
                return new SignUpResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    GeneralError = new GeneralError
                    {
                        Title = ex.Message,
                        TraceId = ex.HResult.ToString()
                    }
                };
            }
        }
        
        public async Task<LoginResponse> LoginAsync(LoginRequestDTO body)
        {
            try
            {
                return new LoginResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    UserTokens = await _userApiClient.LoginAsync(body)
                };
            }
            catch (ApiException<GeneralError> ex)
            {
                return new LoginResponse
                {
                    StatusCode = (HttpStatusCode)ex.StatusCode,
                    GeneralError = ex.Result
                };
            }
            catch (Exception ex)
            {
                return new LoginResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    GeneralError = new GeneralError
                    {
                        Title = ex.Message,
                        TraceId = ex.HResult.ToString()
                    }
                };
            }
        }
        
        public async Task<GetCurrentUserDetailsResponse> GetCurrentUserDetailsAsync(string jwt)
        {
            try
            {
                UserApiClientHelper.AddAuthentication(jwt, _userApiClient.HttpClient);
                return new GetCurrentUserDetailsResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    UserDetails = await _userApiClient.UsersGET2Async()
                };
            }
            catch (ApiException<GeneralError> ex)
            {
                return new GetCurrentUserDetailsResponse
                {
                    StatusCode = (HttpStatusCode)ex.StatusCode,
                    GeneralError = ex.Result
                };
            }
            catch (Exception ex)
            {
                return new GetCurrentUserDetailsResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    GeneralError = new GeneralError
                    {
                        Title = ex.Message,
                        TraceId = ex.HResult.ToString()
                    }
                };
            }
        }

        public async Task<BaseResult> UpdateUserAsync(string jwt, UpdateUserDTO body)
        {
            try
            {
                UserApiClientHelper.AddAuthentication(jwt, _userApiClient.HttpClient);
                await _userApiClient.UsersPUT2Async(body);
                return new BaseResult
                {
                    StatusCode = HttpStatusCode.OK
                };
            }
            catch (ApiException<GeneralError> ex)
            {
                return new BaseResult
                {
                    StatusCode = (HttpStatusCode)ex.StatusCode,
                    GeneralError = ex.Result
                };
            }
            catch (Exception ex)
            {
                return new BaseResult
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    GeneralError = new GeneralError
                    {
                        Title = ex.Message,
                        TraceId = ex.HResult.ToString()
                    }
                };
            }
        }
   
        #endregion

        #region Contacts

        public async Task<GetContactAsyncResponse> GetContactAsync(string jwt, Guid id)
        {
            try
            {
                UserApiClientHelper.AddAuthentication(jwt, _userApiClient.HttpClient);
                return new GetContactAsyncResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    Contact = await _userApiClient.ContactsGET2Async(id)
                };
            }
            catch (ApiException<GeneralError> ex)
            {
                return new GetContactAsyncResponse
                {
                    StatusCode = (HttpStatusCode)ex.StatusCode,
                    GeneralError = ex.Result
                };
            }
            catch (Exception ex)
            {
                return new GetContactAsyncResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    GeneralError = new GeneralError
                    {
                        Title = ex.Message,
                        TraceId = ex.HResult.ToString()
                    }
                };
            }
        }

        public async Task<GetContactsAsyncResponse> GetContactsAsync(string jwt, string key, int? offset, int? limit, bool? popular, bool? recent, bool? includeTabletMode)
        {
            try
            {
                UserApiClientHelper.AddAuthentication(jwt, _userApiClient.HttpClient);
                return new GetContactsAsyncResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    Contacts = await _userApiClient.ContactsGETAsync(key, offset, limit, popular, recent, includeTabletMode)
                };
            }
            catch (ApiException<GeneralError> ex)
            {
                return new GetContactsAsyncResponse
                {
                    StatusCode = (HttpStatusCode)ex.StatusCode,
                    GeneralError = ex.Result
                };
            }
            catch (Exception ex)
            {
                return new GetContactsAsyncResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    GeneralError = new GeneralError
                    {
                        Title = ex.Message,
                        TraceId = ex.HResult.ToString()
                    }
                };
            }
        }

        public async Task<CreateContactResponse> CreateContactAsync(string jwt, ContactDTO body)
        {
            try
            {
                UserApiClientHelper.AddAuthentication(jwt, _userApiClient.HttpClient);
                return new CreateContactResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    ContactData = await _userApiClient.ContactsPOSTAsync(body)
                };
            }
            catch (ApiException<GeneralError> ex)
            {
                return new CreateContactResponse
                {
                    StatusCode = (HttpStatusCode)ex.StatusCode,
                    GeneralError = ex.Result
                };
            }
            catch (Exception ex)
            {
                return new CreateContactResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    GeneralError = new GeneralError
                    {
                        Title = ex.Message,
                        TraceId = ex.HResult.ToString()
                    }
                };
            }
        }

        public async Task<BaseResult> UpdateContactAsync(string jwt, Guid id, ContactDTO body)
        {
            try
            {
                UserApiClientHelper.AddAuthentication(jwt, _userApiClient.HttpClient);
                await _userApiClient.ContactsPUTAsync(id, body);
                return new BaseResult { StatusCode = HttpStatusCode.OK };
            }
            catch (ApiException<GeneralError> ex)
            {
                return new BaseResult
                {
                    StatusCode = (HttpStatusCode)ex.StatusCode,
                    GeneralError = ex.Result
                };
            }
            catch (Exception ex)
            {
                return new BaseResult
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    GeneralError = new GeneralError
                    {
                        Title = ex.Message,
                        TraceId = ex.HResult.ToString()
                    }
                };
            }
        }

        #endregion

        #region Templates

        public async Task<GetTemplatesResponse> GetTemplatesAsync(string jwt, string key, string from, string to, int? offset, int? limit, bool? popular, bool? recent)
        {
            try
            {
                UserApiClientHelper.AddAuthentication(jwt, _userApiClient.HttpClient);
                return new GetTemplatesResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    Templates = await _userApiClient.TemplatesGETAsync(key, from, to, offset, limit, popular, recent)
                };
            }
            catch (ApiException<GeneralError> ex)
            {
                return new GetTemplatesResponse
                {
                    StatusCode = (HttpStatusCode)ex.StatusCode,
                    GeneralError = ex.Result
                };
            }
            catch (Exception ex)
            {
                return new GetTemplatesResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    GeneralError = new GeneralError
                    {
                        Title = ex.Message,
                        TraceId = ex.HResult.ToString()
                    }
                };
            }
        }
        
        public async Task<DownloadTemplateResponse> DownloadTemplateAsync(string jwt, Guid id)
        {
            try
            {
                UserApiClientHelper.AddAuthentication(jwt, _userApiClient.HttpClient);                
                return new DownloadTemplateResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    DownloadResponse = await _userApiClient.DownloadAsync(id)
                };
            }
            catch (ApiException<GeneralError> ex)
            {
                return new DownloadTemplateResponse
                {
                    StatusCode = (HttpStatusCode)ex.StatusCode,
                    GeneralError = ex.Result
                };
            }
            catch (Exception ex)
            {
                return new DownloadTemplateResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    GeneralError = new GeneralError
                    {
                        Title = ex.Message,
                        TraceId = ex.HResult.ToString()
                    }
                };
            }
        }

        public async Task<GetTemplatePagesCountResponse> GetTemplatePagesCountAsync(string jwt, Guid id)
        {
            try
            {
                UserApiClientHelper.AddAuthentication(jwt, _userApiClient.HttpClient);
                return new GetTemplatePagesCountResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    TemplateCount = await _userApiClient.Pages3Async(id)
                };
            }
            catch (ApiException<GeneralError> ex)
            {
                return new GetTemplatePagesCountResponse
                {
                    StatusCode = (HttpStatusCode)ex.StatusCode,
                    GeneralError = ex.Result
                };
            }
            catch (Exception ex)
            {
                return new GetTemplatePagesCountResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    GeneralError = new GeneralError
                    {
                        Title = ex.Message,
                        TraceId = ex.HResult.ToString()
                    }
                };
            }
        }

        public async Task<GetTemplatesPagesInfoResponse> GetTemplatesPagesInfoAsync(string jwt, Guid id, int? offset, int? limit)
        {
            try
            {
                UserApiClientHelper.AddAuthentication(jwt, _userApiClient.HttpClient);
                return new GetTemplatesPagesInfoResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    TemplatePagesRange = await _userApiClient.RangeAsync(id, offset, limit)
                };
            }
            catch (ApiException<GeneralError> ex)
            {
                return new GetTemplatesPagesInfoResponse
                {
                    StatusCode = (HttpStatusCode)ex.StatusCode,
                    GeneralError = ex.Result
                };
            }
            catch (Exception ex)
            {
                return new GetTemplatesPagesInfoResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    GeneralError = new GeneralError
                    {
                        Title = ex.Message,
                        TraceId = ex.HResult.ToString()
                    }
                };
            }
        }

        public async Task<CreateTemplateResponse> CreateTemplateAsync(string jwt, CreateTemplateDTO body)
        {
            try
            {
                UserApiClientHelper.AddAuthentication(jwt, _userApiClient.HttpClient);
                return new CreateTemplateResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    TemplateCount = await _userApiClient.TemplatesPOSTAsync(body)
                };
            }
            catch (ApiException<GeneralError> ex)
            {
                return new CreateTemplateResponse
                {
                    StatusCode = (HttpStatusCode)ex.StatusCode,
                    GeneralError = ex.Result
                };
            }
            catch (Exception ex)
            {
                return new CreateTemplateResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    GeneralError = new GeneralError
                    {
                        Title = ex.Message,
                        TraceId = ex.HResult.ToString()
                    }
                };
            }
        }

        public async Task<DuplicateTemplateResponse> DuplicateTemplateAsync(string jwt, Guid id, DuplicateTemplateDTO body)
        {
            try
            {
                UserApiClientHelper.AddAuthentication(jwt, _userApiClient.HttpClient);
                return new DuplicateTemplateResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    DuplicateTemplate = await _userApiClient.TemplatesPOST2Async(id, body)
                };
            }
            catch (ApiException<GeneralError> ex)
            {
                return new DuplicateTemplateResponse
                {
                    StatusCode = (HttpStatusCode)ex.StatusCode,
                    GeneralError = ex.Result
                };
            }
            catch (Exception ex)
            {
                return new DuplicateTemplateResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    GeneralError = new GeneralError
                    {
                        Title = ex.Message,
                        TraceId = ex.HResult.ToString()
                    }
                };
            }
        }

        public async Task<BaseResult> UpdateTemplateAsync(string jwt, Guid id, UpdateTemplateDTO body)
        {
            try
            {
                UserApiClientHelper.AddAuthentication(jwt, _userApiClient.HttpClient);
                await _userApiClient.TemplatesPUTAsync(id, body);
                return new BaseResult { StatusCode = HttpStatusCode.OK };
            }
            catch (ApiException<GeneralError> ex)
            {
                return new BaseResult
                {
                    StatusCode = (HttpStatusCode)ex.StatusCode,
                    GeneralError = ex.Result
                };
            }
            catch (Exception ex)
            {
                return new BaseResult
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    GeneralError = new GeneralError
                    {
                        Title = ex.Message,
                        TraceId = ex.HResult.ToString()
                    }
                };
            }
        }
        
        public async Task<BaseResult> DeleteTemplateAsync(string jwt, Guid id)
        {
            try
            {
                UserApiClientHelper.AddAuthentication(jwt, _userApiClient.HttpClient);
                await _userApiClient.TemplatesDELETEAsync(id);
                return new BaseResult
                {
                    StatusCode = HttpStatusCode.OK,
                };
            }
            catch (ApiException<GeneralError> ex)
            {
                return new BaseResult
                {
                    StatusCode = (HttpStatusCode)ex.StatusCode,
                    GeneralError = ex.Result
                };
            }
            catch (Exception ex)
            {
                return new BaseResult
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    GeneralError = new GeneralError
                    {
                        Title = ex.Message,
                        TraceId = ex.HResult.ToString()
                    }
                };
            }
        }

        #endregion

        #region DocumentCollections

        public async Task<GetDocumentCollectionsResponse> GetDocumentCollectionsAsync(string jwt, string key, bool? sent, bool? viewed, bool? signed, bool? declined, bool? sendingFailed, bool? canceled, string userId, string from, string to, int? offset, int? limit)
        {
            try
            {
                UserApiClientHelper.AddAuthentication(jwt, _userApiClient.HttpClient);
                return new GetDocumentCollectionsResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    DocumentCollections = await _userApiClient.DocumentcollectionsGETAsync(key, sent, viewed, signed, declined, sendingFailed, canceled, userId, from, to, offset, limit)
                };
            }
            catch (ApiException<GeneralError> ex)
            {
                return new GetDocumentCollectionsResponse
                {
                    StatusCode = (HttpStatusCode)ex.StatusCode,
                    GeneralError = ex.Result
                };
            }
            catch (Exception ex)
            {
                return new GetDocumentCollectionsResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    GeneralError = new GeneralError
                    {
                        Title = ex.Message,
                        TraceId = ex.HResult.ToString()
                    }
                };
            }
        }

        public async Task<GetDocumentCollectionDataResponse> GetDocumentCollectionInfoAsync(string jwt, Guid id)
        {
            try
            {
                UserApiClientHelper.AddAuthentication(jwt, _userApiClient.HttpClient);
                return new GetDocumentCollectionDataResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    DocumentCollectionInfo = await _userApiClient.InfoAsync(id)
                };
            }
            catch (ApiException<GeneralError> ex)
            {
                return new GetDocumentCollectionDataResponse
                {
                    StatusCode = (HttpStatusCode)ex.StatusCode,
                    GeneralError = ex.Result
                };
            }
            catch (Exception ex)
            {
                return new GetDocumentCollectionDataResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    GeneralError = new GeneralError
                    {
                        Title = ex.Message,
                        TraceId = ex.HResult.ToString()
                    }
                };
            }
        }
        
        public async Task<DownloadDocumentCollectionResponse> DownloadDocumentCollectionAsync(string jwt, Guid id)
        {
            try
            {
                UserApiClientHelper.AddAuthentication(jwt, _userApiClient.HttpClient);
                return new DownloadDocumentCollectionResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    DownloadResponse = await _userApiClient.DocumentcollectionsGET2Async(id)
                };
            }
            catch (ApiException<GeneralError> ex)
            {
                return new DownloadDocumentCollectionResponse
                {
                    StatusCode = (HttpStatusCode)ex.StatusCode,
                    GeneralError = ex.Result
                };
            }
            catch (Exception ex)
            {
                return new DownloadDocumentCollectionResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    GeneralError = new GeneralError
                    {
                        Title = ex.Message,
                        TraceId = ex.HResult.ToString()
                    }
                };
            }
        }

        public async Task<DownloadDocumentCollectionAttchmentResponse> DownloadDocumentCollectionAttchmentAsync(string jwt, Guid id, Guid signerId)
        {
            try
            {
                UserApiClientHelper.AddAuthentication(jwt, _userApiClient.HttpClient);
                return new DownloadDocumentCollectionAttchmentResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    DownloadAttachmentResponse = await _userApiClient.SignerAsync(id, signerId)
                };
            }
            catch (ApiException<GeneralError> ex)
            {
                return new DownloadDocumentCollectionAttchmentResponse
                {
                    StatusCode = (HttpStatusCode)ex.StatusCode,
                    GeneralError = ex.Result
                };
            }
            catch (Exception ex)
            {
                return new DownloadDocumentCollectionAttchmentResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    GeneralError = new GeneralError
                    {
                        Title = ex.Message,
                        TraceId = ex.HResult.ToString()
                    }
                };
            }
        }

        public async Task<DownloadDocumentCollectionTraceResponse> DownloadDocumentCollectionTraceAsync(string jwt, Guid id, int offset)
        {
            try
            {
                UserApiClientHelper.AddAuthentication(jwt, _userApiClient.HttpClient);
                return new DownloadDocumentCollectionTraceResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    DownloadTraceResponse = await _userApiClient.AuditAsync(id, offset)
                };
            }
            catch (ApiException<GeneralError> ex)
            {
                return new DownloadDocumentCollectionTraceResponse
                {
                    StatusCode = (HttpStatusCode)ex.StatusCode,
                    GeneralError = ex.Result
                };
            }
            catch (Exception ex)
            {
                return new DownloadDocumentCollectionTraceResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    GeneralError = new GeneralError
                    {
                        Title = ex.Message,
                        TraceId = ex.HResult.ToString()
                    }
                };
            }
        }
        
        public async Task<GetDocumentCollectionPagesCountResponse> GetDocumentCollectionPagesCountAsync(string jwt, Guid id, Guid documentId)
        {
            try
            {
                UserApiClientHelper.AddAuthentication(jwt, _userApiClient.HttpClient);
                return new GetDocumentCollectionPagesCountResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    DocumentCollectionCount = await _userApiClient.PagesAsync(id, documentId)
                };
            }
            catch (ApiException<GeneralError> ex)
            {
                return new GetDocumentCollectionPagesCountResponse
                {
                    StatusCode = (HttpStatusCode)ex.StatusCode,
                    GeneralError = ex.Result
                };
            }
            catch (Exception ex)
            {
                return new GetDocumentCollectionPagesCountResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    GeneralError = new GeneralError
                    {
                        Title = ex.Message,
                        TraceId = ex.HResult.ToString()
                    }
                };
            }
        }

        public async Task<GetDocumentCollectionPagesInfoResponse> GetDocumentCollectionsPagesInfoAsync(string jwt, Guid id, Guid documentId, int? offset, int? limit)
        {
            try
            {
                UserApiClientHelper.AddAuthentication(jwt, _userApiClient.HttpClient);
                return new GetDocumentCollectionPagesInfoResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    DocumentCollectionPagesRange = await _userApiClient.DocumentsAsync(id, documentId, offset, limit)
                };
            }
            catch (ApiException<GeneralError> ex)
            {
                return new GetDocumentCollectionPagesInfoResponse
                {
                    StatusCode = (HttpStatusCode)ex.StatusCode,
                    GeneralError = ex.Result
                };
            }
            catch (Exception ex)
            {
                return new GetDocumentCollectionPagesInfoResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    GeneralError = new GeneralError
                    {
                        Title = ex.Message,
                        TraceId = ex.HResult.ToString()
                    }
                };
            }
        }

        public async Task<CreateDocumentCollectionResponse> CreateDocumentCollectionAsync(string jwt, CreateDocumentCollectionDTO body)
        {
            try
            {
                UserApiClientHelper.AddAuthentication(jwt, _userApiClient.HttpClient);
                return new CreateDocumentCollectionResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    CreateDocumentCollection = await _userApiClient.DocumentcollectionsPOSTAsync(body)
                };
            }
            catch (ApiException<GeneralError> ex)
            {
                return new CreateDocumentCollectionResponse
                {
                    StatusCode = (HttpStatusCode)ex.StatusCode,
                    GeneralError = ex.Result
                };
            }
            catch (Exception ex)
            {
                return new CreateDocumentCollectionResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    GeneralError = new GeneralError
                    {
                        Title = ex.Message,
                        TraceId = ex.HResult.ToString()
                    }
                };
            }
        }
        
        public async Task<BaseResult> DeleteDocumentCollectionAsync(string jwt, Guid id)
        {
            try
            {
                UserApiClientHelper.AddAuthentication(jwt, _userApiClient.HttpClient);
                await _userApiClient.DocumentcollectionsDELETEAsync(id);
                return new BaseResult
                {
                    StatusCode = HttpStatusCode.OK,
                };
            }
            catch (ApiException<GeneralError> ex)
            {
                return new BaseResult
                {
                    StatusCode = (HttpStatusCode)ex.StatusCode,
                    GeneralError = ex.Result
                };
            }
            catch (Exception ex)
            {
                return new BaseResult
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    GeneralError = new GeneralError
                    {
                        Title = ex.Message,
                        TraceId = ex.HResult.ToString()
                    }
                };
            }
        }

        public async Task<ResendDocumentCollectionResponse> ResendDocumentCollectionAsync(string jwt, Guid id, Guid signerId, SendingMethod sendingMethod, bool? shouldSend)
        {
            try
            {
                UserApiClientHelper.AddAuthentication(jwt, _userApiClient.HttpClient);
                return new ResendDocumentCollectionResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    SignerLinkResponse = await _userApiClient.MethodAsync(id, signerId, sendingMethod, shouldSend)
                };
            }
            catch (ApiException<GeneralError> ex)
            {
                return new ResendDocumentCollectionResponse
                {
                    StatusCode = (HttpStatusCode)ex.StatusCode,
                    GeneralError = ex.Result
                };
            }
            catch (Exception ex)
            {
                return new ResendDocumentCollectionResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    GeneralError = new GeneralError
                    {
                        Title = ex.Message,
                        TraceId = ex.HResult.ToString()
                    }
                };
            }
        }
        
        public async Task<BaseResult> ShareDocumentCollectionAsync(string jwt, ShareDTO body)
        {
            try
            {
                UserApiClientHelper.AddAuthentication(jwt, _userApiClient.HttpClient);
                await _userApiClient.ShareAsync(body);
                return new BaseResult
                {
                    StatusCode = HttpStatusCode.OK,
                };
            }
            catch (ApiException<GeneralError> ex)
            {
                return new BaseResult
                {
                    StatusCode = (HttpStatusCode)ex.StatusCode,
                    GeneralError = ex.Result
                };
            }
            catch (Exception ex)
            {
                return new BaseResult
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    GeneralError = new GeneralError
                    {
                        Title = ex.Message,
                        TraceId = ex.HResult.ToString()
                    }
                };
            }
        }
        
        public async Task<ExportDocumentCollectionResponse> ExportDocumentCollectionAsync(string jwt, bool? sent, bool? viewed, bool? signed, bool? declined, bool? sendingFailed, bool? canceled)
        {
            try
            {
                UserApiClientHelper.AddAuthentication(jwt, _userApiClient.HttpClient);
                return new ExportDocumentCollectionResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    ExportResponse = await _userApiClient.ExportAsync(sent, viewed, signed, declined, sendingFailed, canceled)
                };
            }
            catch (ApiException<GeneralError> ex)
            {
                return new ExportDocumentCollectionResponse
                {
                    StatusCode = (HttpStatusCode)ex.StatusCode,
                    GeneralError = ex.Result
                };
            }
            catch (Exception ex)
            {
                return new ExportDocumentCollectionResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    GeneralError = new GeneralError
                    {
                        Title = ex.Message,
                        TraceId = ex.HResult.ToString()
                    }
                };
            }
        }
        
        public async Task<ExportDocumentCollectionPdfFieldsResponse> ExportDocumentCollectionPdfFieldsAsync(string jwt, Guid id)
        {
            try
            {
                UserApiClientHelper.AddAuthentication(jwt, _userApiClient.HttpClient);
                return new ExportDocumentCollectionPdfFieldsResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    FieldsResponse = await _userApiClient.FieldsAsync(id)
                };
            }
            catch (ApiException<GeneralError> ex)
            {
                return new ExportDocumentCollectionPdfFieldsResponse
                {
                    StatusCode = (HttpStatusCode)ex.StatusCode,
                    GeneralError = ex.Result
                };
            }
            catch (Exception ex)
            {
                return new ExportDocumentCollectionPdfFieldsResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    GeneralError = new GeneralError
                    {
                        Title = ex.Message,
                        TraceId = ex.HResult.ToString()
                    }
                };
            }
        }

        public async Task<BaseResult> CancelDocumentCollectionAsync(string jwt, Guid id)
        {
            try
            {
                UserApiClientHelper.AddAuthentication(jwt, _userApiClient.HttpClient);
                await _userApiClient.CancelAsync(id);
                return new BaseResult
                {
                    StatusCode = HttpStatusCode.OK,
                };
            }
            catch (ApiException<GeneralError> ex)
            {
                return new BaseResult
                {
                    StatusCode = (HttpStatusCode)ex.StatusCode,
                    GeneralError = ex.Result
                };
            }
            catch (Exception ex)
            {
                return new BaseResult
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    GeneralError = new GeneralError
                    {
                        Title = ex.Message,
                        TraceId = ex.HResult.ToString()
                    }
                };
            }
        }

        public async Task<BaseResult> ReplaceSignerAsync(string jwt, Guid id, Guid signerId, ReplaceSignerDTO body)
        {
            try
            {
                UserApiClientHelper.AddAuthentication(jwt, _userApiClient.HttpClient);
                await _userApiClient.ReplaceAsync(id, signerId, body);
                return new BaseResult
                {
                    StatusCode = HttpStatusCode.OK,
                };
            }
            catch (ApiException<GeneralError> ex)
            {
                return new BaseResult
                {
                    StatusCode = (HttpStatusCode)ex.StatusCode,
                    GeneralError = ex.Result
                };
            }
            catch (Exception ex)
            {
                return new BaseResult
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    GeneralError = new GeneralError
                    {
                        Title = ex.Message,
                        TraceId = ex.HResult.ToString()
                    }
                };
            }
        }

        #endregion
    }
}