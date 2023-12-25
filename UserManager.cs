using System.Text.Json;

public static class UserManager
{
    public static Random random = new Random();
    public static void SaveUsers(List<User> users, string path)
    {
        string data = JsonSerializer.Serialize(users.ToArray());
        File.WriteAllText(path, data);
    }

    public static List<User>? GetUsers(string path)
    {
        try
        {
            string data = File.ReadAllText(path);
            return JsonSerializer.Deserialize<List<User>>(data);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Получена ошибка при работе с файлом: {path}\nОшибка: {e.Message}");
            return null;
        }

    }

    public static string SetSession()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, random.Next(15, 30))
        .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    public static User? GetUserBySession(string sessionId, List<User> users)
    {
        string parseSession = sessionId.Replace("sessionid=", "");
        return users.FirstOrDefault(user => user.SessionId == parseSession);
    }


}