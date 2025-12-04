using FluentAssertions;
using Npgsql;
using Xunit;

namespace ResourceManager.IntegrationTests;

[Collection("api")]
public class SchemaTests
{
    private readonly ApiFactory _factory;

    public SchemaTests(ApiFactory factory)
    {
        _factory = factory;

        // Asking for a client is what starts the application, and starting it is what runs
        // the migrations. Without this the two queries below find nothing.
        factory.CreateClient();
    }

    [Fact]
    public async Task The_no_overlap_constraint_is_present_on_the_bookings_table()
    {
        await using var connection = new NpgsqlConnection(_factory.ConnectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(
            "select contype from pg_constraint where conname = 'bookings_no_overlap'",
            connection);

        var contype = await command.ExecuteScalarAsync();

        contype.Should().Be('x');
    }

    [Fact]
    public async Task The_btree_gist_extension_is_installed()
    {
        await using var connection = new NpgsqlConnection(_factory.ConnectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(
            "select count(*) from pg_extension where extname = 'btree_gist'",
            connection);

        var count = (long)(await command.ExecuteScalarAsync())!;

        count.Should().Be(1);
    }
}
