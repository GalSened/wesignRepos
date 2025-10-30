using System;
using System.Collections.Generic;
using System.IO;

 namespace Common.Interfaces

{
    public interface ICsvHandler<T> // to Delete
    {
        IEnumerable<T> ConvertCsvToModel<TKey>(TextReader textReader, Func<T, TKey> groupBy = null);
    }
}