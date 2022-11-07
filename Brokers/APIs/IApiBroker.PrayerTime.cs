using NamozVaqtlari.Models;

namespace NamozVaqtlari.Brokers.APIs;

public partial interface IApiBroker
{
    Task<PrayerTime> GetDailyPrayerTimeAsync(string region);
    Task<IList<PrayerTime>> GetWeeklyPrayerTimeListAsync(string region);
    Task<IList<PrayerTime>> GetMonthlyPrayerTimeListAsync(string region, int currentMonth);
}