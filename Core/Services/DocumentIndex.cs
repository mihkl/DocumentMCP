using DocumentMcpServer.Core.Models;

namespace DocumentMcpServer.Core.Services;

/// <summary>
/// Document search and indexing engine.
/// </summary>
public class DocumentIndex
{
    private readonly List<Document> _documents;

    public DocumentIndex()
    {
        _documents = [];
    }

    /// <summary>
    /// Builds the index from a collection of documents.
    /// </summary>
    public void Build(IEnumerable<Document> documents)
    {
        _documents.Clear();
        _documents.AddRange(documents);
    }

    /// <summary>
    /// Queries the index for documents matching the search text.
    /// </summary>
    public List<SearchResult> Query(string searchText)
    {
        var results = new List<SearchResult>();
        var queryLower = searchText.ToLower();
        var keywords = queryLower.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        foreach (var doc in _documents)
        {
            var contentLower = doc.Content.ToLower();
            var titleLower = doc.Title.ToLower();

            var score = CalculateRelevance(titleLower, contentLower, queryLower, keywords);

            if (score > 0)
            {
                var excerpts = ExtractExcerpts(doc.Content, keywords, 3);
                var matches = DescribeMatches(titleLower, contentLower, queryLower, keywords);

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

        return results.OrderByDescending(r => r.RelevanceScore).ToList();
    }

    private static int CalculateRelevance(string title, string content, string fullQuery, string[] keywords)
    {
        var score = 0;

        if (content.Contains(fullQuery))
        {
            score += 20;
        }

        foreach (var keyword in keywords)
        {
            if (title.Contains(keyword))
            {
                score += 10;
            }
            if (content.Contains(keyword))
            {
                score += 1;
            }
        }

        return score;
    }

    private static List<string> DescribeMatches(string title, string content, string fullQuery, string[] keywords)
    {
        var matches = new List<string>();

        foreach (var keyword in keywords)
        {
            if (title.Contains(keyword))
            {
                matches.Add($"Title contains '{keyword}'");
            }
            if (content.Contains(keyword))
            {
                matches.Add($"Content contains '{keyword}'");
            }
        }

        if (content.Contains(fullQuery))
        {
            matches.Add("Exact phrase match");
        }

        return matches;
    }

    private static List<string> ExtractExcerpts(string content, string[] keywords, int maxExcerpts)
    {
        var excerpts = new List<string>();
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var lineLower = line.ToLower();
            if (keywords.Any(k => lineLower.Contains(k)))
            {
                excerpts.Add(line.Trim());
                if (excerpts.Count >= maxExcerpts)
                    break;
            }
        }

        return excerpts;
    }
}
