using Microsoft.EntityFrameworkCore;
using Atlas.Infrastructure.Data.Entities;

namespace Atlas.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<ApplicationEntity> Applications => Set<ApplicationEntity>();
    public DbSet<DocumentEntity> Documents => Set<DocumentEntity>();
    public DbSet<IdentityVerificationEntity> IdentityVerifications => Set<IdentityVerificationEntity>();
    public DbSet<SanctionsScreeningEntity> SanctionsScreenings => Set<SanctionsScreeningEntity>();
    public DbSet<SanctionMatchEntity> SanctionMatches => Set<SanctionMatchEntity>();
    public DbSet<AccountDetailsEntity> AccountDetails => Set<AccountDetailsEntity>();
    public DbSet<CardOrderEntity> CardOrders => Set<CardOrderEntity>();
    public DbSet<ApplicationDecisionEntity> ApplicationDecisions => Set<ApplicationDecisionEntity>();
    public DbSet<RejectionReasonEntity> RejectionReasons => Set<RejectionReasonEntity>();
    public DbSet<AuditLogEntity> AuditLogs => Set<AuditLogEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply entity configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
