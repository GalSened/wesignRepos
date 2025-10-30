using System;

namespace WeSignSigner.Models.Requests
{
    public class GenerateCodeDTO
    {
        public Guid Token { get; set; }
        public string Identification { get; set; }
    }
}
