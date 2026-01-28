using NUnit.Framework;
using DocumentMcpServer.Core.Services;
using DocumentMcpServer.Core.Interfaces;
using DocumentMcpServer.Core.Models;

namespace DocumentMcpServer.Tests;

/// <summary>
/// Mock document store for testing
/// </summary>
public class MockDocumentStore(List<Document> documents) : IDocumentStore
{
    private readonly List<Document> _documents = documents;

    public IEnumerable<Document> LoadDocuments() => _documents;

    public Document? GetById(string id) =>
        _documents.FirstOrDefault(d => d.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
}

public class DocumentServiceTests
{
    [Test]
    public void ListDocuments_ReturnsAllDocuments()
    {
        var documents = new List<Document>
        {
            new() {
                Id = "doc1",
                Title = "Document 1",
                Content = "Content 1",
                Date = DateTime.Parse("2024-01-01"),
                FilePath = "doc1.txt"
            },
            new() {
                Id = "doc2",
                Title = "Document 2",
                Content = "Content 2",
                Date = DateTime.Parse("2024-01-02"),
                FilePath = "doc2.txt"
            }
        };

        var store = new MockDocumentStore(documents);
        var searchIndex = new DocumentIndex();
        var service = new DocumentService(store, searchIndex);

        var result = service.ListDocuments();

        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result, Has.Some.Matches<DocumentSummary>(d => d.Id == "doc1"));
        Assert.That(result, Has.Some.Matches<DocumentSummary>(d => d.Id == "doc2"));
    }

    [Test]
    public void GetDocument_WithValidId_ReturnsDocument()
    {
        var documents = new List<Document>
        {
            new() {
                Id = "test-doc",
                Title = "Test Document",
                Content = "Test content",
                Date = DateTime.Now,
                FilePath = "test.txt"
            }
        };
        var store = new MockDocumentStore(documents);
        var searchIndex = new DocumentIndex();
        var service = new DocumentService(store, searchIndex);

        var result = service.GetDocument("test-doc");

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo("test-doc"));
        Assert.That(result.Title, Is.EqualTo("Test Document"));
    }

    [Test]
    public void GetDocument_WithInvalidId_ReturnsNull()
    {
        var store = new MockDocumentStore([]);
        var searchIndex = new DocumentIndex();
        var service = new DocumentService(store, searchIndex);

        var result = service.GetDocument("nonexistent");

        Assert.That(result, Is.Null);
    }

    [Test]
    public void SearchDocuments_FindsMatches()
    {
        var documents = new List<Document>
        {
            new() {
                Id = "doc1",
                Title = "Kubernetes Architecture",
                Content = "Kubernetes is a container orchestration platform",
                Date = DateTime.Now,
                FilePath = "doc1.txt"
            },
            new() {
                Id = "doc2",
                Title = "Docker Basics",
                Content = "Docker is used to containerize applications",
                Date = DateTime.Now,
                FilePath = "doc2.txt"
            }
        };

        var store = new MockDocumentStore(documents);
        var searchIndex = new DocumentIndex();
        var service = new DocumentService(store, searchIndex);

        var results = service.SearchDocuments("kubernetes");

        Assert.That(results, Is.Not.Empty);
        Assert.That(results, Has.Some.Matches<SearchResult>(r => r.DocumentId == "doc1"));
    }

    [Test]
    public void SummarizeDocument_WithValidId_ReturnsSummary()
    {
        var documents = new List<Document>
        {
            new() {
                Id = "test-doc",
                Title = "Test Document",
                Content = "1. Introduction\n2. Main Content\n3. Conclusion",
                Date = DateTime.Parse("2024-01-01"),
                FilePath = "test.txt"
            }
        };

        var store = new MockDocumentStore(documents);
        var searchIndex = new DocumentIndex();
        var service = new DocumentService(store, searchIndex);

        var summary = service.SummarizeDocument("test-doc");

        Assert.That(summary, Does.Contain("Test Document"));
        Assert.That(summary, Does.Contain("test-doc"));
        Assert.That(summary, Does.Contain("2024-01-01"));
    }

    [Test]
    public void SummarizeDocument_WithInvalidId_ReturnsNotFoundMessage()
    {
        var store = new MockDocumentStore([]);
        var searchIndex = new DocumentIndex();
        var service = new DocumentService(store, searchIndex);

        var summary = service.SummarizeDocument("nonexistent");

        Assert.That(summary.ToLower(), Does.Contain("not found"));
    }
}
