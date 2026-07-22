using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTrack.Modules.Budgets.Entities;

public class BudgetEntity
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string Category { get; init; } = string.Empty;
    public decimal AmountLimit { get; init; }
    public decimal AmountSpent { get; internal set; }
    public bool AlertSent { get; internal set; }
    public int Month { get; init; }
    public int Year { get; init; }
    public DateTime CreatedAt { get; init; }
}

public class BudgetEntityConfiguration : IEntityTypeConfiguration<BudgetEntity>
{
    public void Configure(EntityTypeBuilder<BudgetEntity> builder)
    {
        builder.ToTable("budgets");
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Category)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(b => b.AmountLimit)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(b => b.AmountSpent)
            .IsRequired()
            .HasPrecision(18, 2);
        
        builder.HasIndex(b => new { b.UserId, b.Category, b.Month, b.Year })
            .IsUnique()
            .HasDatabaseName("idx_budgets_user_category_month_year");

        builder.HasIndex(b => new { b.UserId, b.Month, b.Year })
            .HasDatabaseName("idx_budgets_user_month_year");
    }
}