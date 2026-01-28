using DocumentMcpServer.Core.Interfaces;

namespace DocumentMcpServer.Infrastructure.Extractors;

/// <summary>
/// Registry for document extractors that selects the appropriate extractor for a file.
/// </summary>
public class ExtractorRegistry(IEnumerable<IDocumentExtractor> extractors)
{
    private readonly IEnumerable<IDocumentExtractor> _extractors = extractors;

    /// <summary>
    /// Gets the first extractor that can handle the given file path.
    /// </summary>
    public IDocumentExtractor? GetExtractor(string filePath)
    {
        return _extractors.FirstOrDefault(e => e.CanExtract(filePath));
    }

    /// <summary>
    /// Extracts content from a file using the appropriate extractor.
    /// </summary>
    public string ExtractContent(string filePath)
    {
        var extractor = GetExtractor(filePath);
        if (extractor != null)
        {
            return extractor.ExtractContent(filePath);
        }
        
        return $"[Binary file: {Path.GetFileName(filePath)}]";
    }
}
