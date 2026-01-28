using DocumentMcpServer.Core.Interfaces;
using DocumentMcpServer.Core.Models;

namespace DocumentMcpServer.Infrastructure.Storage;

/// <summary>
/// Loads documents from the file system.
/// </summary>
public class FileSystemDocumentStore : IDocumentStore
{
    private readonly string _documentsPath;
    private readonly IEnumerable<IDocumentExtractor> _extractors;
    private readonly List<Document> _documents;

    public event Action<string>? Log;

    public FileSystemDocumentStore(
        string documentsPath,
        IEnumerable<IDocumentExtractor> extractors)
    {
        _documentsPath = documentsPath;
        _extractors = extractors;
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
            Log?.Invoke($"WARNING: Documents path not found: {_documentsPath}");
            return;
        }

        Log?.Invoke($"Loading documents from: {_documentsPath}");
        var files = Directory.GetFiles(_documentsPath, "*.*", SearchOption.AllDirectories).ToList();
        Log?.Invoke($"Found {files.Count} files");

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
                        if (title.Length > 100)
                        {
                            title = string.Concat(title.AsSpan(0, 100), "...");
                        }
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

                Log?.Invoke($"  Loaded: {relativePath}");
            }
            catch (Exception ex)
            {
                Log?.Invoke($"  Error loading {filePath}: {ex.Message}");
            }
        }

        if (_documents.Count == 0)
        {
            Log?.Invoke("WARNING: No documents loaded. Server will return empty results.");
        }
    }
}
