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
    private readonly ResourceRepository _resources;

    public BookingsController(BookingRepository bookings, ResourceRepository resources)
    {
        _bookings = bookings;
        _resources = resources;
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
        var resource = await _resources.GetAsync(request.ResourceId);
        if (resource is null || !resource.IsActive)
        {
            return BadRequest(new ErrorResponse("That resource does not exist or is not active."));
        }

        var booking = request.ToBooking();

        if (await _bookings.HasClashAsync(booking))
        {
            return Conflict(new ErrorResponse($"{resource.Name} is already booked for part of that interval."));
        }

        var created = await _bookings.AddAsync(booking);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created.ToResponse());
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<BookingResponse>> Cancel(Guid id, CancelBookingRequest request)
    {
        var cancelled = await _bookings.CancelAsync(id, request.CancelledBy, request.Reason);
        if (cancelled is null)
        {
            return NotFound();
        }

        return Ok(cancelled.ToResponse());
    }

    [HttpGet("{id:guid}/audit")]
    public async Task<ActionResult<List<BookingAuditResponse>>> Audit(Guid id)
    {
        var booking = await _bookings.GetAsync(id);
        if (booking is null)
        {
            return NotFound();
        }

        var trail = await _bookings.AuditTrailAsync(id);
        return Ok(trail.Select(a => a.ToResponse()).ToList());
    }
}
