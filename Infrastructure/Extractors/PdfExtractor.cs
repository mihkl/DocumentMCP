using DocumentMcpServer.Core.Interfaces;
using iTextSharp.text.pdf;
using System.Text;

namespace DocumentMcpServer.Infrastructure.Extractors;

/// <summary>
/// Extracts content from PDF documents.
/// </summary>
public class PdfExtractor : IDocumentExtractor
{
    public bool CanExtract(string filePath)
    {
        return Path.GetExtension(filePath).Equals(".pdf", StringComparison.OrdinalIgnoreCase);
    }

    public string ExtractContent(string filePath)
    {
        try
        {
            var sb = new StringBuilder();
            using var reader = new PdfReader(filePath);

            for (int page = 1; page <= reader.NumberOfPages; page++)
            {
                var contentBytes = reader.GetPageContent(page);
                var content = Encoding.UTF8.GetString(contentBytes);

                var matches = System.Text.RegularExpressions.Regex.Matches(content, @"\((.*?)\)");
                foreach (System.Text.RegularExpressions.Match match in matches)
                {
                    sb.Append(match.Groups[1].Value).Append(' ');
                }
            }

            return sb.ToString();
        }
        catch (Exception ex)
        {
            return $"[Unable to read PDF: {ex.Message}]";
        }
    }
}
