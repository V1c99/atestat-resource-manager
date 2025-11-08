namespace ResourceManager.Domain;

public class BookingAudit
{
    public long Id { get; set; }
    public Guid BookingId { get; set; }
    public BookingStatus? FromStatus { get; set; }
    public BookingStatus ToStatus { get; set; }
    public Guid ChangedBy { get; set; }
    public DateTimeOffset ChangedAt { get; set; }
    public string Note { get; set; } = string.Empty;
}
