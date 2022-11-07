using NamozVaqtlari.Extensions;
using NamozVaqtlari.Services.PrayerTime;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using TelegramBot;
using TelegramBot.Core;


namespace NamozVaqtlari.Views;

public class MainView
{
    private const string TOKEN = "1840796662:AAHeCquOT3iDWQt_2ObHzXVZmbX4MAIRXkg";
    private Dictionary<long, short> users = new Dictionary<long, short>();
    private List<string> regions = new List<string>() { "Toshkent", "Andijon" };
    private List<string> timeOptions = new() { "Kunlik", "Haftalik", "Oylik" };
    private List<string> WeekNames = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.DayNames.ToList<string>();
    private List<string> MonthNames = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.MonthNames.ToList<string>();


    private IPrayerTimeService prayerTimeService;

    public MainView(IPrayerTimeService prayerTimeService)
    {
        this.prayerTimeService = prayerTimeService;
    }

    public void Start()
    {
        TelegramBot.Core.TelegramBot bot = new TelegramBot.Core.TelegramBot(TOKEN, HandlePollingErrorAsync);

        bot.Hears("salom", async ctx =>
        {
            await ctx.ReplyWithHTML("Salom");
        });

        //Start command handling
        bot.Start(async ctx =>
        {
            await StartAction(ctx);
        });


        bot.Action("GoToStart", this.StartAction);





        //Actions
        bot.CallbackData(async ctx =>
        {
            if (ctx.CallbackQuery.Data.StartsWith("SetOption"))
            {
                int index = int.Parse(ctx.CallbackQuery.Data.Split("#")[1]);
                if (index == 0)
                    await SendDaily(ctx);
                else if (index == 1)
                    await sendWeekly(ctx, ((int)DateTime.Now.DayOfWeek));
                else if (index == 2)
                    await sendMonthly(ctx);

            }
            else if (ctx.CallbackQuery.Data.StartsWith("SetRegion"))
            {
                if (this.users.ContainsKey(ctx.CallbackQuery.From.Id))
                    this.users[ctx.CallbackQuery.From.Id] = short.Parse(ctx.CallbackQuery.Data.Split("#")[1]);
                else
                    this.users.Add(ctx.CallbackQuery.From.Id, short.Parse(ctx.CallbackQuery.Data.Split("#")[1]));
                await this.ReplyWithOptions(ctx);
            }
            else if (ctx.CallbackQuery.Data.StartsWith("WeekDayIndex"))
            {
                int dayIndex = int.Parse(ctx.CallbackQuery.Data.Split("#")[1]);
                if (dayIndex < 0 || dayIndex >= this.WeekNames.Count)
                {
                    await ctx.Client.AnswerCallbackQueryAsync(ctx.CallbackQuery.Id, "Noto'g'ri tanlov!");
                    return;
                };
                await this.sendWeekly(ctx, dayIndex);
            }
            else if (ctx.CallbackQuery.Data.StartsWith("MonthIndex"))
            {
                int monthIndex = int.Parse(ctx.CallbackQuery.Data.Split("#")[1]);
                if (monthIndex < 0 || monthIndex >= this.MonthNames.Count)
                {
                    await ctx.Client.AnswerCallbackQueryAsync(ctx.CallbackQuery.Id, "Noto'g'ri tanlov!");
                    return;
                };
                await this.sendMonthly(ctx, monthIndex);
            }
        });




        try
        {
            bot.Launch();
        }
        catch (Exception)
        {

            throw;
        }
    }

    private async Task StartAction(IContext ctx)
    {
        if (this.users.ContainsKey(ctx.From.Id) || this.users.ContainsKey(ctx.Chat.Id))
        {
            await this.ReplyWithOptions(ctx);
            return;
        }
        var inlineButtons = new Markup.InlineKeyboardBuilder(regions.Count / 3 + 1);
        foreach (var item in regions.Select((str, index) => (str, index)))
            inlineButtons.AddButton(new(item.str) { CallbackData = $"SetRegion#{item.index}" });
        if (ctx.CallbackQuery != null)
            await ctx.Client.EditMessageTextAsync(ctx.Chat.Id, ctx.Message.MessageId, "<b>Assalomu alaykum Xush kelibsiz: </b>",
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html, replyMarkup: inlineButtons.Build());
        else
            await ctx.ReplyWithHTML("<b>Assalomu alaykum Xush kelibsiz: </b>", new() { ReplyMarkup = inlineButtons.Build() });


    }

