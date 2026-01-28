using NUnit.Framework;
using DocumentMcpServer.Core.Services;
using DocumentMcpServer.Core.Models;

namespace DocumentMcpServer.Tests;

public class DocumentIndexRankingTests
{
    [Test]
    public void Query_ExactPhraseInContent_GetsHighScore()
    {
        var documents = new List<Document>
        {
            new() {
                Id = "doc1",
                Title = "Guide",
                Content = "machine learning techniques",
                Date = DateTime.Now,
                FilePath = "doc1.txt"
            },
            new() {
                Id = "doc2",
                Title = "Reference",
                Content = "machine and learning separately",
                Date = DateTime.Now,
                FilePath = "doc2.txt"
            }
        };

        var index = new DocumentIndex();
        index.Build(documents);

        var results = index.Query("machine learning");

        Assert.That(results, Has.Count.EqualTo(2));
        Assert.That(results[0].DocumentId, Is.EqualTo("doc1"));
        Assert.That(results[0].RelevanceScore, Is.GreaterThan(results[1].RelevanceScore));
    }

    [Test]
    public void Query_TitleMatchesOutrankContentMatches()
    {
        var documents = new List<Document>
        {
            new() {
                Id = "doc1",
                Title = "Python Programming",
                Content = "Introduction to software development",
                Date = DateTime.Now,
                FilePath = "doc1.txt"
            },
            new() {
                Id = "doc2",
                Title = "Software Guide",
                Content = "Other topics without the keyword",
                Date = DateTime.Now,
                FilePath = "doc2.txt"
            }
        };

        var index = new DocumentIndex();
        index.Build(documents);

        var results = index.Query("python");

        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results[0].DocumentId, Is.EqualTo("doc1"));
    }

    [Test]
    public void Query_MultipleKeywords_AggregatesScores()
    {
        var documents = new List<Document>
        {
            new() {
                Id = "doc1",
                Title = "Kubernetes",
                Content = "Container orchestration",
                Date = DateTime.Now,
                FilePath = "doc1.txt"
            },
            new() {
                Id = "doc2",
                Title = "Docker Guide",
                Content = "Docker with Kubernetes",
                Date = DateTime.Now,
                FilePath = "doc2.txt"
            }
        };

        var index = new DocumentIndex();
        index.Build(documents);

        var results = index.Query("kubernetes docker");

        Assert.That(results, Has.Count.EqualTo(2));
        Assert.That(results[0].RelevanceScore, Is.GreaterThan(0));
    }

    [Test]
    public void Query_CaseInsensitive_FindsMatches()
    {
        var documents = new List<Document>
        {
            new() {
                Id = "doc1",
                Title = "UPPERCASE TITLE",
                Content = "lowercase content",
                Date = DateTime.Now,
                FilePath = "doc1.txt"
            }
        };

        var index = new DocumentIndex();
        index.Build(documents);

        var results = index.Query("uppercase");

        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results[0].DocumentId, Is.EqualTo("doc1"));
    }

    [Test]
    public void Query_EmptyQuery_ReturnsNoResults()
    {
        var documents = new List<Document>
        {
            new() {
                Id = "doc1",
                Title = "Test",
                Content = "Content",
                Date = DateTime.Now,
                FilePath = "doc1.txt"
            }
        };

        var index = new DocumentIndex();
        index.Build(documents);

        var results = index.Query("   ");

        Assert.That(results, Is.Empty);
    }

    [Test]
    public void Query_ReturnsExcerpts()
    {
        var documents = new List<Document>
        {
            new() {
                Id = "doc1",
                Title = "Test Document",
                Content = "This is a test document with kubernetes mentioned in the content multiple times. Kubernetes is great.",
                Date = DateTime.Now,
                FilePath = "doc1.txt"
            }
        };

        var index = new DocumentIndex();
        index.Build(documents);

        var results = index.Query("kubernetes");

        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results[0].Excerpts, Is.Not.Empty);
        Assert.That(results[0].MatchSummary, Does.Contain("kubernetes"));
    }
}
