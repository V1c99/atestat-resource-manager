using FluentAssertions;
using ResourceManager.Domain;
using Xunit;

namespace ResourceManager.UnitTests;

public class BookingRulesTests
{
    private static DateTimeOffset At(int hour, int minute = 0)
    {
        return new DateTimeOffset(2025, 12, 3, hour, minute, 0, TimeSpan.Zero);
    }

    [Fact]
    public void Two_bookings_over_the_same_hour_overlap()
    {
        BookingRules.Overlaps(At(10), At(11), At(10), At(11)).Should().BeTrue();
    }

    [Fact]
    public void A_booking_inside_another_one_overlaps()
    {
        BookingRules.Overlaps(At(9), At(13), At(10), At(11)).Should().BeTrue();
    }

    [Fact]
    public void A_booking_that_starts_before_the_other_ends_overlaps()
    {
        BookingRules.Overlaps(At(10), At(11), At(10, 30), At(11, 30)).Should().BeTrue();
    }

    [Fact]
    public void A_booking_that_starts_exactly_when_another_ends_does_not_overlap()
    {
        BookingRules.Overlaps(At(10), At(11), At(11), At(12)).Should().BeFalse();
    }

    [Fact]
    public void Bookings_on_different_parts_of_the_day_do_not_overlap()
    {
        BookingRules.Overlaps(At(8), At(9), At(16), At(17)).Should().BeFalse();
    }

    [Fact]
    public void Bookings_for_different_resources_never_overlap()
    {
        var first = Confirmed(Guid.NewGuid(), At(10), At(11));
        var second = Confirmed(Guid.NewGuid(), At(10), At(11));

        BookingRules.Overlaps(first, second).Should().BeFalse();
    }

    [Fact]
    public void A_cancelled_booking_does_not_overlap_anything()
    {
        var resourceId = Guid.NewGuid();
        var first = Confirmed(resourceId, At(10), At(11));
        var second = Confirmed(resourceId, At(10), At(11));
        second.Status = BookingStatus.Cancelled;

        BookingRules.Overlaps(first, second).Should().BeFalse();
    }

    [Fact]
    public void A_range_that_ends_before_it_starts_is_not_valid()
    {
        BookingRules.IsValidRange(At(11), At(10)).Should().BeFalse();
    }

    [Fact]
    public void A_range_of_zero_minutes_is_not_valid()
    {
        BookingRules.IsValidRange(At(10), At(10)).Should().BeFalse();
    }

    [Fact]
    public void A_range_longer_than_the_maximum_is_not_valid()
    {
        var start = At(6);
        BookingRules.IsValidRange(start, start.AddHours(BookingRules.MaxHours + 1)).Should().BeFalse();
    }

    [Fact]
    public void A_range_of_one_hour_is_valid()
    {
        BookingRules.IsValidRange(At(10), At(11)).Should().BeTrue();
    }

    private static Booking Confirmed(Guid resourceId, DateTimeOffset startsAt, DateTimeOffset endsAt)
    {
        return new Booking
        {
            Id = Guid.NewGuid(),
            ResourceId = resourceId,
            StartsAt = startsAt,
            EndsAt = endsAt,
            Status = BookingStatus.Confirmed
        };
    }
}
