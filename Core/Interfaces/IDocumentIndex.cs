using DocumentMcpServer.Core.Models;

namespace DocumentMcpServer.Core.Interfaces;

/// <summary>
/// Document indexing and search service.
/// </summary>
public interface IDocumentIndex
{
    /// <summary>
    /// Builds the index from a collection of documents.
    /// </summary>
    void Build(IEnumerable<Document> documents);

    /// <summary>
    /// Queries the index for documents matching the search text.
    /// </summary>
    List<SearchResult> Query(string searchText);
}
