using DevStash.Api.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace DevStash.Api.Data.Identity;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public ApplicationUser()
    {
        Id = Guid.NewGuid();
    }

    public ICollection<Item> Items { get; } = [];
    public ICollection<Collection> Collections { get; } = [];
    public ICollection<Tag> Tags { get; } = [];
    public ICollection<ItemType> ItemTypes { get; } = [];
}
