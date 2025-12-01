using EcoRide.Modules.Security.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoRide.Modules.Security.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for UserSettings entity
/// </summary>
internal sealed class UserSettingsConfiguration : IEntityTypeConfiguration<UserSettings>
{
    public void Configure(EntityTypeBuilder<UserSettings> builder)
    {
        builder.ToTable("user_settings", "security");

        builder.HasKey(us => us.Id);

        builder.Property(us => us.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(us => us.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(us => us.PushNotificationsEnabled)
            .HasColumnName("push_notifications_enabled")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(us => us.DarkModeEnabled)
            .HasColumnName("dark_mode_enabled")
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(us => us.HapticFeedbackEnabled)
            .HasColumnName("haptic_feedback_enabled")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(us => us.LanguageCode)
            .HasColumnName("language_code")
            .HasMaxLength(5)
            .IsRequired()
            .HasDefaultValue("en");

        builder.Property(us => us.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(us => us.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        // Indexes
        builder.HasIndex(us => us.UserId)
            .IsUnique()
            .HasDatabaseName("idx_user_settings_user_id");
    }
}
