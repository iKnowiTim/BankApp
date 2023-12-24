class User
{
    public User(string login, string password, int balance)
    {
        Login = login;
        Password = password;
        Balance = balance;
    }
    public string Login { get; set; }
    public string Password { get; set; }
    public int Balance { get; set; }
}