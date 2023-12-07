namespace Citilab_Results_Telegram_Bot.Exceptions;

public class NullAccessTokenException : Exception
{
    public NullAccessTokenException()
    {
    }

    public NullAccessTokenException(string message)
        : base(message)
    {
    }

    public NullAccessTokenException(string message, Exception inner)
        : base(message, inner)
    {
    }
}