namespace DocumentMcpServer.Core.Models;

public class Document
{
    public required string Id { get; set; }
    public required string Title { get; set; }
    public DateTime Date { get; set; }
    public required string Content { get; set; }
    public required string FilePath { get; set; }
}

public class DocumentSummary
{
    public required string Id { get; set; }
    public required string Title { get; set; }
    public DateTime Date { get; set; }
    public required string FilePath { get; set; }
}

public class SearchResult
{
    public required string DocumentId { get; set; }
    public required string Title { get; set; }
    public required string FilePath { get; set; }
    public int RelevanceScore { get; set; }
    public required List<string> Excerpts { get; set; }
    public required string MatchSummary { get; set; }
}
