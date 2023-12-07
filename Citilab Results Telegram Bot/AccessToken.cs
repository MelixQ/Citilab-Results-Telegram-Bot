using Citilab_Results_Telegram_Bot.Exceptions;
using Newtonsoft.Json;

namespace Citilab_Results_Telegram_Bot;

public static class AccessToken
{
    public static string Token { get; private set; } = GetAccessToken();
    
    private static string GetAccessToken()
    {
        var token = "";
        
        var rawJson = File.ReadAllText("accessInformation.json");
        var jsonTextReader = new JsonTextReader(new StringReader(rawJson));
        
        while (jsonTextReader.Read())
            if (jsonTextReader.TokenType is JsonToken.String && jsonTextReader.Value != null)
                token = jsonTextReader.Value.ToString();

        switch (token)
        {
            case "":
                throw new EmptyAccessTokenException("Access token came empty after reading credentials json");
            case null:
                throw new NullAccessTokenException("Access token is null after reading credentials json");
        }

        return token;
    }
}