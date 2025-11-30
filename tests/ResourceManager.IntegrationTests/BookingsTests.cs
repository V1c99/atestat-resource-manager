using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using ResourceManager.Api.Contracts;
using ResourceManager.Domain;
using Xunit;

namespace ResourceManager.IntegrationTests;

[Collection("api")]
public class BookingsTests
{
    private readonly HttpClient _client;

    public BookingsTests(ApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task A_booking_that_starts_when_another_ends_is_accepted()
    {
        var resource = await TestData.CreateResourceAsync(_client);
        var day = TestData.NextMonday(9);

        var first = await _client.PostAsJsonAsync("/api/bookings", TestData.Booking(resource.Id, day, day.AddHours(1)), ApiFactory.Json);
        var second = await _client.PostAsJsonAsync("/api/bookings", TestData.Booking(resource.Id, day.AddHours(1), day.AddHours(2)), ApiFactory.Json);

        first.StatusCode.Should().Be(HttpStatusCode.Created);
        second.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task A_booking_that_ends_before_it_starts_is_rejected()
    {
        var resource = await TestData.CreateResourceAsync(_client);
        var day = TestData.NextMonday(11);

        var response = await _client.PostAsJsonAsync("/api/bookings", TestData.Booking(resource.Id, day.AddHours(2), day), ApiFactory.Json);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task A_booking_longer_than_the_maximum_is_rejected()
    {
        var resource = await TestData.CreateResourceAsync(_client);
        var day = TestData.NextMonday(6);

        var response = await _client.PostAsJsonAsync("/api/bookings",
            TestData.Booking(resource.Id, day, day.AddHours(BookingRules.MaxHours + 1)), ApiFactory.Json);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task A_booking_for_an_unknown_resource_is_rejected()
    {
        var day = TestData.NextMonday(12);

        var response = await _client.PostAsJsonAsync("/api/bookings", TestData.Booking(Guid.NewGuid(), day, day.AddHours(1)), ApiFactory.Json);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task A_booking_for_a_deactivated_resource_is_rejected()
    {
        var resource = await TestData.CreateResourceAsync(_client);
        await _client.PostAsync($"/api/resources/{resource.Id}/deactivate", null);

        var day = TestData.NextMonday(15);
        var response = await _client.PostAsJsonAsync("/api/bookings", TestData.Booking(resource.Id, day, day.AddHours(1)), ApiFactory.Json);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task The_seeded_users_are_listed()
    {
        var users = await _client.GetFromJsonAsync<List<UserResponse>>("/api/users", ApiFactory.Json);

        users.Should().HaveCount(4);
        users!.Should().Contain(u => u.Role == UserRole.Administrator);
    }

    [Fact]
    public async Task The_audit_trail_records_the_creation_and_the_cancellation()
    {
        var resource = await TestData.CreateResourceAsync(_client);
        var day = TestData.NextMonday(8);

        var created = await _client.PostAsJsonAsync("/api/bookings", TestData.Booking(resource.Id, day, day.AddHours(1)), ApiFactory.Json);
        var booking = (await created.Content.ReadFromJsonAsync<BookingResponse>(ApiFactory.Json))!;

        await _client.PostAsJsonAsync($"/api/bookings/{booking.Id}/cancel",
            new CancelBookingRequest { CancelledBy = TestData.Requester, Reason = "Not needed anymore" }, ApiFactory.Json);

        var trail = await _client.GetFromJsonAsync<List<BookingAuditResponse>>($"/api/bookings/{booking.Id}/audit", ApiFactory.Json);

        trail.Should().HaveCount(2);
        trail![0].FromStatus.Should().BeNull();
        trail[0].ToStatus.Should().Be(BookingStatus.Confirmed);
        trail[1].FromStatus.Should().Be(BookingStatus.Confirmed);
        trail[1].ToStatus.Should().Be(BookingStatus.Cancelled);
        trail[1].Note.Should().Be("Not needed anymore");
    }

    [Fact]
    public async Task A_booking_sent_with_a_local_offset_is_stored_as_utc()
    {
        var resource = await TestData.CreateResourceAsync(_client);
        var day = TestData.NextMonday(7);
        var bucharest = new DateTimeOffset(day.DateTime, TimeSpan.FromHours(2));

        var response = await _client.PostAsJsonAsync("/api/bookings",
            TestData.Booking(resource.Id, bucharest, bucharest.AddHours(1)), ApiFactory.Json);
        var booking = (await response.Content.ReadFromJsonAsync<BookingResponse>(ApiFactory.Json))!;

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        booking.StartsAt.Offset.Should().Be(TimeSpan.Zero);
        booking.StartsAt.Should().Be(bucharest.ToUniversalTime());
    }
}
