using Microsoft.AspNetCore.Mvc;
using ResourceManager.Api.Contracts;
using ResourceManager.Domain;
using ResourceManager.Infrastructure.Repositories;

namespace ResourceManager.Api.Controllers;

[ApiController]
[Route("api/bookings")]
public class BookingsController : ControllerBase
{
    private readonly BookingRepository _bookings;

    public BookingsController(BookingRepository bookings)
    {
        _bookings = bookings;
    }

    [HttpGet]
    public async Task<ActionResult<List<BookingResponse>>> List(
        [FromQuery] Guid? resourceId,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        [FromQuery] BookingStatus? status)
    {
        var bookings = await _bookings.ListAsync(resourceId, from?.ToUniversalTime(), to?.ToUniversalTime(), status);
        return Ok(bookings.Select(b => b.ToResponse()).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BookingResponse>> Get(Guid id)
    {
        var booking = await _bookings.GetAsync(id);
        if (booking is null)
        {
            return NotFound();
        }

        return Ok(booking.ToResponse());
    }

    [HttpPost]
    public async Task<ActionResult<BookingResponse>> Create(CreateBookingRequest request)
    {
        var booking = request.ToBooking();

        if (await _bookings.HasClashAsync(booking))
        {
            return Conflict(new ErrorResponse("Something else is booked on that resource for part of that interval."));
        }

        var created = await _bookings.AddAsync(booking);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created.ToResponse());
    }
}
