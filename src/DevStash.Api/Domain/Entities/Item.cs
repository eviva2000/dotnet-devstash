using DevStash.Api.Data.Identity;

namespace DevStash.Api.Domain.Entities;

public sealed class Item
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string? Language { get; set; }
    public string? Url { get; set; }
    public bool IsFavorite { get; set; }
    public bool IsPinned { get; set; }
    public DateTimeOffset? LastUsedAt { get; set; }
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    public Guid ItemTypeId { get; set; }
    public ItemType ItemType { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public ICollection<ItemCollection> ItemCollections { get; } = [];
    public ICollection<ItemTag> ItemTags { get; } = [];
}
