using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Extensions
{
    public static class ByteExtentions
    {

        static readonly char[] padding = { '=' };

        public static string ToBase64Url(this byte[] buffer)
        {
            if (buffer == null)
                return string.Empty;

            string base64Url = Convert.ToBase64String(buffer)
                .TrimEnd(padding).Replace('+', '-').Replace('/', '_');

            return base64Url;
        }
    }
}
