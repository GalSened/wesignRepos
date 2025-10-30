using Common.Dictionaries;
using Common.Enums.Results;
using Common.Enums.Users;
using Common.Extensions;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Common.Handlers
{
    public static class CsvHandler
    {
        public static byte[] ExportDocumentsCollection(IEnumerable<object> DTOCollection, Language language)
        {
            if (DTOCollection.Count() == 0)
            {
                throw new InvalidOperationException(ResultCode.InvalidObjectType.GetNumericString());
            }

            if (!validateListType(DTOCollection))
            {
                throw new InvalidOperationException(ResultCode.InvalidObjectType.GetNumericString());
            }

            IList<IDictionary<string, object>> rows = GetCsvInfo(DTOCollection, language);
            string result = DocumentsRowsToString(rows);

            return Encoding.UTF8.GetBytes(result);
        }

        private static bool validateListType(IEnumerable<object> DTOCollection)
        {
            var type = DTOCollection.First().GetType();
            return true;
        }

        private static string DocumentsRowsToString(IList<IDictionary<string, object>> rows)
        {
            var delimiter = ",";
            StringBuilder result = new StringBuilder();

            foreach (var item in rows ?? Enumerable.Empty<IDictionary<string, object>>())
            {
                foreach (var innerItem in item ?? Enumerable.Empty<KeyValuePair<string, object>>())
                {
                    result.Append(innerItem.Value);
                    result.Append(delimiter);
                }
                result.AppendLine();
            }

            return result.ToString();
        }

        private static IList<IDictionary<string, object>> GetCsvInfo(IEnumerable<object> DTOCollection, Language language)
        {
            if (DTOCollection.ToList().Count() == 0)
            {
                return null;
            }

            IDictionary<string, object> header = GetCsvHeader(DTOCollection, language);
            IList<IDictionary<string, object>> rows = new List<IDictionary<string, object>>() { header };
            AddCsvBody(DTOCollection, rows);
            return rows;
        }

        private static void AddCsvBody(IEnumerable<object> DTOCollection, IList<IDictionary<string, object>> rows)
        {
            foreach (object obj in DTOCollection)
            {
                IDictionary<string, object> row = new ExpandoObject();

                foreach (PropertyInfo propertyInfo in obj.GetType().GetProperties())
                {
                    row.Add(propertyInfo.Name, propertyInfo.GetValue(obj, null));
                }

                rows.Add(row);
            }
        }

        private static IDictionary<string, object> GetCsvHeader(IEnumerable<object> DTOCollection, Language language)
        {
            IDictionary<string, object> header = new ExpandoObject();

            foreach (PropertyInfo propertyInfo in DTOCollection.First().GetType().GetProperties())
            {
                var a = propertyInfo.Name;

                if (LanguageDictionary.languageDictionary[language].ContainsKey(propertyInfo.Name))
                {
                    header.Add(propertyInfo.Name, LanguageDictionary.languageDictionary[language][propertyInfo.Name]);
                }

                else
                {
                    header.Add(propertyInfo.Name, propertyInfo.Name);
                }
            }

            return header;
        }
    }
}