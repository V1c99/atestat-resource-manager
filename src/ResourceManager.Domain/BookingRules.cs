namespace ResourceManager.Domain;

public static class BookingRules
{
    public const int MaxHours = 12;

    // Half open interval, the same as the '[)' I put in the database constraint. A booking
    // that ends at 11:00 does not clash with one that starts at 11:00.
    public static bool Overlaps(DateTimeOffset firstStart, DateTimeOffset firstEnd, DateTimeOffset secondStart, DateTimeOffset secondEnd)
    {
        return firstStart < secondEnd && secondStart < firstEnd;
    }

    public static bool Overlaps(Booking first, Booking second)
    {
        if (first.ResourceId != second.ResourceId)
        {
            return false;
        }

        if (first.Status != BookingStatus.Confirmed || second.Status != BookingStatus.Confirmed)
        {
            return false;
        }

        return Overlaps(first.StartsAt, first.EndsAt, second.StartsAt, second.EndsAt);
    }

    public static bool IsValidRange(DateTimeOffset startsAt, DateTimeOffset endsAt)
    {
        if (endsAt <= startsAt)
        {
            return false;
        }

        return endsAt - startsAt <= TimeSpan.FromHours(MaxHours);
    }
}
