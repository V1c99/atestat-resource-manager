using ResourceManager.Domain;

namespace ResourceManager.Api.Contracts;

public static class Mapping
{
    public static ResourceResponse ToResponse(this Resource resource)
    {
        return new ResourceResponse(
            resource.Id,
            resource.Name,
            resource.Type,
            resource.Capacity,
            resource.Location,
            resource.IsActive);
    }

    public static BookingResponse ToResponse(this Booking booking)
    {
        return new BookingResponse(
            booking.Id,
            booking.ResourceId,
            booking.Resource?.Name ?? string.Empty,
            booking.RequesterId,
            booking.Requester?.FullName ?? string.Empty,
            booking.StartsAt,
            booking.EndsAt,
            booking.Purpose,
            booking.Status,
            booking.CreatedAt);
    }

    public static BookingAuditResponse ToResponse(this BookingAudit audit)
    {
        return new BookingAuditResponse(
            audit.Id,
            audit.FromStatus,
            audit.ToStatus,
            audit.ChangedBy,
            audit.ChangedAt,
            audit.Note);
    }

    public static Booking ToBooking(this CreateBookingRequest request)
    {
        return new Booking
        {
            ResourceId = request.ResourceId,
            RequesterId = request.RequesterId,
            // Npgsql refuses a DateTimeOffset that is not offset 0 when the column is
            // timestamptz. The browser sends +02:00 and it threw here every time.
            StartsAt = request.StartsAt.ToUniversalTime(),
            EndsAt = request.EndsAt.ToUniversalTime(),
            Purpose = request.Purpose
        };
    }

    public static Resource ToResource(this CreateResourceRequest request)
    {
        return new Resource
        {
            Name = request.Name,
            Type = request.Type,
            Capacity = request.Capacity,
            Location = request.Location
        };
    }
}
