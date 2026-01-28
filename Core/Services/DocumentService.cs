using DocumentMcpServer.Core.Interfaces;
using DocumentMcpServer.Core.Models;

namespace DocumentMcpServer.Core.Services;

public class DocumentService : IDocumentService
{
    private readonly IDocumentStore _store;
    private readonly List<Document> _documents;

    public event Action<string>? Log;

    public DocumentService(IDocumentStore store)
    {
        _store = store;
        _documents = [.. _store.LoadDocuments()];

        Log?.Invoke($"DocumentService initialized with {_documents.Count} documents");
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
        var results = new List<SearchResult>();
        var queryLower = query.ToLower();
        var keywords = queryLower.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        Log?.Invoke($"Searching for: {query}");

        foreach (var doc in _documents)
        {
            var contentLower = doc.Content.ToLower();
            var titleLower = doc.Title.ToLower();

            var score = 0;
            var matches = new List<string>();

            foreach (var keyword in keywords)
            {
                if (titleLower.Contains(keyword))
                {
                    score += 10;
                    matches.Add($"Title contains '{keyword}'");
                }
                if (contentLower.Contains(keyword))
                {
                    score += 1;
                    matches.Add($"Content contains '{keyword}'");
                }
            }

            if (contentLower.Contains(queryLower))
            {
                score += 20;
                matches.Add($"Exact phrase match");
            }

            if (score > 0)
            {
                var excerpts = ExtractExcerpts(doc.Content, keywords, 3);

                results.Add(new SearchResult
                {
                    DocumentId = doc.Id,
                    Title = doc.Title,
                    FilePath = doc.FilePath,
                    RelevanceScore = score,
                    Excerpts = excerpts,
                    MatchSummary = string.Join("; ", matches.Distinct())
                });
            }
        }

        Log?.Invoke($"Found {results.Count} matching documents");
        return results.OrderByDescending(r => r.RelevanceScore).ToList();
    }

    public string SummarizeDocument(string documentId)
    {
        var doc = GetDocument(documentId);
        if (doc == null)
        {
            Log?.Invoke($"Document not found: {documentId}");
            return $"Document '{documentId}' not found.";
        }

        var lines = doc.Content.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(l => l.Trim())
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .ToList();

        var keySections = lines
            .Where(l =>
                System.Text.RegularExpressions.Regex.IsMatch(l, @"^\d+\.") ||
                l.StartsWith("-"))
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

        Log?.Invoke($"Generated summary for: {documentId}");
        return summary;
    }

    private static List<string> ExtractExcerpts(string content, string[] keywords, int maxExcerpts)
    {
        var excerpts = new List<string>();
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var lineLower = line.ToLower();
            if (keywords.Any(lineLower.Contains))
            {
                excerpts.Add(line.Trim());
                if (excerpts.Count >= maxExcerpts)
                {
                    break;
                }
            }
        }

        return excerpts;
    }
}
