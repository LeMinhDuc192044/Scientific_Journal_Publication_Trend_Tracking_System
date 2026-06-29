using System.Text.RegularExpressions;
using Scientific_Journal_Publication_Trend_Tracking_System.Domain.Entities;
using Scientific_Journal_Publication_Trend_Tracking_System.Domain.Enums;

namespace Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.ResearchPapers.Services;

public sealed class ResearchTopicMatcher : IResearchTopicMatcher
{
    private const int MinimumScore = 5;

    private static readonly Dictionary<string, string[]> TopicSignals = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Artificial Intelligence"] =
        [
            "artificial intelligence",
            "applications of artificial intelligence",
            "symbolic artificial intelligence",
            "cognitive computing",
            "cognitive robotics",
            "intelligent system",
            "expert system"
        ],
        ["Machine Learning"] =
        [
            "machine learning",
            "statistical learning",
            "supervised learning",
            "unsupervised learning",
            "reinforcement learning",
            "classification",
            "regression",
            "predictive analytics"
        ],
        ["Deep Learning"] =
        [
            "deep learning",
            "neural network",
            "neural networks",
            "convolutional neural network",
            "recurrent neural network",
            "transformer",
            "representation learning"
        ],
        ["Natural Language Processing"] =
        [
            "natural language processing",
            "nlp",
            "language model",
            "large language model",
            "text mining",
            "text classification",
            "information retrieval",
            "semantic analysis"
        ],
        ["Computer Vision"] =
        [
            "computer vision",
            "image recognition",
            "image processing",
            "medical imaging",
            "object detection",
            "visual recognition",
            "convolutional neural network",
            "radiology"
        ],
        ["Data Mining"] =
        [
            "data mining",
            "big data",
            "data science",
            "analytics",
            "knowledge discovery",
            "predictive analytics",
            "unstructured data"
        ],
        ["Cybersecurity"] =
        [
            "cybersecurity",
            "cyber security",
            "network security",
            "intrusion detection",
            "malware",
            "cryptography",
            "privacy",
            "authentication"
        ],
        ["Generative AI"] =
        [
            "generative ai",
            "generative artificial intelligence",
            "generative model",
            "large language model",
            "llm",
            "chatgpt",
            "diffusion model",
            "prompt engineering"
        ]
    };

    public IReadOnlyList<ResearchTopic> MatchTopics(
        ResearchPaper paper,
        IReadOnlyCollection<ResearchTopic> availableTopics,
        int maxTopics)
    {
        if (maxTopics <= 0 || availableTopics.Count == 0)
        {
            return [];
        }

        var title = Normalize(paper.Title);
        var abstractText = Normalize(paper.Abstract);
        var keywords = paper.Keywords
            .Where(k => !string.IsNullOrWhiteSpace(k))
            .Select(Normalize)
            .ToArray();

        var scoredTopics = availableTopics
            .Select(topic => new
            {
                Topic = topic,
                Score = ScoreTopic(topic, title, abstractText, keywords, paper.Domain)
            })
            .Where(x => x.Score >= MinimumScore)
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.Topic.Name)
            .Take(maxTopics)
            .Select(x => x.Topic)
            .ToList();

        return scoredTopics;
    }

    private static int ScoreTopic(
        ResearchTopic topic,
        string title,
        string abstractText,
        IReadOnlyCollection<string> keywords,
        ResearchDomain paperDomain)
    {
        var topicName = Normalize(topic.Name);
        var signals = TopicSignals.TryGetValue(topic.Name, out var configuredSignals)
            ? configuredSignals
            : [topic.Name];

        var normalizedSignals = signals
            .Append(topic.Name)
            .Select(Normalize)
            .Where(s => s.Length > 0)
            .Distinct()
            .ToArray();

        var score = 0;

        foreach (var signal in normalizedSignals)
        {
            if (keywords.Any(keyword => keyword == signal))
            {
                score += 10;
            }
            else if (keywords.Any(keyword => ContainsPhrase(keyword, signal)))
            {
                score += 8;
            }

            if (ContainsPhrase(title, signal))
            {
                score += signal == topicName ? 8 : 6;
            }

            if (ContainsPhrase(abstractText, signal))
            {
                score += 2;
            }
        }

        if (topic.Domain == paperDomain && score > 0)
        {
            score += 1;
        }

        return score;
    }

    private static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var lower = value.Trim().ToLowerInvariant();
        var normalized = Regex.Replace(lower, @"[^a-z0-9]+", " ");
        return Regex.Replace(normalized, @"\s+", " ").Trim();
    }

    private static bool ContainsPhrase(string text, string phrase)
    {
        if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(phrase))
        {
            return false;
        }

        return text == phrase ||
               text.StartsWith($"{phrase} ", StringComparison.Ordinal) ||
               text.EndsWith($" {phrase}", StringComparison.Ordinal) ||
               text.Contains($" {phrase} ", StringComparison.Ordinal);
    }
}
