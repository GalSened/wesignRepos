using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WSE_ADAuth.Extensions
{
    public static class PhoneExtractExtension
    {
        private static Dictionary<string, string> _phoneExtentionReplace = new Dictionary<string, string> { { "972", "0"},{ "+972", "0"} };
        public static List<string> GetAllPhones(this string str)
        {
            List<string> phones = new List<string>();
            if (string.IsNullOrWhiteSpace(str))
            {
                return phones;
            }

            phones.Add(str);
            str = str.Replace(" ", "");
            str = str.Replace("-", "");
            phones.Add(str);
            foreach (var pair in _phoneExtentionReplace)
            {
                if (str.Contains(pair.Key))
                {
                    var tempPhone = str.Replace(pair.Key, pair.Value).Trim();
                    if (!phones.Contains(tempPhone))
                    {
                        phones.Add(tempPhone);
                    }
                }
            }


            return phones;
        }

    }
}
