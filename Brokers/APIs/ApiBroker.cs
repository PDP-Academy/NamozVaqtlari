using NamozVaqtlari.Models;

namespace NamozVaqtlari.Brokers.APIs;

public partial class ApiBroker : IApiBroker
{
    private async Task<string> GetContentAsync(string url)
    {
        using var client = new HttpClient();
        return await client.GetStringAsync(url);
    }

    public void Dispose()
    { }
}