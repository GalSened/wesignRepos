using System.Collections.Generic;

namespace Common.Interfaces
{
    public interface ITableFormatter
    {
        string CreateHtmlTableSyntax(List<string> headers, List<List<string>> rows, Dictionary<string, string> headerColorMap);
    }
}
