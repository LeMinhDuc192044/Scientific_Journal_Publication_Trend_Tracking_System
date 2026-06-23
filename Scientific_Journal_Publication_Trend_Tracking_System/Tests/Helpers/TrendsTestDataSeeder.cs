using Scientific_Journal_Publication_Trend_Tracking_System.Domain.Entities;
using Scientific_Journal_Publication_Trend_Tracking_System.Domain.Enums;
using Scientific_Journal_Publication_Trend_Tracking_System.Infrastructure.Persistence;

namespace Scientific_Journal_Publication_Trend_Tracking_System.Tests.Helpers;

public static class TrendsTestDataSeeder
{
    public static readonly Guid MachineLearningTopicId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public static async Task SeedAsync(AppDbContext context)
    {
        var utcNow = DateTime.UtcNow;

        var journalA = new Journal
        {
            Id = Guid.NewGuid(),
            Title = "AI Journal",
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };

        var journalB = new Journal
        {
            Id = Guid.NewGuid(),
            Title = "CS Review",
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };

        var mlKeyword = new Keyword
        {
            Id = Guid.NewGuid(),
            Name = "machine learning",
            FrequencyCount = 0,
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };

        var mlTopic = new ResearchTopic
        {
            Id = MachineLearningTopicId,
            Name = "Machine Learning",
            Description = "ML research",
            Domain = ResearchDomain.ArtificialIntelligence,
            PapersCount = 0,
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };
        mlTopic.Keywords.Add(mlKeyword);

        var papers = new List<ResearchPaper>
        {
            CreatePaper("ext-1", "Deep Learning advances", 2022, 100,
                ["machine learning", "deep learning"], journalA, ResearchDomain.ArtificialIntelligence),
            CreatePaper("ext-2", "Machine Learning survey", 2023, 50,
                ["machine learning", "survey"], journalA, ResearchDomain.ArtificialIntelligence),
            CreatePaper("ext-3", "Machine Learning in 2024", 2024, 20,
                ["machine learning"], journalA, ResearchDomain.ArtificialIntelligence),
            CreatePaper("ext-4", "Old ML paper", 2020, 10,
                ["machine learning"], journalA, ResearchDomain.ArtificialIntelligence),
            CreatePaper("ext-5", "Cybersecurity overview", 2023, 5,
                ["cybersecurity"], journalB, ResearchDomain.ComputerScience),
            CreatePaper("ext-6", "NLP transformer study", 2024, 30,
                ["nlp", "transformer"], journalB, ResearchDomain.ArtificialIntelligence)
        };

        papers[0].ResearchTopics.Add(mlTopic);
        papers[1].ResearchTopics.Add(mlTopic);

        context.Journals.AddRange(journalA, journalB);
        context.Keywords.Add(mlKeyword);
        context.ResearchTopics.Add(mlTopic);
        context.ResearchPapers.AddRange(papers);
        context.Authors.Add(new Author
        {
            Id = Guid.NewGuid(),
            FullName = "John Doe",
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        });

        await context.SaveChangesAsync();
    }

    private static ResearchPaper CreatePaper(
        string externalId,
        string title,
        int year,
        int citations,
        string[] keywords,
        Journal journal,
        ResearchDomain domain)
    {
        var utcNow = DateTime.UtcNow;
        return new ResearchPaper
        {
            Id = Guid.NewGuid(),
            ExternalId = externalId,
            ApiSource = "openalex",
            Title = title,
            Abstract = $"Abstract about {title}",
            PublicationYear = year,
            Doi = $"10.1000/{externalId}",
            CitationCount = citations,
            Keywords = keywords,
            Domain = domain,
            Journal = journal,
            IsFullTextAvailable = false,
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };
    }
}
