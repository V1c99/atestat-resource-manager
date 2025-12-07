using Microsoft.EntityFrameworkCore;
using Npgsql;
using ResourceManager.Domain;

namespace ResourceManager.Infrastructure.Repositories;

public class BookingRepository
{
    private readonly AppDbContext _db;

    public BookingRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Booking>> ListAsync(Guid? resourceId, DateTimeOffset? from, DateTimeOffset? to, BookingStatus? status)
    {
        var query = _db.Bookings
            .AsNoTracking()
            .Include(b => b.Resource)
            .Include(b => b.Requester)
            .AsQueryable();

        if (resourceId is not null)
        {
            query = query.Where(b => b.ResourceId == resourceId);
        }

        if (status is not null)
        {
            query = query.Where(b => b.Status == status);
        }

        if (from is not null)
        {
            query = query.Where(b => b.EndsAt > from);
        }

        if (to is not null)
        {
            query = query.Where(b => b.StartsAt < to);
        }

        return await query.OrderBy(b => b.StartsAt).ToListAsync();
    }

    public async Task<Booking?> GetAsync(Guid id)
    {
        return await _db.Bookings
            .AsNoTracking()
            .Include(b => b.Resource)
            .Include(b => b.Requester)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<Booking> AddAsync(Booking booking)
    {
        booking.Id = Guid.NewGuid();
        booking.Status = BookingStatus.Confirmed;
        booking.CreatedAt = DateTimeOffset.UtcNow;

        _db.Bookings.Add(booking);
        _db.BookingAudits.Add(new BookingAudit
        {
            BookingId = booking.Id,
            FromStatus = null,
            ToStatus = BookingStatus.Confirmed,
            ChangedBy = booking.RequesterId,
            ChangedAt = booking.CreatedAt,
            Note = "Booking created"
        });

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            // 23P01 is exclusion_violation, which here can only be bookings_no_overlap.
            // Anything else that went wrong is not mine to swallow.
            if (ex.InnerException is PostgresException postgres && postgres.SqlState == "23P01")
            {
                throw new BookingOverlapException(await FindClashAsync(booking));
            }

            throw;
        }

        return await GetAsync(booking.Id) ?? booking;
    }

    public async Task<Booking?> CancelAsync(Guid id, Guid cancelledBy, string reason)
    {
        var booking = await _db.Bookings.FirstOrDefaultAsync(b => b.Id == id);
        if (booking is null)
        {
            return null;
        }

        if (booking.Status == BookingStatus.Cancelled)
        {
            return booking;
        }

        booking.Status = BookingStatus.Cancelled;
        _db.BookingAudits.Add(new BookingAudit
        {
            BookingId = booking.Id,
            FromStatus = BookingStatus.Confirmed,
            ToStatus = BookingStatus.Cancelled,
            ChangedBy = cancelledBy,
            ChangedAt = DateTimeOffset.UtcNow,
            Note = reason
        });

        await _db.SaveChangesAsync();
        return await GetAsync(booking.Id);
    }

    public async Task<List<BookingAudit>> AuditTrailAsync(Guid bookingId)
    {
        return await _db.BookingAudits
            .AsNoTracking()
            .Where(a => a.BookingId == bookingId)
            .OrderBy(a => a.Id)
            .ToListAsync();
    }

    // The database has already refused the insert. This only finds the booking to name
    // in the message.
    private async Task<Booking?> FindClashAsync(Booking booking)
    {
        var from = booking.StartsAt.AddDays(-1);
        var to = booking.EndsAt.AddDays(1);

        var nearby = await _db.Bookings
            .AsNoTracking()
            .Include(b => b.Resource)
            .Include(b => b.Requester)
            .Where(b => b.ResourceId == booking.ResourceId
                && b.Status == BookingStatus.Confirmed
                && b.StartsAt > from
                && b.StartsAt < to)
            .ToListAsync();

        return nearby.FirstOrDefault(existing => BookingRules.Overlaps(existing, booking));
    }
}
