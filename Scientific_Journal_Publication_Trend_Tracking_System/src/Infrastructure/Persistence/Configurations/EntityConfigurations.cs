using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Scientific_Journal_Publication_Trend_Tracking_System.Domain.Entities;

namespace Scientific_Journal_Publication_Trend_Tracking_System.Infrastructure.Persistence.Configurations;

public class JournalConfiguration : IEntityTypeConfiguration<Journal>
{
    public void Configure(EntityTypeBuilder<Journal> builder)
    {
        builder.HasKey(j => j.Id);
        builder.Property(j => j.Title).IsRequired().HasMaxLength(256);
        builder.Property(j => j.Issn).HasMaxLength(50);
        builder.Property(j => j.Publisher).HasMaxLength(256);
        builder.HasIndex(j => j.Title);
        builder.ToTable("Journals");
    }
}

public class AuthorConfiguration : IEntityTypeConfiguration<Author>
{
    public void Configure(EntityTypeBuilder<Author> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.FullName).IsRequired().HasMaxLength(256);
        builder.Property(a => a.Email).HasMaxLength(256);
        builder.Property(a => a.Orcid).HasMaxLength(50);
        builder.HasIndex(a => a.FullName);
        builder.ToTable("Authors");
    }
}

public class KeywordConfiguration : IEntityTypeConfiguration<Keyword>
{
    public void Configure(EntityTypeBuilder<Keyword> builder)
    {
        builder.HasKey(k => k.Id);
        builder.Property(k => k.Name).IsRequired().HasMaxLength(200);
        builder.HasIndex(k => k.Name).IsUnique();
        builder.ToTable("Keywords");
    }
}

public class ResearchTopicConfiguration : IEntityTypeConfiguration<ResearchTopic>
{
    public void Configure(EntityTypeBuilder<ResearchTopic> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Name).IsRequired().HasMaxLength(256);
        builder.Property(t => t.Description).HasMaxLength(1000);
        builder.HasIndex(t => t.Name);
        builder.ToTable("ResearchTopics");
    }
}

public class PublicationTrendConfiguration : IEntityTypeConfiguration<PublicationTrend>
{
    public void Configure(EntityTypeBuilder<PublicationTrend> builder)
    {
        builder.HasKey(t => t.Id);
        builder.HasOne(t => t.ResearchTopic)
            .WithMany(rt => rt.PublicationTrends)
            .HasForeignKey(t => t.ResearchTopicId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(t => t.Keyword)
            .WithMany() 
            .HasForeignKey(t => t.KeywordId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(t => t.Journal)
            .WithMany(j => j.PublicationTrends)
            .HasForeignKey(t => t.JournalId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.HasIndex(t => new { t.Year, t.ResearchTopicId });
        builder.ToTable("PublicationTrends");
    }
}

public class BookmarkConfiguration : IEntityTypeConfiguration<Bookmark>
{
    public void Configure(EntityTypeBuilder<Bookmark> builder)
    {
        builder.HasKey(b => b.Id);
        builder.HasOne(b => b.ResearchPaper)
            .WithMany(p => p.Bookmarks)
            .HasForeignKey(b => b.ResearchPaperId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(b => b.Journal)
            .WithMany(j => j.Bookmarks)
            .HasForeignKey(b => b.JournalId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(b => b.ResearchTopic)
            .WithMany()
            .OnDelete(DeleteBehavior.SetNull);
        builder.ToTable("Bookmarks");
    }
}

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Title).IsRequired().HasMaxLength(256);
        builder.Property(n => n.Message).IsRequired().HasMaxLength(1000);
        builder.HasIndex(n => n.UserId);
        builder.HasIndex(n => new { n.UserId, n.IsRead });
        builder.ToTable("Notifications");
    }
}

public class RefreshTokenHistoryConfiguration : IEntityTypeConfiguration<RefreshTokenHistory>
{
    public void Configure(EntityTypeBuilder<RefreshTokenHistory> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Token).IsRequired();
        builder.HasIndex(r => r.UserId);
        builder.ToTable("RefreshTokenHistories");
    }
}

public class DashboardReportConfiguration : IEntityTypeConfiguration<DashboardReport>
{
    public void Configure(EntityTypeBuilder<DashboardReport> builder)
    {
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Title).IsRequired().HasMaxLength(256);
        builder.ToTable("DashboardReports");
    }
}

public class ApiDataSourceConfiguration : IEntityTypeConfiguration<ApiDataSource>
{
    public void Configure(EntityTypeBuilder<ApiDataSource> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Name).IsRequired().HasMaxLength(256);
        builder.Property(a => a.BaseUrl).IsRequired().HasMaxLength(500);
        builder.HasMany(a => a.SyncLogs)
            .WithOne(s => s.ApiDataSource)
            .HasForeignKey(s => s.ApiDataSourceId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(a => a.SyncJobs)
            .WithOne(s => s.ApiDataSource)
            .HasForeignKey(s => s.ApiDataSourceId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.ToTable("ApiDataSources");
    }
}

public class SyncJobConfiguration : IEntityTypeConfiguration<SyncJob>
{
    public void Configure(EntityTypeBuilder<SyncJob> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.JobName).IsRequired().HasMaxLength(256);
        builder.HasMany(s => s.SyncLogs)
            .WithOne(l => l.SyncJob)
            .HasForeignKey(l => l.SyncJobId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(s => s.IsActive);
        builder.ToTable("SyncJobs");
    }
}

public class SyncLogConfiguration : IEntityTypeConfiguration<SyncLog>
{
    public void Configure(EntityTypeBuilder<SyncLog> builder)
    {
        builder.HasKey(s => s.Id);
        builder.HasIndex(s => s.SyncJobId);
        builder.HasIndex(s => s.ExecutionStartTime);
        builder.ToTable("SyncLogs");
    }
}
