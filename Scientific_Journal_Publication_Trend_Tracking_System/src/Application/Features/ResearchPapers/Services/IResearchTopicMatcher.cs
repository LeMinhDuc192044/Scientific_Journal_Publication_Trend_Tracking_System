using Scientific_Journal_Publication_Trend_Tracking_System.Domain.Entities;

namespace Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.ResearchPapers.Services;

public interface IResearchTopicMatcher
{
    IReadOnlyList<ResearchTopic> MatchTopics(
        ResearchPaper paper,
        IReadOnlyCollection<ResearchTopic> availableTopics,
        int maxTopics);
}
