using Avro.Specific;
using Avro;

namespace UserEvents.Infra;

public class UserCreatedEvent : ISpecificRecord
{
    private static readonly Schema _schema = Schema.Parse(@"
    {
        ""type"": ""record"",
        ""name"": ""UserCreatedEvent"",
        ""fields"": [
            {""name"": ""UserId"", ""type"": ""string""},
            {""name"": ""UserName"", ""type"": ""string""},
            {""name"": ""UserEmail"", ""type"": ""string""},
            {""name"": ""CreatedAt"", ""type"": ""long""}
        ]
    }");

    public string UserId { get; set; }
    public string UserName { get; set; }
    public string UserEmail { get; set; }
    public long CreatedAt { get; set; }

    public UserCreatedEvent() { }

    public UserCreatedEvent(string userId, string userName, string userEmail, long createdAt)
    {
        UserId = userId;
        UserName = userName;
        UserEmail = userEmail;
        CreatedAt = createdAt;
    }

    public global::Avro.Schema Schema => _schema;

    public object Get(int fieldPos)
    {
        return fieldPos switch
        {
            0 => UserId,
            1 => UserName,
            2 => UserEmail,
            3 => CreatedAt,
            _ => throw new Avro.AvroRuntimeException("Bad index " + fieldPos + " in Get()")
        };
    }

    public void Put(int fieldPos, object fieldValue)
    {
        switch (fieldPos)
        {
            case 0: UserId = (string)fieldValue; break;
            case 1: UserName = (string)fieldValue; break;
            case 2: UserEmail = (string)fieldValue; break;
            case 3: CreatedAt = (long)fieldValue; break;
            default: throw new Avro.AvroRuntimeException("Bad index " + fieldPos + " in Put()");
        }
    }
}