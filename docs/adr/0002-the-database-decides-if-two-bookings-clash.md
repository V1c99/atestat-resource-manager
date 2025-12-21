# 2. The database decides if two bookings clash

- Date: 2025-12-03
- Status: accepted

## Context

The whole application exists to stop two people booking the same room at the same time, and my
first version checked it the obvious way:

```csharp
var nearby = await _db.Bookings
    .Where(b => b.ResourceId == booking.ResourceId
        && b.Status == BookingStatus.Confirmed
        && b.StartsAt > from
        && b.StartsAt < to)
    .ToListAsync();

return nearby.Any(existing => BookingRules.Overlaps(existing, booking));
```

That is a read followed by a write, and nothing holds the room between them. Two requests
arriving together both run the query, both see nothing, and both insert. I wrote a test that
fires two overlapping bookings at the same time and it failed the way I expected, with two rows
in the table.

The 2024 project had exactly this bug and nobody at the defence asked about it.

## Decision

The constraint goes in the schema:

```sql
CREATE EXTENSION IF NOT EXISTS btree_gist;

ALTER TABLE bookings
ADD CONSTRAINT bookings_no_overlap
EXCLUDE USING gist (
    resource_id WITH =,
    tstzrange(starts_at, ends_at, '[)') WITH &&
)
WHERE (status = 'Confirmed');
```

`resource_id WITH =` and `tstzrange(...) WITH &&` say: two rows may not have the same resource
and an overlapping time range. `[)` is the half-open interval, so a booking ending at 11:00 does
not clash with one starting at 11:00. The `WHERE` makes the constraint partial, so a cancelled
booking stops blocking the slot the moment it is cancelled.

## Consequences

The check is now atomic. The second transaction waits for the first to commit and is then
rejected, so the race cannot happen no matter how many instances of the API are running.

Three things follow from it. `resource_id` is a uuid and GiST does not index equality on plain
types on its own, so `btree_gist` has to be installed first, and the migration failed on a clean
database until I added that line. EF Core cannot model an exclusion constraint, so the migration
uses `migrationBuilder.Sql` and the model snapshot does not know the constraint exists. And the
insert now throws instead of returning a flag, so `BookingRepository` looks at SQLSTATE 23P01 on
the `DbUpdateException` and the controller turns that into a 409.

The C# check is gone rather than kept as a first line of defence. Two checks that can disagree
are worse than one, and the database is the one that cannot be bypassed. `BookingRules.Overlaps`
stays, but only to work out which booking to name in the 409.
