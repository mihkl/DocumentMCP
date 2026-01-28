using NUnit.Framework;
using DocumentMcpServer.Core.Services;
using DocumentMcpServer.Core.Models;

namespace DocumentMcpServer.Tests;

public class DocumentIndexTests
{
    [Test]
    public void Query_WithExactPhrase_ReturnsHigherScore()
    {
        var documents = new List<Document>
        {
            new() {
                Id = "doc1",
                Title = "Introduction to Machine Learning",
                Content = "Machine learning is a subset of artificial intelligence",
                Date = DateTime.Now,
                FilePath = "doc1.txt"
            },
            new() {
                Id = "doc2",
                Title = "Data Science Guide",
                Content = "Data science involves statistics and machine learning techniques",
                Date = DateTime.Now,
                FilePath = "doc2.txt"
            }
        };

        var index = new DocumentIndex();
        index.Build(documents);

        var results = index.Query("machine learning");

        Assert.That(results, Is.Not.Empty);
        Assert.That(results[0].DocumentId, Is.EqualTo("doc1"));
        Assert.That(results[0].RelevanceScore, Is.GreaterThan(20));
    }

    [Test]
    public void Query_WithNoMatches_ReturnsEmpty()
    {
        var documents = new List<Document>
        {
            new() {
                Id = "doc1",
                Title = "Test Document",
                Content = "Some content here",
                Date = DateTime.Now,
                FilePath = "doc1.txt"
            }
        };

        var index = new DocumentIndex();
        index.Build(documents);

        var results = index.Query("nonexistent keyword");

        Assert.That(results, Is.Empty);
    }

    [Test]
    public void Query_WithTitleMatch_BoostsScore()
    {
        var documents = new List<Document>
        {
            new() {
                Id = "doc1",
                Title = "Kubernetes Guide",
                Content = "Container orchestration platform",
                Date = DateTime.Now,
                FilePath = "doc1.txt"
            },
            new() {
                Id = "doc2",
                Title = "Docker Guide",
                Content = "This covers k8s and containers",
                Date = DateTime.Now,
                FilePath = "doc2.txt"
            }
        };

        var index = new DocumentIndex();
        index.Build(documents);

        var results = index.Query("kubernetes");

        Assert.That(results.Count, Is.EqualTo(1));
        Assert.That(results[0].DocumentId, Is.EqualTo("doc1"));
    }
}
