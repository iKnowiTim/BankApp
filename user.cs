using System.Text.Json.Serialization;

public class User
{
    public User(string sessionId, string login, string password, int balance)
    {
        SessionId = sessionId;
        Login = login;
        Password = password;
        Balance = balance;
    }
    [JsonPropertyName("sessionId")]
    public string SessionId { get; set; }
    [JsonPropertyName("login")]
    public string Login { get; set; }
    [JsonPropertyName("password")]
    public string Password { get; set; }
    [JsonPropertyName("balance")]
    public int Balance { get; set; }


    public int? GetBalance(string login, string password)
    {
        if (Login == login && Password == password)
        {
            return Balance;
        }
        else return null;
    }
}