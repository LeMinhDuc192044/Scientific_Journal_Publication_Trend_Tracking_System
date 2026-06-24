using Microsoft.EntityFrameworkCore;
using Scientific_Journal_Publication_Trend_Tracking_System.Domain.Entities;
using Scientific_Journal_Publication_Trend_Tracking_System.Domain.Enums;
using Scientific_Journal_Publication_Trend_Tracking_System.Infrastructure.Persistence;

namespace Scientific_Journal_Publication_Trend_Tracking_System.src.Infrastructure.Seeding;

public static class ResearchTopicSeeder
{
    private static readonly (string Name, string Description, ResearchDomain Domain, string[] Keywords)[] DefaultTopics =
    [
        ("Artificial Intelligence", "AI research including agents, reasoning, and intelligent systems",
            ResearchDomain.ArtificialIntelligence,
            ["artificial intelligence", "ai", "intelligent systems"]),
        ("Machine Learning", "Supervised, unsupervised, and reinforcement learning methods",
            ResearchDomain.ArtificialIntelligence,
            ["machine learning", "supervised learning", "classification"]),
        ("Deep Learning", "Neural networks and deep architectures",
            ResearchDomain.ArtificialIntelligence,
            ["deep learning", "neural network", "cnn", "rnn"]),
        ("Natural Language Processing", "Text understanding, generation, and language models",
            ResearchDomain.ArtificialIntelligence,
            ["natural language processing", "nlp", "text mining", "language model"]),
        ("Computer Vision", "Image and video understanding",
            ResearchDomain.ComputerScience,
            ["computer vision", "image recognition", "object detection"]),
        ("Generative AI", "Generative models including LLMs and diffusion models",
            ResearchDomain.ArtificialIntelligence,
            ["generative ai", "llm", "transformer", "diffusion"]),
        ("Cybersecurity", "Security, privacy, and threat detection in computing systems",
            ResearchDomain.ComputerScience,
            ["cybersecurity", "security", "privacy", "encryption"]),
        ("Data Mining", "Knowledge discovery and large-scale data analysis",
            ResearchDomain.ComputerScience,
            ["data mining", "big data", "analytics", "knowledge discovery"])
    ];

    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (await db.ResearchTopics.AnyAsync())
            return;

        var utcNow = DateTime.UtcNow;

        foreach (var (name, description, domain, keywords) in DefaultTopics)
        {
            var topic = new ResearchTopic
            {
                Id = Guid.NewGuid(),
                Name = name,
                Description = description,
                Domain = domain,
                PapersCount = 0,
                CreatedAt = utcNow,
                UpdatedAt = utcNow
            };

            foreach (var keywordName in keywords)
            {
                var keyword = await db.Keywords
                    .FirstOrDefaultAsync(k => k.Name.ToLower() == keywordName.ToLower());

                if (keyword is null)
                {
                    keyword = new Keyword
                    {
                        Id = Guid.NewGuid(),
                        Name = keywordName,
                        FrequencyCount = 0,
                        CreatedAt = utcNow,
                        UpdatedAt = utcNow
                    };
                    db.Keywords.Add(keyword);
                }

                topic.Keywords.Add(keyword);
            }

            db.ResearchTopics.Add(topic);
        }

        await db.SaveChangesAsync();
    }
}
