namespace Content.Shared._AS.PersistentSystems;

public static class CanonicalFormat
{
    private const int IdMultiplier = 21; // Reasonable churn. bigger = faster growing
    private const int IdOffset = 1690420; // Population in the millions by the time any of us are around. bigger = older

    private const int YearOffset = 500; // 500 was chosen as the offset to match with existing ss13/14 conventions.

    /// <summary>
    /// Converts a timestamp to the canonical in-game date by offsetting the year by YearOffset.
    /// Defaults to the current time if none is given.
    /// </summary>
    /// <remarks>
    /// Refactor this by 2-29-2400 to properly account for 400 divisibility rules for leap years.
    /// See: https://learn.microsoft.com/en-us/dotnet/api/system.datetime.addyears?view=net-10.0
    /// </remarks>
    /// <param name="dateTime">The UTC timestamp to convert. If null, uses the current UTC time.</param>
    /// <returns>The corresponding canonical in-game date.</returns>
    public static DateTime CanonDateTime(DateTime? dateTime = null)
    {
        return (dateTime ?? DateTime.UtcNow).AddYears(YearOffset);
    }

    /// <summary>
    /// Converts the internal profileId into a corresponding ID number canonical for in game displays.
    /// An offset and scaling factor are applied to give realistic id numbers while preserving order.
    /// </summary>
    /// <remarks>
    /// Only used for player facing displays use profileId for any in game logic and networking.
    /// </remarks>
    /// <param name="profileId">Raw profileId as represented in the database.</param>
    /// <returns>The canonical in-game ID number for in game display.</returns>
    public static int? CanonId(int? profileId)
    {
        return (profileId * IdMultiplier) + IdOffset;
    }
}
