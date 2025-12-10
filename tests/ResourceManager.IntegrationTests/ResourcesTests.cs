using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ResourceManager.Api.Contracts;
using ResourceManager.Domain;
using Xunit;

namespace ResourceManager.IntegrationTests;

[Collection("api")]
public class ResourcesTests
{
    private readonly HttpClient _client;

    public ResourcesTests(ApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task A_new_resource_can_be_read_back()
    {
        var created = await TestData.CreateResourceAsync(_client);

        var fetched = await _client.GetFromJsonAsync<ResourceResponse>($"/api/resources/{created.Id}", ApiFactory.Json);

        fetched.Should().NotBeNull();
        fetched!.Name.Should().Be(created.Name);
        fetched.Capacity.Should().Be(8);
        fetched.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task A_resource_without_a_name_is_rejected()
    {
        var response = await _client.PostAsJsonAsync("/api/resources", new CreateResourceRequest
        {
            Name = string.Empty,
            Type = ResourceType.Equipment,
            Capacity = 1,
            Location = "Store room"
        }, ApiFactory.Json);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task A_resource_with_zero_capacity_is_rejected()
    {
        var response = await _client.PostAsJsonAsync("/api/resources", new CreateResourceRequest
        {
            Name = "Broken " + Guid.NewGuid(),
            Type = ResourceType.Equipment,
            Capacity = 0,
            Location = "Store room"
        }, ApiFactory.Json);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task A_deactivated_resource_is_hidden_from_the_default_list()
    {
        var resource = await TestData.CreateResourceAsync(_client);
        await _client.PostAsync($"/api/resources/{resource.Id}/deactivate", null);

        var active = await _client.GetFromJsonAsync<List<ResourceResponse>>("/api/resources", ApiFactory.Json);
        var all = await _client.GetFromJsonAsync<List<ResourceResponse>>("/api/resources?includeInactive=true", ApiFactory.Json);

        active!.Should().NotContain(r => r.Id == resource.Id);
        all!.Should().Contain(r => r.Id == resource.Id);
    }

    [Fact]
    public async Task A_resource_can_be_renamed_and_moved()
    {
        var resource = await TestData.CreateResourceAsync(_client);
        var newName = "Renamed " + Guid.NewGuid();

        var response = await _client.PutAsJsonAsync($"/api/resources/{resource.Id}", new UpdateResourceRequest
        {
            Name = newName,
            Type = ResourceType.LabSlot,
            Capacity = 3,
            Location = "Workshop"
        }, ApiFactory.Json);

        var updated = (await response.Content.ReadFromJsonAsync<ResourceResponse>(ApiFactory.Json))!;

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        updated.Name.Should().Be(newName);
        updated.Type.Should().Be(ResourceType.LabSlot);
        updated.Capacity.Should().Be(3);
        updated.Location.Should().Be("Workshop");
    }

    [Fact]
    public async Task A_deactivated_resource_can_be_activated_again()
    {
        var resource = await TestData.CreateResourceAsync(_client);

        await _client.PostAsync($"/api/resources/{resource.Id}/deactivate", null);
        var response = await _client.PostAsync($"/api/resources/{resource.Id}/activate", null);
        var updated = (await response.Content.ReadFromJsonAsync<ResourceResponse>(ApiFactory.Json))!;

        updated.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Asking_for_a_resource_that_does_not_exist_gives_404()
    {
        var unknown = Guid.NewGuid();

        var get = await _client.GetAsync($"/api/resources/{unknown}");
        var now = DateTimeOffset.UtcNow;
        var schedule = await _client.GetAsync(
            $"/api/resources/{unknown}/schedule?from={Uri.EscapeDataString(now.ToString("O"))}&to={Uri.EscapeDataString(now.AddDays(1).ToString("O"))}");
        var deactivate = await _client.PostAsync($"/api/resources/{unknown}/deactivate", null);

        get.StatusCode.Should().Be(HttpStatusCode.NotFound);
        schedule.StatusCode.Should().Be(HttpStatusCode.NotFound);
        deactivate.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task A_schedule_window_that_ends_before_it_starts_is_rejected()
    {
        var resource = await TestData.CreateResourceAsync(_client);
        var now = DateTimeOffset.UtcNow;

        var response = await _client.GetAsync(
            $"/api/resources/{resource.Id}/schedule?from={Uri.EscapeDataString(now.ToString("O"))}&to={Uri.EscapeDataString(now.AddHours(-1).ToString("O"))}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Resources_can_be_filtered_by_type()
    {
        var rooms = await _client.GetFromJsonAsync<List<ResourceResponse>>("/api/resources?type=Vehicle", ApiFactory.Json);

        rooms!.Should().NotBeEmpty();
        rooms.Should().OnlyContain(r => r.Type == ResourceType.Vehicle);
    }

    [Fact]
    public async Task The_schedule_only_returns_the_bookings_inside_the_window()
    {
        var resource = await TestData.CreateResourceAsync(_client);
        var day = TestData.NextMonday(9);

        await _client.PostAsJsonAsync("/api/bookings", TestData.Booking(resource.Id, day, day.AddHours(1)), ApiFactory.Json);
        await _client.PostAsJsonAsync("/api/bookings", TestData.Booking(resource.Id, day.AddDays(3), day.AddDays(3).AddHours(1)), ApiFactory.Json);

        var window = await _client.GetFromJsonAsync<List<BookingResponse>>(
            $"/api/resources/{resource.Id}/schedule?from={Uri.EscapeDataString(day.AddHours(-1).ToString("O"))}&to={Uri.EscapeDataString(day.AddHours(6).ToString("O"))}",
            ApiFactory.Json);

        window.Should().HaveCount(1);
        window![0].StartsAt.Should().Be(day);
    }
}
