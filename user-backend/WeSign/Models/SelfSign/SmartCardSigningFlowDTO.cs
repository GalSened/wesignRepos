using System.Collections.Generic;

namespace WeSign.Models.SelfSign
{
    public class SmartCardSigningFlowDTO
    {
        public string Token { get; set; }
        public List<SmartCardSignFlowFieldsDTO> Fields { get; set; }
    }
}
