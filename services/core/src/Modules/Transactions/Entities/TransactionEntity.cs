using FinTrack.Common.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTrack.Modules.Transactions.Entities;

public class TransactionEntity
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public decimal Amount { get; init; }
    public TransactionType Type { get; init; }
    public string Category { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DateOnly TransactionDate { get; init; }
    public string? IdempotencyKey { get; init; }
    public DateTime CreatedAt { get; init; }
}

public class TransactionEntityConfiguration : IEntityTypeConfiguration<TransactionEntity>
{
    public void Configure(EntityTypeBuilder<TransactionEntity> builder)
    {
        builder.ToTable("transactions");
        builder.HasKey(t => t.Id);
        
        builder.Property(t => t.Amount)
            .IsRequired()
            .HasPrecision(18,2);

        builder.Property(t => t.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(10);
        
        builder.Property(t => t.Category)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.Description)
            .HasMaxLength(500);

        builder.Property(t => t.IdempotencyKey)
            .HasMaxLength(256);
        
        builder.HasIndex(t => t.IdempotencyKey)
            .IsUnique()
            .HasFilter("idempotency_key IS NOT NULL")
            .HasDatabaseName("idx_transactions_idempotency_key");;
        
        builder.HasIndex(t => new {t.UserId, t.TransactionDate})
            .HasDatabaseName("idx_transactions_user_date");

        builder.HasIndex(t => new {t.UserId, t.Category, t.TransactionDate})
            .HasDatabaseName("idx_transactions_user_category_date");

    }
}