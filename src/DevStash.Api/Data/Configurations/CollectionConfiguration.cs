using DevStash.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevStash.Api.Data.Configurations;

public sealed class CollectionConfiguration : IEntityTypeConfiguration<Collection>
{
    public void Configure(EntityTypeBuilder<Collection> builder)
    {
        builder.ToTable("collections", DatabaseSchema.Name);

        builder.HasKey(collection => collection.Id).HasName("pk_collections");
        builder.Property(collection => collection.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(collection => collection.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
        builder.Property(collection => collection.Slug).HasColumnName("slug").HasMaxLength(100).IsRequired();
        builder.Property(collection => collection.Description).HasColumnName("description").HasMaxLength(2000);
        builder.Property(collection => collection.IsFavorite).HasColumnName("is_favorite");
        builder.Property(collection => collection.UserId).HasColumnName("user_id");
        builder.Property(collection => collection.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone");
        builder.Property(collection => collection.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp with time zone");

        builder.HasOne(collection => collection.User)
            .WithMany(user => user.Collections)
            .HasForeignKey(collection => collection.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_collections_users_user_id");

        builder.HasIndex(collection => new { collection.UserId, collection.Slug })
            .IsUnique()
            .HasDatabaseName("ux_collections_user_slug");
        builder.HasIndex(collection => new { collection.UserId, collection.UpdatedAt })
            .HasDatabaseName("ix_collections_user_updated_at");
    }
}
