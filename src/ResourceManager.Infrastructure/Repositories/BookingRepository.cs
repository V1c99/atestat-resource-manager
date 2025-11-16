using Microsoft.EntityFrameworkCore;
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

        await _db.SaveChangesAsync();

        return await GetAsync(booking.Id) ?? booking;
    }
}
