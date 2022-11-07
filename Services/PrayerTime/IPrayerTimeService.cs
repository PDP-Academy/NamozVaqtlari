using NamozVaqtlari.Models;

namespace NamozVaqtlari.Services.PrayerTime;

public interface IPrayerTimeService
{
    Task<Time> RetrieveDailyPrayerTimeAsync(string region);
    Task<IList<Time>> RetrieveWeeklyPrayerTimeListAsync(string region);
    Task<IList<Time>> RetrieveMonthlyPrayerTimeListAsync(string region, int currentMonth);
}
