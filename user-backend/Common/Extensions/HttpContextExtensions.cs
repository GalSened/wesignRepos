using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Common.Extensions
{
    public static class HttpContextExtensions
    {
        public static string GetRemoteIPAddress(this HttpContext context, bool allowForwarded = true)
        {
            if (allowForwarded)
            {
                string header =  context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
                if (IPAddress.TryParse(header, out _))
                {
                    return header;
                }
            }
            return context.Connection.RemoteIpAddress.ToString();
        }
    }
}
