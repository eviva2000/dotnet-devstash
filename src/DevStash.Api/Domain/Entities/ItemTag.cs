namespace DevStash.Api.Domain.Entities;

public sealed class ItemTag
{
    public Guid ItemId { get; set; }
    public Item Item { get; set; } = null!;
    public Guid TagId { get; set; }
    public Tag Tag { get; set; } = null!;
}
