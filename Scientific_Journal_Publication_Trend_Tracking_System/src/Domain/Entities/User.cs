using System;
using Scientific_Journal_Publication_Trend_Tracking_System.Domain.Enums;

namespace Scientific_Journal_Publication_Trend_Tracking_System.Domain.Entities;

/// <summary>
/// Represents a system user with authentication and role-based access control
/// </summary>
public class User : IAuditableEntity
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.User;
    public bool IsEmailVerified { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<RefreshTokenHistory> RefreshTokenHistories { get; set; } = new List<RefreshTokenHistory>();
    public ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<FollowSubscription> FollowSubscriptions { get; set; } = new List<FollowSubscription>();
}

public interface IAuditableEntity
{
    DateTime CreatedAt { get; set; }
    DateTime UpdatedAt { get; set; }
}
