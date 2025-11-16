using Microsoft.AspNetCore.Mvc;
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
    public async Task<ActionResult<List<Booking>>> List(
        [FromQuery] Guid? resourceId,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        [FromQuery] BookingStatus? status)
    {
        return Ok(await _bookings.ListAsync(resourceId, from, to, status));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Booking>> Get(Guid id)
    {
        var booking = await _bookings.GetAsync(id);
        if (booking is null)
        {
            return NotFound();
        }

        return Ok(booking);
    }

    [HttpPost]
    public async Task<ActionResult<Booking>> Create(Booking booking)
    {
        var created = await _bookings.AddAsync(booking);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }
}
