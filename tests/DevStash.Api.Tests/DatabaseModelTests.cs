using DevStash.Api.Data;
using DevStash.Api.Data.Identity;
using DevStash.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace DevStash.Api.Tests;

public sealed class DatabaseModelTests
{
    private readonly IModel _model = CreateModel();

    [Fact]
    public void Model_AllTablesUseTheIsolatedSchema()
    {
        var mappedTables = _model.GetEntityTypes()
            .Where(entityType => entityType.GetTableName() is not null)
            .ToArray();

        Assert.NotEmpty(mappedTables);
        Assert.All(
            mappedTables,
            entityType => Assert.Equal(DatabaseSchema.Name, entityType.GetSchema()));
    }

    [Fact]
    public void JoinEntities_UseTheRequiredCompositeKeys()
    {
        Assert.Equal(
            [nameof(ItemCollection.ItemId), nameof(ItemCollection.CollectionId)],
            PrimaryKeyProperties<ItemCollection>());
        Assert.Equal(
            [nameof(ItemTag.ItemId), nameof(ItemTag.TagId)],
            PrimaryKeyProperties<ItemTag>());
    }

    [Fact]
    public void OwnerScopedSlugs_HaveUniqueIndexes()
    {
        AssertUniqueIndex<Collection>(nameof(Collection.UserId), nameof(Collection.Slug));
        AssertUniqueIndex<Tag>(nameof(Tag.UserId), nameof(Tag.Slug));

        var customTypeIndex = AssertUniqueIndex<ItemType>(nameof(ItemType.UserId), nameof(ItemType.Slug));
        Assert.Equal("\"user_id\" IS NOT NULL", customTypeIndex.GetFilter());

        var systemTypeIndex = AssertUniqueIndex<ItemType>(nameof(ItemType.Slug));
        Assert.Equal("\"is_system\" = TRUE", systemTypeIndex.GetFilter());
    }

    [Fact]
    public void OwnerQueries_HaveUpdatedTimestampIndexes()
    {
        AssertIndex<Item>(nameof(Item.UserId), nameof(Item.UpdatedAt));
        AssertIndex<Collection>(nameof(Collection.UserId), nameof(Collection.UpdatedAt));
        AssertIndex<Tag>(nameof(Tag.UserId), nameof(Tag.UpdatedAt));
        AssertIndex<ItemType>(nameof(ItemType.UserId), nameof(ItemType.UpdatedAt));
    }

    [Fact]
    public void ItemTypes_EnforceTheSystemOwnerInvariant()
    {
        var constraint = EntityType<ItemType>().FindCheckConstraint("ck_item_types_owner");

        Assert.NotNull(constraint);
        Assert.Equal(
            "(is_system = TRUE AND user_id IS NULL) OR (is_system = FALSE AND user_id IS NOT NULL)",
            constraint.Sql);
    }

    [Fact]
    public void Relationships_UseTheRequiredDeleteBehaviors()
    {
        AssertDeleteBehavior<Item, ApplicationUser>(DeleteBehavior.Cascade, nameof(Item.UserId));
        AssertDeleteBehavior<Item, ItemType>(DeleteBehavior.Restrict, nameof(Item.ItemTypeId));
        AssertDeleteBehavior<Collection, ApplicationUser>(DeleteBehavior.Cascade, nameof(Collection.UserId));
        AssertDeleteBehavior<Tag, ApplicationUser>(DeleteBehavior.Cascade, nameof(Tag.UserId));
        AssertDeleteBehavior<ItemType, ApplicationUser>(DeleteBehavior.Cascade, nameof(ItemType.UserId));
        AssertDeleteBehavior<ItemCollection, Item>(DeleteBehavior.Cascade, nameof(ItemCollection.ItemId));
        AssertDeleteBehavior<ItemCollection, Collection>(DeleteBehavior.Cascade, nameof(ItemCollection.CollectionId));
        AssertDeleteBehavior<ItemTag, Item>(DeleteBehavior.Cascade, nameof(ItemTag.ItemId));
        AssertDeleteBehavior<ItemTag, Tag>(DeleteBehavior.Cascade, nameof(ItemTag.TagId));
    }

    [Fact]
    public void ItemTypes_SeedExactlySevenSystemTypes()
    {
        var itemType = EntityType<ItemType>();
        var seeds = itemType.GetSeedData().ToArray();

        Assert.Equal(7, seeds.Length);
        Assert.All(seeds, seed => Assert.True(Assert.IsType<bool>(seed[nameof(ItemType.IsSystem)])));
        Assert.All(seeds, seed => Assert.Null(seed[nameof(ItemType.UserId)]));
        Assert.Equal(
            ["command", "file", "image", "link", "note", "prompt", "snippet"],
            seeds.Select(seed => Assert.IsType<string>(seed[nameof(ItemType.Slug)]))
                .Order()
                .ToArray());
    }

