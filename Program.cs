using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using NamozVaqtlari.Views;
using NamozVaqtlari.Brokers.APIs;
using NamozVaqtlari.Services.PrayerTime;

class Program
{
    private const string TOKEN = "5463139532:AAEG8uujeblecAwOxEbfTX1NbP57-2DxYWQ";
    public static Dictionary<long, short> regions = new Dictionary<long, short>();

    #region Main
    static void Main(string[] args)
    {
        try
        {
            using var cts = new CancellationTokenSource();            
            var botClient = new TelegramBotClient(TOKEN);

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>()
            };

            botClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                pollingErrorHandler: HandlePollingErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: cts.Token);

            Console.WriteLine("Bot started working");

            Console.ReadLine();
        }
        catch { }
    }

    #endregion

    #region Update Handler
    static async Task HandleUpdateAsync(
        ITelegramBotClient botClient,
        Update update,
        CancellationToken cancellationToken)
    {
        var handler = update switch
        {
            { Message: { } } => OnMessageReceived(),
            { CallbackQuery: { } } => OnCallbackQueryReceived()
        };

        await handler;

        #region OnMessageReceived

        async Task OnMessageReceived()
        {
            var mainView = new MainView(GetPrayerTimeService());
            var message = update.Message;

            if (message.Type != MessageType.Text)
                return;

            if (message.Text == "/start")
            {
                await mainView.StartCommandAsync(botClient, message.Chat.Id, message.MessageId);
            }
        }
        #endregion

        #region OnCallbackQueryReceived
        async Task OnCallbackQueryReceived()
        {
            var mainView = new MainView(GetPrayerTimeService());
            var message = update.CallbackQuery.Message;
            string[] data = update.CallbackQuery.Data.Split();

            if (data[0] == "region")
            {
                regions.TryAdd(message.Chat.Id, short.Parse(data[1]));
               
                await mainView.SendOptionsAsync(
                    botClient: botClient,
                    chatId: message.Chat.Id,
                    messageId: message.MessageId,
                    isCallback: true);
            }
            else if (data[0] == "option")
            {
                int option = int.Parse(data[1]);

                await mainView.SendPrayerTimeAsync(
                    botClient: botClient,
                    chatId: message.Chat.Id,
                    messageId: message.MessageId,
                    option: option);
            }
        }
        #endregion

        IPrayerTimeService GetPrayerTimeService()
        {
            IApiBroker broker = new ApiBroker();

            return new PrayerTimeService(broker);
        }
    }
    #endregion

    #region Error handling
    static Task HandlePollingErrorAsync(
        ITelegramBotClient botClient,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        Console.WriteLine(ErrorMessage);
        return Task.CompletedTask;
    }
    #endregion
}