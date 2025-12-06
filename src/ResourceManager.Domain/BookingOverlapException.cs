namespace ResourceManager.Domain;

public class BookingOverlapException : Exception
{
    public BookingOverlapException()
        : base("The resource is already booked for part of that interval.")
    {
    }
}
