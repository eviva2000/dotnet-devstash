namespace DevStash.Api.Domain.Entities;

public sealed class ItemCollection
{
    public Guid ItemId { get; set; }
    public Item Item { get; set; } = null!;
    public Guid CollectionId { get; set; }
    public Collection Collection { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
