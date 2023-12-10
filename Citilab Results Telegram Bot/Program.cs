using Citilab_Results_Telegram_Bot;
using Citilab_Results_Telegram_Bot.TestClientData;
using Citilab_Results_Telegram_Bot.WebDriver;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using File = System.IO.File;

var accessToken = AccessToken.Token;

// TODO: Implement proper file download with native C# 

#region WORK_IN_PROGRESS
var botClient = new TelegramBotClient(accessToken);

using CancellationTokenSource cts = new ();

// StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
ReceiverOptions receiverOptions = new ()
{
    AllowedUpdates = Array.Empty<UpdateType>() // receive all update types except ChatMember related updates
};

botClient.StartReceiving(
    updateHandler: HandleUpdateAsync,
    pollingErrorHandler: HandlePollingErrorAsync,
    receiverOptions: receiverOptions,
    cancellationToken: cts.Token
);

Console.ReadLine();
// Send cancellation request to stop bot
cts.Cancel();
return;

Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
    var handler = update switch
    {
        { Message: { } message }             => BotOnMessageReceived(message, cancellationToken),
        { EditedMessage: { } message }       => BotOnMessageReceived(message, cancellationToken),
        { CallbackQuery: { } callbackQuery } => BotOnCallbackQueryReceived(callbackQuery, cancellationToken),
        _                                    => UnknownUpdateHandlerAsync(update, cancellationToken)
    };

    return Task.FromResult(handler);
}

async Task BotOnMessageReceived(Message message, CancellationToken cancellationToken)
{
    if (message.Text is not { } messageText) 
        return;
    
    var chatId = message.Chat.Id;

    var action = messageText.Split(' ')[0] switch
    {
        "/results"          => SendResultInlineKeyboardAsync(botClient, message, cancellationToken),
        "/author"           => SendAboutInformationAsync(botClient, message, cancellationToken), 
      //"/remove"           => RemoveKeyboard(botClient, message, cancellationToken),
        "/throw"            => FailingHandlerAsync(botClient, message, cancellationToken),
        "/start"            => SendWelcomeAsync(botClient, message, cancellationToken),
        _                   => SendUsageInformationAsync(botClient, message, cancellationToken)
    };
    
    Message sentMessage = await action;
    Console.WriteLine($"The message was sent with id: {sentMessage.MessageId}");

    Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");
    return;

    static async Task<Message> SendWelcomeAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        const string welcome = "Приветствую \ud83d\udc4b, с помощью данного бота вы можете выгрузить результаты анализов, " +
                      "полученных в лабораториях Citilab.\n\nПожалуйста, выберите одну из команд ниже:";
        await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: welcome,
            cancellationToken: cancellationToken);

        return await 
            SendUsageInformationAsync(botClient: botClient, message: message, cancellationToken: cancellationToken);
    }
    
    static async Task<Message> SendResultInlineKeyboardAsync(ITelegramBotClient botClient, Message message,
        CancellationToken cancellationToken)
    {
        InlineKeyboardMarkup inlineKeyboard = new(
            new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Получить результаты", "Получение результатов"),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Показать возможности бота", "Функционал бота"),
                },
            });

        return await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: "Выберите следующее действие:",
            replyMarkup: inlineKeyboard,
            cancellationToken: cancellationToken);
    }
    
    static async Task<Message> SendAboutInformationAsync(ITelegramBotClient botClient, Message message,
        CancellationToken cancellationToken)
    {
        const string info = "Бот находится в процессе создания\n" +
                            "Для связи с разработчиком: @melix42 \u270c\ufe0f";
        
        return await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: info,
            cancellationToken: cancellationToken);
    }
    
    static async Task<Message> SendUsageInformationAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        const string usage = "Функционал бота:\n" +
                             "/results - получить результаты анализов\n" +
                             "/author - получить информацию об авторе";
        
        return await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: usage,
            replyMarkup: new ReplyKeyboardRemove(),
            cancellationToken: cancellationToken);
    }

    /*static async Task<Message> RemoveKeyboard(ITelegramBotClient botClient, Message message,
        CancellationToken cancellationToken)
    {
        return await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: "Убираем выбор",
            replyMarkup: new ReplyKeyboardRemove(),
            cancellationToken: cancellationToken);
    }*/

    static Task<Message> FailingHandlerAsync(ITelegramBotClient botClient, Message message,
        CancellationToken cancellationToken)
    {
        throw new IndexOutOfRangeException();
    }
}

