using Common.Interfaces;
using System.Collections.Generic;
using System.Text;

namespace Common.Handlers
{
    public class TableFormatterHandler : ITableFormatter
    {
        public string CreateHtmlTableSyntax(List<string> headers, List<List<string>> rows, Dictionary<string, string> headerColorMap)
        {
            // Using StringBuilder for efficient string concatenation
            StringBuilder htmlOutput = new StringBuilder();

            // Start the <table> tag with border style
            htmlOutput.AppendLine("<table style='border-collapse: collapse;'>");

            // Create the header row with background color and borders (use <th> tags)
            htmlOutput.AppendLine("<tr>");
            foreach (var header in headers)
            {
                htmlOutput.AppendLine($"<th style='background-color: #30BBDE; color: white; border: 1px solid black; padding: 5px;'>{header}</th>");
            }
            htmlOutput.AppendLine("</tr>");

            // Create the data rows with borders and color based on header (use <td> tags)
            foreach (var row in rows)
            {
                htmlOutput.AppendLine("<tr>");
                for (int i = 0; i < row.Count; i++)
                {
                    string cellColor = GetCellColor(headers[i], headerColorMap); // Determine the color for the cell
                    htmlOutput.AppendLine($"<td style='border: 1px solid black; padding: 5px; color: {cellColor};font-size: small;'>{row[i]}</td>");
                }
                htmlOutput.AppendLine("</tr>");
            }

            // End the <table> tag
            htmlOutput.AppendLine("</table>");

            // Append the <br/> tag at the end
            htmlOutput.AppendLine("<br/>");

            return htmlOutput.ToString();
        }

        // Helper function to get the color based on the header name
        private string GetCellColor(string header, Dictionary<string, string> headerColorMap)
        {
            // Check if the header is in the color map and return the corresponding color
            if (headerColorMap.ContainsKey(header))
            {
                return headerColorMap[header];
            }

            // Default color if no specific color is found
            return "black";
        }
    }
}
