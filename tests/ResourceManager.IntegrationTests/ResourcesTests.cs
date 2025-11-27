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
}
