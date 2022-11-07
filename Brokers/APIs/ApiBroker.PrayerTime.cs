using NamozVaqtlari.Models;
using System.Text.Json;

namespace NamozVaqtlari.Brokers.APIs;

public partial class ApiBroker : IApiBroker
{
    private const string BASE_URL = "https://islomapi.uz/api/";
    public async Task<PrayerTime> GetDailyPrayerTimeAsync(string region)
    {
        var url = BASE_URL + $"present/day?region={region}";

        var content = await GetContentAsync(url);

        return JsonSerializer.Deserialize<PrayerTime>(content);
    }

    public async Task<IList<PrayerTime>> GetWeeklyPrayerTimeListAsync(string region)
    {
        var url = BASE_URL + $"present/week?region={region}";

        var content = await GetContentAsync(url);

        return JsonSerializer.Deserialize<IList<PrayerTime>>(content);
    }

    public async Task<IList<PrayerTime>> GetMonthlyPrayerTimeListAsync(string region, int currentMonth)
    {
        //int currentMonth = DateTime.Now.Month;
        var url = BASE_URL + $"monthly?region={region}&month={currentMonth}";

        var content = await GetContentAsync(url);

        return JsonSerializer.Deserialize<IList<PrayerTime>>(content);
    }
}