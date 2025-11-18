using ResourceManager.Domain;

namespace ResourceManager.Api.Contracts;

public class CreateResourceRequest
{
    public string Name { get; set; } = string.Empty;
    public ResourceType Type { get; set; }
    public int Capacity { get; set; }
    public string Location { get; set; } = string.Empty;
}

public class UpdateResourceRequest
{
    public string Name { get; set; } = string.Empty;
    public ResourceType Type { get; set; }
    public int Capacity { get; set; }
    public string Location { get; set; } = string.Empty;
}

public class CreateBookingRequest
{
    public Guid ResourceId { get; set; }
    public Guid RequesterId { get; set; }
    public DateTimeOffset StartsAt { get; set; }
    public DateTimeOffset EndsAt { get; set; }
    public string Purpose { get; set; } = string.Empty;
}
