namespace Citilab_Results_Telegram_Bot.TestClientData;

public record ClientBaseRecord
{
    public string Name { get; init; }
    public string LastName { get; init; }
    public string Surname { get; init; }
    public string BirthDay { get; init; }
    public string BirthMonth { get; init; }
    public string BirthYear { get; init; }
    public string RequestId { get; set; }
    public string Lab { get; set; }

    public ClientBaseRecord(string name, string lastName, string surname, 
        string birthDay, string birthMonth, string birthYear, string requestId, string lab)
    {
        Name = name;
        LastName = lastName;
        Surname = surname;
        BirthDay = birthDay;
        BirthMonth = birthMonth;
        BirthYear = birthYear;
        RequestId = requestId;
        Lab = lab;
    }
}