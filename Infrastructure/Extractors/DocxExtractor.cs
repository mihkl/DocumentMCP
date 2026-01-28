using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentMcpServer.Core.Interfaces;
using System.Text;

namespace DocumentMcpServer.Infrastructure.Extractors;

/// <summary>
/// Extracts content from Microsoft Word documents (.docx).
/// </summary>
public class DocxExtractor : IDocumentExtractor
{
    public bool CanExtract(string filePath)
    {
        return Path.GetExtension(filePath).Equals(".docx", StringComparison.OrdinalIgnoreCase);
    }

    public string ExtractContent(string filePath)
    {
        try
        {
            using var doc = WordprocessingDocument.Open(filePath, false);
            var body = doc.MainDocumentPart?.Document.Body;
            if (body == null)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            foreach (var text in body.Descendants<Text>())
            {
                sb.Append(text.Text);
            }
            return sb.ToString();
        }
        catch (Exception ex)
        {
            return $"[Unable to read Word document: {ex.Message}]";
        }
    }
}