    [Fact]
    public void DomainIds_AreGeneratedByTheApplication()
    {
        Assert.NotEqual(Guid.Empty, new ApplicationUser().Id);
        Assert.NotEqual(Guid.Empty, new Item().Id);
        Assert.NotEqual(Guid.Empty, new Collection().Id);
        Assert.NotEqual(Guid.Empty, new Tag().Id);
        Assert.NotEqual(Guid.Empty, new ItemType().Id);

        Assert.Equal(
            ValueGenerated.Never,
            EntityType<ApplicationUser>().FindProperty(nameof(ApplicationUser.Id))!.ValueGenerated);
        Assert.Equal(ValueGenerated.Never, EntityType<Item>().FindProperty(nameof(Item.Id))!.ValueGenerated);
        Assert.Equal(ValueGenerated.Never, EntityType<Collection>().FindProperty(nameof(Collection.Id))!.ValueGenerated);
        Assert.Equal(ValueGenerated.Never, EntityType<Tag>().FindProperty(nameof(Tag.Id))!.ValueGenerated);
        Assert.Equal(ValueGenerated.Never, EntityType<ItemType>().FindProperty(nameof(ItemType.Id))!.ValueGenerated);
    }

    [Fact]
    public void ConnectionOptions_ConvertAPostgreSqlUriForNpgsql()
    {
        var normalized = DevStashDbContextOptions.NormalizeConnectionString(
            "postgresql://devstash_user:p%40ssword@development.example.test:5433/devstash?sslmode=require&channel_binding=require");
        var parsed = new NpgsqlConnectionStringBuilder(normalized);

        Assert.Equal("development.example.test", parsed.Host);
        Assert.Equal(5433, parsed.Port);
        Assert.Equal("devstash", parsed.Database);
        Assert.Equal("devstash_user", parsed.Username);
        Assert.Equal("p@ssword", parsed.Password);
        Assert.Equal(SslMode.Require, parsed.SslMode);
        Assert.Equal(ChannelBinding.Require, parsed.ChannelBinding);
    }

    [Fact]
    public void ConnectionOptions_EnableBoundedTransientRetries()
    {
        using var context = CreateContext();

        var strategy = Assert.IsType<NpgsqlRetryingExecutionStrategy>(
            context.Database.CreateExecutionStrategy());

        Assert.True(strategy.RetriesOnFailure);
        Assert.Equal(5, strategy.MaxRetryCount);
        Assert.Equal(TimeSpan.FromSeconds(10), strategy.MaxRetryDelay);
    }

    private static IModel CreateModel()
    {
        using var context = CreateContext();
        return context.GetService<IDesignTimeModel>().Model;
    }

    private static DevStashDbContext CreateContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<DevStashDbContext>();
        DevStashDbContextOptions.Configure(
            optionsBuilder,
            "Host=localhost;Database=devstash_model_tests;Username=devstash");

        return new DevStashDbContext(optionsBuilder.Options);
    }

    private IEntityType EntityType<TEntity>() =>
        _model.FindEntityType(typeof(TEntity))
        ?? throw new InvalidOperationException($"{typeof(TEntity).Name} is not part of the EF model.");

    private string[] PrimaryKeyProperties<TEntity>() =>
        EntityType<TEntity>().FindPrimaryKey()!.Properties.Select(property => property.Name).ToArray();

    private IIndex AssertUniqueIndex<TEntity>(params string[] propertyNames)
    {
        var index = AssertIndex<TEntity>(propertyNames);

        Assert.True(index.IsUnique);
        return index;
    }

    private IIndex AssertIndex<TEntity>(params string[] propertyNames) =>
        EntityType<TEntity>().GetIndexes().Single(candidate =>
            candidate.Properties.Select(property => property.Name).SequenceEqual(propertyNames));

    private void AssertDeleteBehavior<TDependent, TPrincipal>(
        DeleteBehavior expected,
        params string[] foreignKeyProperties)
    {
        var foreignKey = EntityType<TDependent>().GetForeignKeys().Single(candidate =>
            candidate.PrincipalEntityType.ClrType == typeof(TPrincipal)
            && candidate.Properties.Select(property => property.Name).SequenceEqual(foreignKeyProperties));

        Assert.Equal(expected, foreignKey.DeleteBehavior);
    }
}
