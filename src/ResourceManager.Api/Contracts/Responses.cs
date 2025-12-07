using ResourceManager.Domain;

namespace ResourceManager.Api.Contracts;

public record ResourceResponse(
    Guid Id,
    string Name,
    ResourceType Type,
    int Capacity,
    string Location,
    bool IsActive);

public record BookingResponse(
    Guid Id,
    Guid ResourceId,
    string ResourceName,
    Guid RequesterId,
    string RequesterName,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    string Purpose,
    BookingStatus Status,
    DateTimeOffset CreatedAt);

public record BookingAuditResponse(
    long Id,
    BookingStatus? FromStatus,
    BookingStatus ToStatus,
    Guid ChangedBy,
    DateTimeOffset ChangedAt,
    string Note);

public record UserResponse(
    Guid Id,
    string FullName,
    string Email,
    UserRole Role);

public record ErrorResponse(string Message);

public record BookingConflictResponse(string Message, BookingResponse? ClashesWith);
