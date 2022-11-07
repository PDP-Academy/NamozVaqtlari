using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using NamozVaqtlari.Brokers.APIs;
using NamozVaqtlari.Services.PrayerTime;
using TelegramBot.Core;
using NamozVaqtlari.Views;
class Program
{
    public static void Main(string[] args)
    {

        new NamozVaqtlari.Views.MainView(new PrayerTimeService(new ApiBroker())).Start();
    }
}