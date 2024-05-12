using System.Reflection.Metadata.Ecma335;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SimpleTGBot;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

public class TelegramBot
{
    // Токен TG-бота. Можно получить у @BotFather
    private const string BotToken = "6994478860:AAHYEHnzMTf8CxMTtitvgrMs79gqC6i48ug";

    /// <summary>
    /// Инициализирует и обеспечивает работу бота до нажатия клавиши Esc
    /// </summary>
   

    public async Task Run()
    {
        // Инициализация необходимых данных здесь

        var botClient = new TelegramBotClient(BotToken);

        using CancellationTokenSource cts = new CancellationTokenSource();

        ReceiverOptions receiverOptions = new ReceiverOptions()
        {
            AllowedUpdates = new[] { UpdateType.Message }
        };

        botClient.StartReceiving(
            updateHandler: OnMessageReceived,
            pollingErrorHandler: OnErrorOccured,
            receiverOptions: receiverOptions,
            cancellationToken: cts.Token
        );

        var me = await botClient.GetMeAsync(cancellationToken: cts.Token);
        Console.WriteLine($"Бот @{me.Username} запущен.\nДля остановки нажмите клавишу Esc...");

        while (Console.ReadKey().Key != ConsoleKey.Escape) { }

        cts.Cancel();
    }

    /// <summary>
    /// Обработчик события получения сообщения.
    /// </summary>
    /// <param name="botClient">Клиент, который получил сообщение</param>
    /// <param name="update">Событие, произошедшее в чате. Новое сообщение, голос в опросе, исключение из чата и т. д.</param>
    /// <param name="cancellationToken">Служебный токен для работы с многопоточностью</param>
    async Task OnMessageReceived(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        var message = update.Message;

        if (message.Text != null)
        {
            string userMessage = message.Text.ToLower();

            if (userMessage.Contains("/start"))
            {
                string reply = "Привет! Я могу порекомендовать тебе книги. Какие жанры тебя интересуют? " +
                    "Доступные жанры: детектив, романтика, мистика, проза, фантастика.";
                await botClient.SendTextMessageAsync(chatId: message.Chat.Id, text: reply);
            }
            else if (userMessage.Contains("привет") || userMessage.Contains("здравствуй"))
            {
                string reply = "Привет! Я могу порекомендовать тебе книги. Какие жанры тебя интересуют? " +
                    "Доступные жанры: детектив, романтика, мистика, проза, фантастика.";
                await botClient.SendTextMessageAsync(chatId: message.Chat.Id, text: reply);
            }
            else if (userMessage.Contains("детектив") || userMessage.Contains("романтика") ||
                     userMessage.Contains("мистика") || userMessage.Contains("проза") || userMessage.Contains("фантастика"))
            {
                string selectedGenre = userMessage.Split(' ').First();
                var recommendedBooks = GetRecommendedBooks(selectedGenre);
                string reply = $"Рекомендую тебе книги из жанра '{selectedGenre}': {string.Join(", ", recommendedBooks)}\n" +
                    "Теперь ты можешь запросить описание книги, написав 'описание книги (Название книги)'";
                await botClient.SendTextMessageAsync(chatId: message.Chat.Id, text: reply);
            }
            else if (userMessage.StartsWith("описание книги"))
            {
                string bookName = userMessage.Substring("описание книги".Length).Trim();
                string bookDescription = GetBookDescription(bookName);
                if (!string.IsNullOrEmpty(bookDescription))
                {
                    string reply = $"Описание книги '{bookName}': {bookDescription}";
                    await botClient.SendTextMessageAsync(chatId: message.Chat.Id, text: reply);
                }
                else
                {
                    string reply = $"Извини, описание для книги '{bookName}' не найдено.";
                    await botClient.SendTextMessageAsync(chatId: message.Chat.Id, text: reply);
                }
            }
            else
            {
                string generalReply = "Извини, не могу понять твой запрос. Попробуй выбрать жанр из списка или запросить описание книги.";
                await botClient.SendTextMessageAsync(chatId: message.Chat.Id, text: generalReply);
            }
        }
    }


    string GetBookDescription(string bookName)
    {
        // Описание книг
        Dictionary<string, string> bookDescriptions = new Dictionary<string, string>()
    {
        { "код да винчи", "Роман Дэна Брауна, ставший бестселлером, совмещает в себе элементы детектива и мистики." },
        { "гордость и предубеждение", "Классический роман Джейн Остин о любви, гордости и предвзятости." },
        { "гарри поттер и философский камень", "Первая книга в серии о Гарри Поттере, полная магии и приключений." },
        { "тень ветра", "Роман Карлоса Руиса Сафона о таинственной книге, которая меняет жизнь главного героя." },
        { "мастер и маргарита", "Главное произведение Михаила Булгакова, сочетающее сатиру, мистику и философию." },
        { "1984", "Роман Джорджа Оруэлла, предсказывающий тоталитарное будущее и контроль над людьми." },
        { "ведьмак", "Сага о геральте из Ривии, охотнике на монстров, написанная Анджеем Сапковским." },
        { "убийство в восточном экспрессе", "Детектив Агаты Кристи, в котором Эркюль Пуаро расследует убийство в поезде." },
        { "три метра над уровнем неба", "Роман Федерико Моччиа о запретной любви между двумя молодыми людьми." },
        { "амулет", "Книга Кэза Казуира, рассказывающая о приключениях двух детей в фэнтезийном мире." },
        { "марсианин", "Научно-фантастический роман Энди Вейра о выживании астронавта на Марсе." },
        { "автостопом по галактике", "Юмористическая фантастическая сага Дугласа Адамса о приключениях по галактике." },
        { "игра эндера", "Научно-фантастический роман Орсона Скотта Карда о детях, обучающихся в боевой школе в космосе." }
    };

        return bookDescriptions.ContainsKey(bookName.ToLower()) ? bookDescriptions[bookName.ToLower()] : "";
    }



    List<string> GetRecommendedBooks(string genre)
    {
        //список жанров и книг
        Dictionary<string, List<string>> bookRecommendations = new Dictionary<string, List<string>>()
    {
        { "детектив", new List<string> { "Код да Винчи", "Убийство в Восточном экспрессе", "Тень ветра" } },
        { "романтика", new List<string> { "Гордость и предубеждение", "Три метра над уровнем неба" } },
        { "мистика", new List<string> { "Гарри Поттер и философский камень", "Ведьмак", "Амулет" } },
        { "проза", new List<string> { "Мастер и Маргарита", "Преступление и наказание", "1984" } },
        { "фантастика", new List<string> { "Марсианин", "Автостопом по галактике", "Игра Эндера" } }

    };

        return bookRecommendations.ContainsKey(genre) ? bookRecommendations[genre] : new List<string>();
    }


    /// <summary>
    /// Обработчик исключений, возникших при работе бота
    /// </summary>
    /// <param name="botClient">Клиент, для которого возникло исключение</param>
    /// <param name="exception">Возникшее исключение</param>
    /// <param name="cancellationToken">Служебный токен для работы с многопоточностью</param>
    /// <returns></returns>
    Task OnErrorOccured(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Ошибка: {exception.Message}");

        // Логирование ошибок в файл
        string logFilePath = "error.log";
        using (StreamWriter writer = new StreamWriter(logFilePath, append: true))
        {
            writer.WriteLine($"Ошибка: {exception.Message}");
            writer.WriteLine($"Стек вызовов: {exception.StackTrace}");
            writer.WriteLine();
        }
        // Завершаем работу
        return Task.CompletedTask;
    }
}

