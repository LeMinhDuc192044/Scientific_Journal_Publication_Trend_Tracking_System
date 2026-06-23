using Scientific_Journal_Publication_Trend_Tracking_System.Domain.Entities;

namespace Scientific_Journal_Publication_Trend_Tracking_System.Infrastructure.Persistence.Repositories;

/// <summary>
/// Unit of Work pattern for managing transaction scope across repositories
/// </summary>
public interface IUnitOfWork : IAsyncDisposable
{
    IUserRepository Users { get; }
    IResearchPaperRepository ResearchPapers { get; }
    IRepository<Journal> Journals { get; }
    IRepository<Author> Authors { get; }
    IRepository<Keyword> Keywords { get; }
    IRepository<ResearchTopic> ResearchTopics { get; }
    IRepository<PublicationTrend> PublicationTrends { get; }
    IRepository<Bookmark> Bookmarks { get; }
    IRepository<Notification> Notifications { get; }
    IRepository<FollowSubscription> FollowSubscriptions { get; }
    IRepository<RefreshTokenHistory> RefreshTokenHistories { get; }
    IRepository<DashboardReport> DashboardReports { get; }
    IRepository<ApiDataSource> ApiDataSources { get; }
    IRepository<SyncJob> SyncJobs { get; }
    IRepository<SyncLog> SyncLogs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IUserRepository? _userRepository;
    private IResearchPaperRepository? _researchPaperRepository;
    private IRepository<Journal>? _journalRepository;
    private IRepository<Author>? _authorRepository;
    private IRepository<Keyword>? _keywordRepository;
    private IRepository<ResearchTopic>? _researchTopicRepository;
    private IRepository<PublicationTrend>? _publicationTrendRepository;
    private IRepository<Bookmark>? _bookmarkRepository;
    private IRepository<Notification>? _notificationRepository;
    private IRepository<FollowSubscription>? _followSubscriptionRepository;
    private IRepository<RefreshTokenHistory>? _refreshTokenHistoryRepository;
    private IRepository<DashboardReport>? _dashboardReportRepository;
    private IRepository<ApiDataSource>? _apiDataSourceRepository;
    private IRepository<SyncJob>? _syncJobRepository;
    private IRepository<SyncLog>? _syncLogRepository;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IUserRepository Users => _userRepository ??= new UserRepository(_context);
    public IResearchPaperRepository ResearchPapers => _researchPaperRepository ??= new ResearchPaperRepository(_context);
    public IRepository<Journal> Journals => _journalRepository ??= new Repository<Journal>(_context);
    public IRepository<Author> Authors => _authorRepository ??= new Repository<Author>(_context);
    public IRepository<Keyword> Keywords => _keywordRepository ??= new Repository<Keyword>(_context);
    public IRepository<ResearchTopic> ResearchTopics => _researchTopicRepository ??= new Repository<ResearchTopic>(_context);
    public IRepository<PublicationTrend> PublicationTrends => _publicationTrendRepository ??= new Repository<PublicationTrend>(_context);
    public IRepository<Bookmark> Bookmarks => _bookmarkRepository ??= new Repository<Bookmark>(_context);
    public IRepository<Notification> Notifications => _notificationRepository ??= new Repository<Notification>(_context);
    public IRepository<FollowSubscription> FollowSubscriptions => _followSubscriptionRepository ??= new Repository<FollowSubscription>(_context);
    public IRepository<RefreshTokenHistory> RefreshTokenHistories => _refreshTokenHistoryRepository ??= new Repository<RefreshTokenHistory>(_context);
    public IRepository<DashboardReport> DashboardReports => _dashboardReportRepository ??= new Repository<DashboardReport>(_context);
    public IRepository<ApiDataSource> ApiDataSources => _apiDataSourceRepository ??= new Repository<ApiDataSource>(_context);
    public IRepository<SyncJob> SyncJobs => _syncJobRepository ??= new Repository<SyncJob>(_context);
    public IRepository<SyncLog> SyncLogs => _syncLogRepository ??= new Repository<SyncLog>(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            await _context.Database.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await _context.Database.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        await _context.Database.RollbackTransactionAsync(cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await _context.DisposeAsync();
    }
}