async Task BotOnCallbackQueryReceived(CallbackQuery callbackQuery, CancellationToken cancellationToken)
{
    await botClient.AnswerCallbackQueryAsync(
        callbackQueryId: callbackQuery.Id,
        text: $"{callbackQuery.Data}",
        cancellationToken: cancellationToken,
        showAlert: false);
    
    if (callbackQuery.Message == null) return;
    Console.WriteLine("IM HERE ON TOP OF AWAITING ACTION BEFORE SWITCH");
    var action = callbackQuery.Data switch
    {
        "Получение результатов"     => ProceedToFetchResultsAsync(botClient, callbackQuery.Message, cancellationToken),
        "Функционал бота"           => SendAboutInformationAsync(botClient, callbackQuery.Message, cancellationToken), 
        _                           => BackToListeningAsync(botClient, callbackQuery.Message, cancellationToken)
    };
    Console.WriteLine("IM HERE AWAITING ACTION");
    await action;
    Console.WriteLine($"IM HERE AFTER AWAITING ACTION RETURNED: {action}. IM SUPPOSED TO BE LAST ONE AFTER FILE SEND");
    return;

    static async Task ProceedToFetchResultsAsync(ITelegramBotClient botClient, Message message,
        CancellationToken cancellationToken)
    {
        await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: "Начался поиск ваших результатов, пожалуйста подождите",
            cancellationToken: cancellationToken);
        
        await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: "\ud83d\udc68\u200d\ud83d\udcbb",
            cancellationToken: cancellationToken);
        
        Console.WriteLine("IM HERE STARTING TO DOWNLOAD");
        var path = await Task.Run(() => WebDriver.GetResultsFromCitilab(ClientHelper.GetTestClient()));
        //var path = await WebDriver.GetResultsFromCitilabAsync(ClientHelper.GetTestClient()));
        Console.WriteLine("IM HERE AFTER FILE WAS DOWNLOADED SUCCESSFULLY");
        await SendResultsToUserAsync(path);
        return;

        async Task SendResultsToUserAsync(string pathToFile)
        {
            Console.WriteLine($"Im here starting to SEND FILE FROM: {pathToFile}");
            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Вы можете просмотреть свои результаты в данном файле \ud83d\udc47",
                cancellationToken: cancellationToken);

            await using Stream stream = File.OpenRead(pathToFile);
            await botClient.SendDocumentAsync(
                chatId: message.Chat.Id,
                document: new InputFileStream(content: stream, fileName: "RESULTS.pdf"),
                cancellationToken: cancellationToken
            );
            
            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Надеемся всё хорошо \u2764\ufe0f",
                cancellationToken: cancellationToken);
            //await using FileStream stream = File.OpenRead(pathToFile);
            /*await botClient.SendDocumentAsync(
                chatId: message.Chat.Id,
                document: new InputFileStream(content: stream, fileName: "RESULTS.pdf"),
                cancellationToken: cancellationToken
            );*/
        }
        /*InlineKeyboardMarkup inlineKeyboard = new(
            new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Получить результаты", "Получение результатов"),
                    InlineKeyboardButton.WithCallbackData("Получить результаты", "Получение результатов"),
                    InlineKeyboardButton.WithCallbackData("Получить результаты", "Получение результатов"),
                    InlineKeyboardButton.WithCallbackData("Получить результаты", "Получение результатов"),
                    InlineKeyboardButton.WithCallbackData("Получить результаты", "Получение результатов"),
                    InlineKeyboardButton.WithCallbackData("Получить результаты", "Получение результатов"),
                    InlineKeyboardButton.WithCallbackData("Получить результаты", "Получение результатов"),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Показать возможности бота", "Функционал бота"),
                },
            });

        return await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: "Выберите следующее действие:",
            replyMarkup: inlineKeyboard,
            cancellationToken: cancellationToken);
            */
        
        
        /*return await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: "Введите свои данные в формате:\n" +
                  "Город сдачи анализа, фамилия имя отчество, дата рождения, номер заявки. Пример:\n" +
                  "нижний тагил, иванов иван иванович, 01.01.1921, 707375545",
            cancellationToken: cancellationToken);*/
    }
    
    static async Task<Message> SendAboutInformationAsync(ITelegramBotClient botClient, Message message,
        CancellationToken cancellationToken)
    {
        const string usage = "Функционал бота:\n" +
                             "/results - получить результаты анализов\n" +
                             "/author - получить информацию об авторе";
        
        return await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: usage,
            replyMarkup: new ReplyKeyboardRemove(),
            cancellationToken: cancellationToken);
    }

    static Task<Message> BackToListeningAsync(ITelegramBotClient botClient, Message message,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(new Message());
    }
}

Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
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

Task UnknownUpdateHandlerAsync(Update update, CancellationToken cancellationToken)
{
    return Task.CompletedTask;
}
#endregion