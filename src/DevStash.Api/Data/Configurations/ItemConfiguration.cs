using DevStash.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevStash.Api.Data.Configurations;

public sealed class ItemConfiguration : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> builder)
    {
        builder.ToTable("items", DatabaseSchema.Name);

        builder.HasKey(item => item.Id).HasName("pk_items");
        builder.Property(item => item.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(item => item.Title).HasColumnName("title").HasMaxLength(200).IsRequired();
        builder.Property(item => item.Description).HasColumnName("description").HasMaxLength(2000);
        builder.Property(item => item.ContentType).HasColumnName("content_type").HasMaxLength(100).IsRequired();
        builder.Property(item => item.Content).HasColumnName("content");
        builder.Property(item => item.Language).HasColumnName("language").HasMaxLength(100);
        builder.Property(item => item.Url).HasColumnName("url").HasMaxLength(2048);
        builder.Property(item => item.IsFavorite).HasColumnName("is_favorite");
        builder.Property(item => item.IsPinned).HasColumnName("is_pinned");
        builder.Property(item => item.LastUsedAt).HasColumnName("last_used_at").HasColumnType("timestamp with time zone");
        builder.Property(item => item.UserId).HasColumnName("user_id");
        builder.Property(item => item.ItemTypeId).HasColumnName("item_type_id");
        builder.Property(item => item.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone");
        builder.Property(item => item.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp with time zone");

        builder.HasOne(item => item.User)
            .WithMany(user => user.Items)
            .HasForeignKey(item => item.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_items_users_user_id");
        builder.HasOne(item => item.ItemType)
            .WithMany(itemType => itemType.Items)
            .HasForeignKey(item => item.ItemTypeId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_items_item_types_item_type_id");

        builder.HasIndex(item => new { item.UserId, item.UpdatedAt })
            .HasDatabaseName("ix_items_user_updated_at");
        builder.HasIndex(item => item.ItemTypeId)
            .HasDatabaseName("ix_items_item_type_id");
    }
}
