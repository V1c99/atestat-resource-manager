namespace ResourceManager.Domain;

public class BookingOverlapException : Exception
{
    public BookingOverlapException(Booking? clashesWith)
        : base("The resource is already booked for part of that interval.")
    {
        ClashesWith = clashesWith;
    }

    public Booking? ClashesWith { get; }
}
