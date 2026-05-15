using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Scientific_Journal_Publication_Trend_Tracking_System.Domain.Entities;

namespace Scientific_Journal_Publication_Trend_Tracking_System.Infrastructure.Persistence.Configurations;

public class ResearchPaperConfiguration : IEntityTypeConfiguration<ResearchPaper>
{
    public void Configure(EntityTypeBuilder<ResearchPaper> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Title)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(p => p.Abstract)
            .HasMaxLength(4000);

        builder.Property(p => p.ExternalId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(p => p.ApiSource)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.Doi)
            .HasMaxLength(256);

        builder.Property(p => p.Keywords)
            .HasColumnType("text[]");

        builder.HasIndex(p => p.ExternalId);
        builder.HasIndex(p => p.Title);
        builder.HasIndex(p => p.PublicationYear);

        builder.HasOne(p => p.Journal)
            .WithMany(j => j.ResearchPapers)
            .HasForeignKey(p => p.JournalId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(p => p.Authors)
            .WithMany(a => a.ResearchPapers)
            .UsingEntity("PaperAuthors");

        builder.HasMany(p => p.ResearchKeywords)
            .WithMany(k => k.ResearchPapers)
            .UsingEntity("PaperKeywords");

        builder.HasMany(p => p.ResearchTopics)
            .WithMany(t => t.ResearchPapers)
            .UsingEntity("PaperTopics");

        builder.ToTable("ResearchPapers");
    }
}
