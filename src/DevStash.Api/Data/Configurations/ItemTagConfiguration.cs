using DevStash.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevStash.Api.Data.Configurations;

public sealed class ItemTagConfiguration : IEntityTypeConfiguration<ItemTag>
{
    public void Configure(EntityTypeBuilder<ItemTag> builder)
    {
        builder.ToTable("item_tags", DatabaseSchema.Name);

        builder.HasKey(itemTag => new { itemTag.ItemId, itemTag.TagId })
            .HasName("pk_item_tags");
        builder.Property(itemTag => itemTag.ItemId).HasColumnName("item_id");
        builder.Property(itemTag => itemTag.TagId).HasColumnName("tag_id");

        builder.HasOne(itemTag => itemTag.Item)
            .WithMany(item => item.ItemTags)
            .HasForeignKey(itemTag => itemTag.ItemId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_item_tags_items_item_id");
        builder.HasOne(itemTag => itemTag.Tag)
            .WithMany(tag => tag.ItemTags)
            .HasForeignKey(itemTag => itemTag.TagId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_item_tags_tags_tag_id");

        builder.HasIndex(itemTag => itemTag.TagId)
            .HasDatabaseName("ix_item_tags_tag_id");
    }
}
