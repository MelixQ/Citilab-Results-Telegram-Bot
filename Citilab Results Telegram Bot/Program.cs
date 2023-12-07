using Citilab_Results_Telegram_Bot;
using Telegram.Bot;

var accessToken = AccessToken.Token;
var botClient = new TelegramBotClient(accessToken);

var me = await botClient.GetMeAsync();
Console.WriteLine($"Hello, World! I am user {me.Id} and my name is {me.FirstName}.");