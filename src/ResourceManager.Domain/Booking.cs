namespace ResourceManager.Domain;

public class Booking
{
    public Guid Id { get; set; }

    public Guid ResourceId { get; set; }
    public Resource? Resource { get; set; }

    public Guid RequesterId { get; set; }
    public User? Requester { get; set; }

    public DateTimeOffset StartsAt { get; set; }
    public DateTimeOffset EndsAt { get; set; }

    public string Purpose { get; set; } = string.Empty;
    public BookingStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
