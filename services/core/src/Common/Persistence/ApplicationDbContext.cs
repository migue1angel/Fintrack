using FinTrack.Common.Outbox;
using FinTrack.Modules.Budgets.Entities;
using FinTrack.Modules.Goals.Entities;
using FinTrack.Modules.Transactions.Entities;
using FinTrack.Modules.Users.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Common.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<UserEntity> Users => Set<UserEntity>();
    public DbSet<TransactionEntity> Transactions => Set<TransactionEntity>();
    public DbSet<GoalEntity> Goals => Set<GoalEntity>();
    public DbSet<BudgetEntity> Budgets => Set<BudgetEntity>();
    public DbSet<OutboxEventEntity> OutboxEvents => Set<OutboxEventEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}