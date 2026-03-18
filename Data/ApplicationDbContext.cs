using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SaasAsaasApp.Data.Entities;
using SaasAsaasApp.Data.Interfaces;

namespace SaasAsaasApp.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    private readonly ITenantProvider _tenantProvider;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ITenantProvider tenantProvider)
        : base(options)
    {
        _tenantProvider = tenantProvider;
    }

    public DbSet<Tenant> Tenants { get; set; } = null!;
    public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; } = null!;
    public DbSet<ProcessedWebhook> ProcessedWebhooks { get; set; } = null!;
    public DbSet<Customer> Customers { get; set; } = null!;
    public DbSet<Project> Projects { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply Global Filters for Soft Delete and Multi-Tenancy
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // Soft Delete Filter
            if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(CreateSoftDeleteFilter(entityType.ClrType));
            }

            // Tenant Isolation Filter
            if (typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(CreateTenantFilter(entityType.ClrType, this));
            }
        }

        // Unique index configurations
        modelBuilder.Entity<Tenant>()
            .HasIndex(t => t.Slug)
            .IsUnique();
            
        modelBuilder.Entity<SubscriptionPlan>()
            .HasIndex(p => p.InternalName)
            .IsUnique();

        modelBuilder.Entity<ProcessedWebhook>()
            .HasIndex(w => w.EventId)
            .IsUnique();
    }

    private static dynamic CreateSoftDeleteFilter(Type type)
    {
        // Conceptually generates: x => !x.IsDeleted
        var method = typeof(ApplicationDbContext)
            .GetMethod(nameof(GetSoftDeleteFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
            ?.MakeGenericMethod(type);
            
        return method?.Invoke(null, null)!;
    }

    private static System.Linq.Expressions.Expression<Func<T, bool>> GetSoftDeleteFilter<T>() where T : class, ISoftDeletable
    {
        return x => !x.IsDeleted;
    }

    private static dynamic CreateTenantFilter(Type type, ApplicationDbContext context)
    {
        var method = typeof(ApplicationDbContext)
            .GetMethod(nameof(GetTenantFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
            ?.MakeGenericMethod(type);
            
        return method?.Invoke(null, new object[] { context })!;
    }

    private static System.Linq.Expressions.Expression<Func<T, bool>> GetTenantFilter<T>(ApplicationDbContext context) where T : class, ITenantEntity
    {
        return x => x.TenantId == context._tenantProvider.TenantId;
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        UpdateTenantId();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        UpdateAuditFields();
        UpdateTenantId();
        return base.SaveChanges();
    }

    private void UpdateTenantId()
    {
        // Automatically inject TenantId on new entities
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is ITenantEntity && e.State == EntityState.Added);

        foreach (var entry in entries)
        {
            var entity = (ITenantEntity)entry.Entity;
            if (entity.TenantId == Guid.Empty)
            {
                entity.TenantId = _tenantProvider.TenantId;
            }
        }
    }

    private void UpdateAuditFields()
    {
        // Handle CreatedAt and UpdatedAt fields
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is IAuditableEntity && (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            var entity = (IAuditableEntity)entry.Entity;
            if (entry.State == EntityState.Added)
            {
                entity.CreatedAt = DateTime.UtcNow;
            }
            else
            {
                entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        // Automatic Soft Delete when calling context.Remove()
        var deleteEntries = ChangeTracker.Entries()
            .Where(e => e.Entity is ISoftDeletable && e.State == EntityState.Deleted);

        foreach (var entry in deleteEntries)
        {
            entry.State = EntityState.Modified;
            var entity = (ISoftDeletable)entry.Entity;
            entity.IsDeleted = true;
            entity.DeletedAt = DateTime.UtcNow;
        }
    }
}
