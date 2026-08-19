using DevStash.Api.Data.Identity;
using DevStash.Api.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DevStash.Api.Data;

public sealed class DevStashDbContext(
    DbContextOptions<DevStashDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    public const string Schema = DatabaseSchema.Name;
    public const string MigrationsHistoryTable = DatabaseSchema.MigrationsHistoryTable;

    public DbSet<ItemType> ItemTypes => Set<ItemType>();
    public DbSet<Item> Items => Set<Item>();
    public DbSet<Collection> Collections => Set<Collection>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<ItemCollection> ItemCollections => Set<ItemCollection>();
    public DbSet<ItemTag> ItemTags => Set<ItemTag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(Schema);
        ConfigureIdentityTables(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DevStashDbContext).Assembly);
    }

    private static void ConfigureIdentityTables(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IdentityRole<Guid>>(builder =>
        {
            builder.ToTable("identity_roles", Schema);
            builder.Property(role => role.Id).HasColumnName("id").ValueGeneratedNever();
            builder.Property(role => role.Name).HasColumnName("name").HasMaxLength(256);
            builder.Property(role => role.NormalizedName).HasColumnName("normalized_name").HasMaxLength(256);
            builder.Property(role => role.ConcurrencyStamp).HasColumnName("concurrency_stamp").IsConcurrencyToken();
            builder.HasIndex(role => role.NormalizedName)
                .IsUnique()
                .HasDatabaseName("ux_identity_roles_normalized_name");
        });

        modelBuilder.Entity<IdentityRoleClaim<Guid>>(builder =>
        {
            builder.ToTable("identity_role_claims", Schema);
            builder.Property(claim => claim.Id).HasColumnName("id");
            builder.Property(claim => claim.RoleId).HasColumnName("role_id");
            builder.Property(claim => claim.ClaimType).HasColumnName("claim_type");
            builder.Property(claim => claim.ClaimValue).HasColumnName("claim_value");
        });

        modelBuilder.Entity<IdentityUserClaim<Guid>>(builder =>
        {
            builder.ToTable("identity_user_claims", Schema);
            builder.Property(claim => claim.Id).HasColumnName("id");
            builder.Property(claim => claim.UserId).HasColumnName("user_id");
            builder.Property(claim => claim.ClaimType).HasColumnName("claim_type");
            builder.Property(claim => claim.ClaimValue).HasColumnName("claim_value");
        });

        modelBuilder.Entity<IdentityUserLogin<Guid>>(builder =>
        {
            builder.ToTable("identity_user_logins", Schema);
            builder.Property(login => login.LoginProvider).HasColumnName("login_provider");
            builder.Property(login => login.ProviderKey).HasColumnName("provider_key");
            builder.Property(login => login.ProviderDisplayName).HasColumnName("provider_display_name");
            builder.Property(login => login.UserId).HasColumnName("user_id");
        });

        modelBuilder.Entity<IdentityUserRole<Guid>>(builder =>
        {
            builder.ToTable("identity_user_roles", Schema);
            builder.Property(userRole => userRole.UserId).HasColumnName("user_id");
            builder.Property(userRole => userRole.RoleId).HasColumnName("role_id");
        });

        modelBuilder.Entity<IdentityUserToken<Guid>>(builder =>
        {
            builder.ToTable("identity_user_tokens", Schema);
            builder.Property(token => token.UserId).HasColumnName("user_id");
            builder.Property(token => token.LoginProvider).HasColumnName("login_provider");
            builder.Property(token => token.Name).HasColumnName("name");
            builder.Property(token => token.Value).HasColumnName("value");
        });
    }
}
