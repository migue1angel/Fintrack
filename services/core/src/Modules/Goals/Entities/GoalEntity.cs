using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTrack.Modules.Goals.Entities;

public enum GoalStatus { Active = 0, Completed = 1, Cancelled = 2 }

public class GoalEntity
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal TargetAmount { get; init; }
    
    public decimal CurrentAmount { get; internal set; }

    public GoalStatus Status { get; internal set; } = GoalStatus.Active;
    public DateTime CreatedAt { get; init; }
    public DateTime? CompletedAt { get; internal set; }
    public DateTime? CancelledAt { get; internal set; }
    public DateOnly? TargetDate { get; init; }
}

public class GoalEntityConfiguration : IEntityTypeConfiguration<GoalEntity>
{
    public void Configure(EntityTypeBuilder<GoalEntity> builder)
    {
        builder.ToTable("goals");
        builder.HasKey(g => g.Id);

        builder.Property(g => g.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(g => g.Description)
            .HasMaxLength(500);

        builder.Property(g => g.TargetAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(g => g.CurrentAmount)
            .IsRequired()
            .HasPrecision(18, 2);
        
        builder.Property(g => g.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);
        
        builder.HasIndex(g => new { g.UserId, g.Status })
            .HasDatabaseName("idx_goals_user_status");
        
        builder.HasIndex(g => new { g.UserId, g.TargetDate })
            .HasFilter("target_date IS NOT NULL")
            .HasDatabaseName("idx_goals_user_target_date");
    }
}