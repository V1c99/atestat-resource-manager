namespace ResourceManager.Domain;

public class User
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }

    public List<Booking> Bookings { get; set; } = new();
}
