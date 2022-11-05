using NamozVaqtlari.Brokers.APIs;
using NamozVaqtlari.Models;

namespace NamozVaqtlari.Services.PrayerTime;

public class PrayerTimeService : IPrayerTimeService
{
    private readonly IApiBroker apiBroker;

    public PrayerTimeService(IApiBroker apiBroker)
    {
        this.apiBroker = apiBroker;
    }

    public async Task<Time> RetrieveDailyPrayerTimeAsync(string region)
    {
        var prayerTimes = await this.apiBroker
            .GetDailyPrayerTimeAsync(region);

        return prayerTimes.Time;
    }

    public async Task<IList<Time>> RetrieveWeeklyPrayerTimeListAsync(string region)
    {
        var payerTimesList = await this.apiBroker
            .GetWeeklyPrayerTimeListAsync(region);

        return payerTimesList.Select(prayerTimes => prayerTimes.Time).ToList();
    }

    public async Task<IList<Time>> RetrieveMonthlyPrayerTimeListAsync(string region)
    {
        var payerTimesList = await this.apiBroker
            .GetMonthlyPrayerTimeListAsync(region);

        return payerTimesList.Select(prayerTimes => prayerTimes.Time).ToList();
    }
}