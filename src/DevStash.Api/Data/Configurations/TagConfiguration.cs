using DevStash.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevStash.Api.Data.Configurations;

public sealed class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.ToTable("tags", DatabaseSchema.Name);

        builder.HasKey(tag => tag.Id).HasName("pk_tags");
        builder.Property(tag => tag.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(tag => tag.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
        builder.Property(tag => tag.Slug).HasColumnName("slug").HasMaxLength(100).IsRequired();
        builder.Property(tag => tag.UserId).HasColumnName("user_id");
        builder.Property(tag => tag.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone");
        builder.Property(tag => tag.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp with time zone");

        builder.HasOne(tag => tag.User)
            .WithMany(user => user.Tags)
            .HasForeignKey(tag => tag.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_tags_users_user_id");

        builder.HasIndex(tag => new { tag.UserId, tag.Slug })
            .IsUnique()
            .HasDatabaseName("ux_tags_user_slug");
        builder.HasIndex(tag => new { tag.UserId, tag.UpdatedAt })
            .HasDatabaseName("ix_tags_user_updated_at");
    }
}
