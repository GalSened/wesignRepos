using Common.Interfaces;
using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace BL.Handlers.FilesHandler
{
    public class CsvHandler<T> : ICsvHandler<T>
    {

        /// <summary>
        /// Csv helper to convert csv as base64 to model
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="textReader"></param>
        /// <param name="groupBy"></param>
        /// <returns></returns>
        public IEnumerable<T> ConvertCsvToModel<TKey>(TextReader textReader, Func<T, TKey> groupBy = null)
        {
            var csvReader = new CsvReader(textReader, CultureInfo.CurrentCulture);
            var csvlist = csvReader.GetRecords<T>();
            return groupBy == null ? csvlist :
                csvlist.GroupBy(groupBy)
                .Select(group => group.First());
        }
    }
}