    private async Task sendMonthly(IContext ctx, int? monthIndex = null)
    {
        if (monthIndex == null)
            monthIndex = DateTime.Now.Month;
        var result = await this.prayerTimeService.RetrieveMonthlyPrayerTimeListAsync(this.regions[this.users[ctx.CallbackQuery.From.Id]], (int)monthIndex);
        string s = "";
        foreach (var item in result)
            s += item.ToString() + "\n";

        var ibutton = new Markup.InlineKeyboardBuilder()
            .AddButton(new("Oldingi") { CallbackData = $"MonthIndex#{monthIndex - 1}" })
            .AddButton(new("Bosh menyu") { CallbackData = $"GoToStart" })
            .AddButton(new("Keyingi") { CallbackData = $"MonthIndex#{ monthIndex + 1}" });
        await ctx.Client.EditMessageTextAsync(ctx.Chat.Id, ctx.CallbackQuery.Message.MessageId, $"<b>{this.MonthNames[(int)monthIndex]}\n{s.ToString()}</b>",
            parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
            replyMarkup: ibutton.Build()
            );
    }

    private async Task sendWeekly(IContext ctx, int dayIndex = 0)
    {
        var result = await this.prayerTimeService.RetrieveWeeklyPrayerTimeListAsync(this.regions[this.users[ctx.CallbackQuery.From.Id]]);
        var ibutton = new Markup.InlineKeyboardBuilder()
            .AddButton(new("Oldingi") { CallbackData = $"WeekDayIndex#{dayIndex - 1}" })
            .AddButton(new("Bosh menyu") { CallbackData = $"GoToStart" })
            .AddButton(new("Keyingi") { CallbackData = $"WeekDayIndex#{ dayIndex + 1}" });
        await ctx.Client.EditMessageTextAsync(ctx.Chat.Id, ctx.CallbackQuery.Message.MessageId, $"<b>{this.WeekNames[dayIndex]}\n{result[dayIndex].ToString()}</b>",
            parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
            replyMarkup: ibutton.Build()
            );
    }

    private async Task SendDaily(IContext ctx)
    {
        if (!this.users.ContainsKey(ctx.CallbackQuery.From.Id))
        {
            await ctx.ReplyWithHTML("<b>So'rovingiz noto'g'ri yuborildi!</b>");
            return;
        }
        try
        {
            var result = await this.prayerTimeService.RetrieveDailyPrayerTimeAsync(this.regions[this.users[ctx.CallbackQuery.From.Id]]);

            await ctx.Client.EditMessageTextAsync(ctx.Chat.Id, ctx.CallbackQuery.Message.MessageId, @$"<b>Namoz vaqtlari: 
Saharlik: {result.TongSaharlik};
Quyosh chiqishi: {result.Quyosh};
Peshin: {result.Peshin};
Asr: {result.Asr};
Shom: {result.Shom};
Hufton: {result.Hufton};</b>", Telegram.Bot.Types.Enums.ParseMode.Html, replyMarkup: new Markup.InlineKeyboardBuilder().AddButton(new("Bosh menyu") { CallbackData = "GoToStart" }).Build());
        }
        catch (Exception ex)
        {
            await ctx.ReplyWithHTML($"<b>❌Xatolik:/n{ex.Message}</b>");
        }
    }

    private async Task ReplyWithOptions(IContext ctx)
    {
        var ibuttons = new Markup.InlineKeyboardBuilder();
        foreach (var item in this.timeOptions.Select((x, index) => (x, index)))
        {
            ibuttons.AddButton(new(item.x) { CallbackData = $"SetOption#{item.index}" });
        }
        if (ctx.CallbackQuery != null)
            await ctx.Client.EditMessageTextAsync(ctx.Chat.Id, ctx.CallbackQuery.Message.MessageId, "<b>Vaqtni tanglang: </b>", parseMode: Telegram.Bot.Types.Enums.ParseMode.Html, replyMarkup: ibuttons.Build());
        else
            await ctx.ReplyWithHTML("<b>Vaqtni tanglang: </b>", new() { ReplyMarkup = ibuttons.Build() });
    }

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