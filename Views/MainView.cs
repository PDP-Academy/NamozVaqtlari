using NamozVaqtlari.Extensions;
using NamozVaqtlari.Services.PrayerTime;
using Telegram.Bot;

namespace NamozVaqtlari.Views;

public class MainView
{
    #region Constants
    private const int OPTION_DAILY = 0;
	private const int OPTION_WEEKLY = 1;
	private const int OPTION_MONTHLY = 2;
	private string[] Regions = { "Toshkent", "Andijon", "Namangan", "Qo'qon", "Jizzax", "Xiva", "Termiz", "Samarqand", };
	private string[] Options = { "Kunlik", "Haftalik", "Oylik" };
    #endregion

    #region Ctor
    private readonly IPrayerTimeService prayerTimeService;

	public MainView(IPrayerTimeService prayerTimeService)
	{
		this.prayerTimeService = prayerTimeService;
    }
    #endregion

    #region Commands
    public async Task StartCommandAsync(
		ITelegramBotClient botClient,
		long chatId,
		int messageId)
	{
		var regions = Program.regions;

		if(!regions.ContainsKey(chatId))
		{
			await SendRegionsAsync(botClient, chatId);
		}
		else
		{
            await SendOptionsAsync(botClient, chatId, messageId);
        }
	}

	public async Task SendRegionsAsync(
		ITelegramBotClient botClient,
		long chatId)
	{
		var callbackDatas = GenerateCallbackData(Regions, "region");
		var markup = MarkupExtensions.GenerateButtons(callbackDatas);

		await botClient.SendTextMessageAsync(
			chatId: chatId,
			text: "Quyidagilardan birini tanlang:",
			replyMarkup: markup);
	}

	public async Task SendOptionsAsync(
		ITelegramBotClient botClient,
		long chatId,
		int messageId,
		bool isCallback = false)
    {
		var callbackDatas = GenerateCallbackData(Options, "option");
        var markup = MarkupExtensions.GenerateButtons(callbackDatas);

		if(isCallback)
		{
            await botClient.EditMessageTextAsync(
                chatId: chatId,
				text: "Quyidagilardan birini tanlang:",
                messageId: messageId,
                replyMarkup: markup);
        }
		else
		{
            await botClient.SendTextMessageAsync(
                chatId: chatId,
				text: "Quyidagilardan birini tanlang",
                replyMarkup: markup);
        }
    }

	public async Task SendPrayerTimeAsync(
		ITelegramBotClient botClient,
		long chatId,
		int messageId,
		int option)
	{
		var regions = Program.regions;
		var isParsed = regions.TryGetValue(chatId, out short region);

		if (!isParsed)
			return;

		var task = option switch
		{
			OPTION_DAILY => SendDailyPrayerTimes(),
			OPTION_WEEKLY => SendWeeklyPrayerTimes(),
			OPTION_MONTHLY => SendMonthlyPrayerTimes(),
			_ => throw new InvalidOperationException()
		};

		await task;

		async Task SendDailyPrayerTimes()
		{
			var prayerTime = await this.prayerTimeService
				.RetrieveDailyPrayerTimeAsync(Regions[region]);

			await botClient.EditMessageTextAsync(
				chatId: chatId,
				messageId: messageId,
				text: prayerTime.ToString());
		}

		async Task SendWeeklyPrayerTimes()
		{
            var prayerTimeList = await this.prayerTimeService
				.RetrieveWeeklyPrayerTimeListAsync(Regions[region]);

			var messageText = string.Join("\n", prayerTimeList);

            await botClient.EditMessageTextAsync(
                chatId: chatId,
				messageId: messageId,
                text: messageText);
        }

		async Task SendMonthlyPrayerTimes()
		{
            var prayerTimeList = await this.prayerTimeService
                .RetrieveMonthlyPrayerTimeListAsync(Regions[region]);

            var messageText = string.Join("\n", prayerTimeList);

            await botClient.EditMessageTextAsync(
                chatId: chatId,
                messageId: messageId,
                text: messageText);
        }
	}
    #endregion

    #region Private methods
    private IList<(string key, string value)> GenerateCallbackData(
		string[] values,
		string callBackData)
	{
		var datas = new List<(string key, string value)>();

		for (int i = 0; i < values.Length; i++)
		{
			datas.Add((values[i], $"{callBackData} {i}"));
		}

		return datas;
	}
    #endregion
}