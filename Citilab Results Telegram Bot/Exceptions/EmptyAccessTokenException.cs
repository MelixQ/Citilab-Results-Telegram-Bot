namespace Citilab_Results_Telegram_Bot.Exceptions;

public class EmptyAccessTokenException : Exception
{
    public EmptyAccessTokenException()
    {
    }

    public EmptyAccessTokenException(string message)
        : base(message)
    {
    }

    public EmptyAccessTokenException(string message, Exception inner)
        : base(message, inner)
    {
    }
}