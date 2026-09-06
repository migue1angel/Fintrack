using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTrack.Common.Outbox;

public class OutboxEventEntity
{
    public Guid Id { get; init; }
    public required string EventType { get; init; }
    public required string Payload { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ProcessedAt { get; set; }
    public string? Error { get; set; }
}

public class OutboxEventEntityConfiguration : IEntityTypeConfiguration<OutboxEventEntity>
{
    public void Configure(EntityTypeBuilder<OutboxEventEntity> builder)
    {
        builder.ToTable("outbox_events");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.EventType)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Payload)
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.Error)
            .HasMaxLength(1000);

        builder.HasIndex(e => new {e.ProcessedAt, e.CreatedAt})
            .HasDatabaseName("idx_outbox_events_pending");
    }
}