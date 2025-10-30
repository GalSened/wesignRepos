using System.Net;
using UserSoapService.HttpClientLogic;

namespace UserSoapService.Responses
{
    public class BaseResult
    {
 
        public HttpStatusCode StatusCode { get; set; }
 
        public GeneralError GeneralError { get; set; }
    }
}