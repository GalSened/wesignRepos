using UserSoapService.HttpClientLogic;

namespace UserSoapService.Responses
{   
    public class GetContactAsyncResponse : BaseResult
    {
        public ContactResponseDTO Contact { get; set; }        
    }

    public class GetContactsAsyncResponse : BaseResult
    {
        public AllContactResponseDTO Contacts { get; set; }
    }

    public class CreateContactResponse : BaseResult
    {
        public CreateContactResponseDTO ContactData { get; set; }
    }
    

}