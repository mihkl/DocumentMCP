using DocumentMcpServer.Core.Interfaces;

namespace DocumentMcpServer.Infrastructure.Extractors;

public class TextFileExtractor : IDocumentExtractor
{
    private static readonly string[] SupportedExtensions = 
    [
        ".txt", ".md", ".json", ".xml", ".csv", ".log",
        ".html", ".css", ".js", ".ts", ".cs", ".java",
        ".py", ".yml", ".yaml", ".config", ".env",
        ".c", ".cpp", ".h", ".hpp", ".go", ".rs", ".rb",
        ".php", ".swift", ".kt", ".scala", ".sh", ".bat"
    ];

    public bool CanExtract(string filePath)
    {
        var ext = Path.GetExtension(filePath).ToLower();
        return SupportedExtensions.Contains(ext);
    }

    public string ExtractContent(string filePath)
    {
        try
        {
            return File.ReadAllText(filePath);
        }
        catch (Exception ex)
        {
            return $"[Unable to read: {Path.GetFileName(filePath)} - {ex.Message}]";
        }
    }
}
