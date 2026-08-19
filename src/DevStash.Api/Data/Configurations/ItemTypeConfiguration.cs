using DevStash.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevStash.Api.Data.Configurations;

public sealed class ItemTypeConfiguration : IEntityTypeConfiguration<ItemType>
{
    private static readonly DateTimeOffset SeededAt = new(2026, 8, 19, 0, 0, 0, TimeSpan.Zero);

    public void Configure(EntityTypeBuilder<ItemType> builder)
    {
        builder.ToTable(
            "item_types",
            DatabaseSchema.Name,
            table => table.HasCheckConstraint(
                "ck_item_types_owner",
                "(is_system = TRUE AND user_id IS NULL) OR (is_system = FALSE AND user_id IS NOT NULL)"));

        builder.HasKey(itemType => itemType.Id).HasName("pk_item_types");
        builder.Property(itemType => itemType.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(itemType => itemType.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
        builder.Property(itemType => itemType.Slug).HasColumnName("slug").HasMaxLength(100).IsRequired();
        builder.Property(itemType => itemType.Icon).HasColumnName("icon").HasMaxLength(100);
        builder.Property(itemType => itemType.Color).HasColumnName("color").HasMaxLength(32);
        builder.Property(itemType => itemType.IsSystem).HasColumnName("is_system");
        builder.Property(itemType => itemType.UserId).HasColumnName("user_id");
        builder.Property(itemType => itemType.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone");
        builder.Property(itemType => itemType.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp with time zone");

        builder.HasOne(itemType => itemType.User)
            .WithMany(user => user.ItemTypes)
            .HasForeignKey(itemType => itemType.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_item_types_users_user_id");

        builder.HasIndex(itemType => itemType.Slug)
            .IsUnique()
            .HasFilter("\"is_system\" = TRUE")
            .HasDatabaseName("ux_item_types_system_slug");
        builder.HasIndex(itemType => new { itemType.UserId, itemType.Slug })
            .IsUnique()
            .HasFilter("\"user_id\" IS NOT NULL")
            .HasDatabaseName("ux_item_types_user_slug");
        builder.HasIndex(itemType => new { itemType.UserId, itemType.UpdatedAt })
            .HasDatabaseName("ix_item_types_user_updated_at");

        builder.HasData(
            SystemType(1, "Snippet", "snippet", "code"),
            SystemType(2, "Prompt", "prompt", "sparkles"),
            SystemType(3, "Note", "note", "note"),
            SystemType(4, "Command", "command", "terminal"),
            SystemType(5, "File", "file", "file"),
            SystemType(6, "Image", "image", "image"),
            SystemType(7, "Link", "link", "link"));
    }

    private static ItemType SystemType(int number, string name, string slug, string icon) => new()
    {
        Id = Guid.Parse($"d0000000-0000-0000-0000-{number:000000000000}"),
        Name = name,
        Slug = slug,
        Icon = icon,
        IsSystem = true,
        CreatedAt = SeededAt,
        UpdatedAt = SeededAt
    };
}
