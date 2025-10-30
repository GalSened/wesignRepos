using System.Net.Http;
using System.Net.Http.Headers;

namespace UserSoapService.HttpClientLogic
{
    public class UserApiClientHelper
    {
        public static void AddAuthentication(string jwt, HttpClient client)
        {
            if (!string.IsNullOrWhiteSpace(jwt))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
            }
        }
            
    }
}