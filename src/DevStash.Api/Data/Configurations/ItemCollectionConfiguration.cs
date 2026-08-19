using DevStash.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevStash.Api.Data.Configurations;

public sealed class ItemCollectionConfiguration : IEntityTypeConfiguration<ItemCollection>
{
    public void Configure(EntityTypeBuilder<ItemCollection> builder)
    {
        builder.ToTable("item_collections", DatabaseSchema.Name);

        builder.HasKey(itemCollection => new { itemCollection.ItemId, itemCollection.CollectionId })
            .HasName("pk_item_collections");
        builder.Property(itemCollection => itemCollection.ItemId).HasColumnName("item_id");
        builder.Property(itemCollection => itemCollection.CollectionId).HasColumnName("collection_id");
        builder.Property(itemCollection => itemCollection.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone");

        builder.HasOne(itemCollection => itemCollection.Item)
            .WithMany(item => item.ItemCollections)
            .HasForeignKey(itemCollection => itemCollection.ItemId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_item_collections_items_item_id");
        builder.HasOne(itemCollection => itemCollection.Collection)
            .WithMany(collection => collection.ItemCollections)
            .HasForeignKey(itemCollection => itemCollection.CollectionId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_item_collections_collections_collection_id");

        builder.HasIndex(itemCollection => itemCollection.CollectionId)
            .HasDatabaseName("ix_item_collections_collection_id");
    }
}
