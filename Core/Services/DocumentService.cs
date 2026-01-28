using DocumentMcpServer.Core.Interfaces;
using DocumentMcpServer.Core.Models;
using Microsoft.Extensions.Logging;

namespace DocumentMcpServer.Core.Services;

/// <summary>
/// Core document service: search, retrieve, summarize.
/// </summary>
public class DocumentService : IDocumentService
{
    private readonly List<Document> _documents;
    private readonly IDocumentIndex _searchIndex;
    private readonly ILogger? _logger;

    public DocumentService(IDocumentStore store, IDocumentIndex searchIndex, ILogger<DocumentService>? logger = null)
    {
        _logger = logger;
        _searchIndex = searchIndex;
        _documents = [.. store.LoadDocuments()];
        
        _searchIndex.Build(_documents);
        
        _logger?.LogInformation("DocumentService initialized with {Count} documents", _documents.Count);
    }

    public List<DocumentSummary> ListDocuments()
    {
        return [.. _documents.Select(d => new DocumentSummary
        {
            Id = d.Id,
            Title = d.Title,
            Date = d.Date,
            FilePath = d.FilePath
        })];
    }

    public Document? GetDocument(string documentId)
    {
        return _documents.FirstOrDefault(d =>
            d.Id.Equals(documentId, StringComparison.OrdinalIgnoreCase));
    }

    public List<SearchResult> SearchDocuments(string query)
    {
        _logger?.LogInformation("Searching for: {Query}", query);
        
        var results = _searchIndex.Query(query);
        
        _logger?.LogInformation("Found {Count} matching documents", results.Count);
        return results;
    }

    public string SummarizeDocument(string documentId)
    {
        var doc = GetDocument(documentId);
        if (doc == null)
        {
            _logger?.LogWarning("Document not found: {DocumentId}", documentId);
            return $"Document '{documentId}' not found.";
        }

        var lines = doc.Content.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(l => l.Trim())
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .ToList();

        var keySections = lines
            .Where(l => System.Text.RegularExpressions.Regex.IsMatch(l, @"^\d+\.") || l.StartsWith("-"))
            .Take(8)
            .ToList();

        var summary = $@"DOCUMENT SUMMARY

Title: {doc.Title}
Document ID: {doc.Id}
File Path: {doc.FilePath}
Date: {doc.Date:yyyy-MM-dd}

KEY SECTIONS:
{string.Join("\n", keySections)}

Total Length: {doc.Content.Length} characters
";

        _logger?.LogInformation("Generated summary for: {DocumentId}", documentId);
        return summary;
    }
}
