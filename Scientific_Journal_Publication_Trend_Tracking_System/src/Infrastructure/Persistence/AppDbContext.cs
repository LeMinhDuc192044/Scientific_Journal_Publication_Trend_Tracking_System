using Microsoft.EntityFrameworkCore;
using Scientific_Journal_Publication_Trend_Tracking_System.Domain.Entities;
using Scientific_Journal_Publication_Trend_Tracking_System.Infrastructure.Persistence.Configurations;

namespace Scientific_Journal_Publication_Trend_Tracking_System.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core database context for Journal Trend Tracker system
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // DbSets
    public DbSet<User> Users { get; set; }
    public DbSet<RefreshTokenHistory> RefreshTokenHistories { get; set; }
    public DbSet<ResearchPaper> ResearchPapers { get; set; }
    public DbSet<Journal> Journals { get; set; }
    public DbSet<Author> Authors { get; set; }
    public DbSet<Keyword> Keywords { get; set; }
    public DbSet<ResearchTopic> ResearchTopics { get; set; }
    public DbSet<PublicationTrend> PublicationTrends { get; set; }
    public DbSet<Bookmark> Bookmarks { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<DashboardReport> DashboardReports { get; set; }
    public DbSet<ApiDataSource> ApiDataSources { get; set; }
    public DbSet<SyncJob> SyncJobs { get; set; }
    public DbSet<SyncLog> SyncLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Global query filters for soft deletes
        modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
        modelBuilder.Entity<Bookmark>().HasQueryFilter(b => !b.IsDeleted);
        modelBuilder.Entity<Notification>().HasQueryFilter(n => !n.IsDeleted);

        // Set default schema

    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditableEntities();
        return await base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        UpdateAuditableEntities();
        return base.SaveChanges();
    }

    private void UpdateAuditableEntities()
    {
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.Entity is IAuditableEntity);

        var utcNow = DateTime.UtcNow;

        foreach (var entry in entries)
        {
            if (entry.Entity is not IAuditableEntity entity) continue;

            if (entry.State == EntityState.Added)
            {
                entity.CreatedAt = utcNow;
            }

            entity.UpdatedAt = utcNow;
        }
    }
}
