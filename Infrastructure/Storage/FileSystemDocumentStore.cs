using DocumentMcpServer.Core.Interfaces;
using DocumentMcpServer.Core.Models;
using Microsoft.Extensions.Logging;

namespace DocumentMcpServer.Infrastructure.Storage;

/// <summary>
/// Loads documents from the file system.
/// </summary>
public class FileSystemDocumentStore : IDocumentStore
{
    private readonly string _documentsPath;
    private readonly IEnumerable<IDocumentExtractor> _extractors;
    private readonly ILogger? _logger;
    private readonly List<Document> _documents;

    public FileSystemDocumentStore(
        string documentsPath,
        IEnumerable<IDocumentExtractor> extractors,
        ILogger<FileSystemDocumentStore>? logger = null)
    {
        _documentsPath = documentsPath;
        _extractors = extractors;
        _logger = logger;
        _documents = [];
        
        LoadDocumentsFromFileSystem();
    }

    public IEnumerable<Document> LoadDocuments() => _documents;

    public Document? GetById(string id)
    {
        return _documents.FirstOrDefault(d =>
            d.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
    }

    private void LoadDocumentsFromFileSystem()
    {
        if (!Directory.Exists(_documentsPath))
        {
            _logger?.LogWarning("Documents path not found: {Path}", _documentsPath);
            return;
        }

        _logger?.LogInformation("Loading documents from: {Path}", _documentsPath);
        var files = Directory.GetFiles(_documentsPath, "*.*", SearchOption.AllDirectories).ToList();
        _logger?.LogInformation("Found {Count} files", files.Count);

        foreach (var filePath in files)
        {
            try
            {
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                var relativePath = Path.GetRelativePath(_documentsPath, filePath);

                var extractor = _extractors.FirstOrDefault(e => e.CanExtract(filePath));
                var content = extractor != null
                    ? extractor.ExtractContent(filePath)
                    : $"[Binary file: {Path.GetFileName(filePath)}]";

                var title = fileName;
                if (!string.IsNullOrWhiteSpace(content) && !content.StartsWith("["))
                {
                    var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                    if (lines.Length > 0)
                    {
                        title = lines[0].Trim();
                        if (title.Length > 100) title = title.Substring(0, 100) + "...";
                    }
                }

                _documents.Add(new Document
                {
                    Id = fileName.ToLower().Replace(" ", "-"),
                    Title = title,
                    Date = File.GetLastWriteTime(filePath),
                    Content = content,
                    FilePath = filePath
                });

                _logger?.LogDebug("Loaded: {Path}", relativePath);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error loading {Path}", filePath);
            }
        }

        if (_documents.Count == 0)
        {
            _logger?.LogWarning("No documents loaded");
        }
    }
}
