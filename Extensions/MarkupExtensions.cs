using Telegram.Bot.Types.ReplyMarkups;

namespace NamozVaqtlari.Extensions;

public static class MarkupExtensions
{
    static int maxButtonsLength = 3;

    public static InlineKeyboardMarkup GenerateButtons(
        IList<(string key, string value)> datas)
    {
        var buttons = new List<List<InlineKeyboardButton>>();

        for (int index = 0; index < datas.Count; index++)
        {
            if(index % maxButtonsLength == 0)
                buttons.Add(new List<InlineKeyboardButton>());

            buttons[index / maxButtonsLength].Add(
                new InlineKeyboardButton(datas[index].key)
                {
                    CallbackData = datas[index].value
                }
            );
        }

        return new InlineKeyboardMarkup(buttons);
    }
}