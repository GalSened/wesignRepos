using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using UserSoapService.HttpClientLogic;

namespace UserSoapService.Responses
{
    public class GetCurrentUserDetailsResponse : BaseResult
    {
        public UserResponseDTO UserDetails{ get; set; }
    }

    public class LoginResponse : BaseResult
    {
        public UserTokensResponseDTO UserTokens { get; set; }
    }

    public class SignUpResponse : BaseResult
    {
        public LinkResponse Link{ get; set; }
    }    
}