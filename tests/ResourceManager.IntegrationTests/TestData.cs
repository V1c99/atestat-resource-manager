using System.Net.Http.Json;
using ResourceManager.Api.Contracts;
using ResourceManager.Domain;

namespace ResourceManager.IntegrationTests;

public static class TestData
{
    public static readonly Guid Requester = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid OtherRequester = Guid.Parse("33333333-3333-3333-3333-333333333333");

    public static async Task<ResourceResponse> CreateResourceAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/resources", new CreateResourceRequest
        {
            Name = "Room " + Guid.NewGuid(),
            Type = ResourceType.MeetingRoom,
            Capacity = 8,
            Location = "Floor 2"
        }, ApiFactory.Json);

        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ResourceResponse>(ApiFactory.Json))!;
    }

    public static CreateBookingRequest Booking(Guid resourceId, DateTimeOffset startsAt, DateTimeOffset endsAt, Guid? requester = null)
    {
        return new CreateBookingRequest
        {
            ResourceId = resourceId,
            RequesterId = requester ?? Requester,
            StartsAt = startsAt,
            EndsAt = endsAt,
            Purpose = "Team meeting"
        };
    }

    public static DateTimeOffset NextMonday(int hour)
    {
        var day = DateTimeOffset.UtcNow.Date.AddDays(7);
        while (day.DayOfWeek != DayOfWeek.Monday)
        {
            day = day.AddDays(1);
        }

        return new DateTimeOffset(day, TimeSpan.Zero).AddHours(hour);
    }
}
