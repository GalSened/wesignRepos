using Common.Enums.Results;
using Common.Extensions;
using System;
using System.Collections.Generic;

namespace Common.Models
{
    public class GeneralError
    {
        public GeneralError(string resultCode, string innerResultCode = "", DateTime? time = null, string extraInfo = "")
        {
            
            Enum.TryParse(resultCode, out ResultCode myResultCode);
            errors = new Dictionary<string, string[]>
            {
                ["error"] = string.IsNullOrWhiteSpace(extraInfo)?
                            new string[] { myResultCode.GetDescription() } :
                            new string[] { myResultCode.GetDescription() , extraInfo}
            };
            status = (int)myResultCode;
            traceId = innerResultCode;
            title = time != null ? $"Internal server error occurred at: {time}" :
                                    "Invalid operation error occurred";
        }
        public IDictionary<string, string[]> errors { get; set; }

        public string title { get; set; }
        public int status { get; set; }
        public string traceId { get; set; }
    }

    //public class Error
    //{        
    //    public string[] error { get; set; }
    //}
}
