using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.Web;
using Microsoft.VisualBasic;

class BankServer
{
    public BankServer()
    {
        listener = new HttpListener();
        users = new List<User>() {
            new User(UserManager.SetSession(), "tim", "123", 2033),
            new User(UserManager.SetSession(), "carl", "1234", 1),
            new User(UserManager.SetSession(), "alex", "hoenn", 400),
            new User(UserManager.SetSession(), "jesus", "god", 99999999),
        };
    }
    private HttpListener listener;
    private const int port = 3000;
    private const string loginPage = "PageTemplates/login.html";
    private const string cabinetPage = "PageTemplates/personalCabinet.html";
    private string baseUrl = $"http://localhost:{port}/";
    private List<User> users;
    private const string filePath = "./users.json";
    public async Task HandleIncomingConnections()
    {
        bool isServer = true;
        UserManager.SaveUsers(users, filePath);

        while (isServer)
        {
            HttpListenerContext listenCont = await listener.GetContextAsync();

            HttpListenerRequest req = listenCont.Request;
            HttpListenerResponse res = listenCont.Response;

            if ((req.HttpMethod == "GET") && (req.Url?.PathAndQuery == "/login/"))
            {
                if (req.Cookies.Count > 0)
                {
                    var sessionUser = UserManager.GetUserBySession(getSession(req), users);
                    if (sessionUser is not null)
                    {
                        Console.WriteLine("Клиент зашел по сессии");
                        res.Redirect("/cabinet/");
                        res.Close();
                        continue;
                    }
                }


                responseHandler(res, req, loginPage);
                res.Close();
            }
            else if ((req.HttpMethod == "POST") && (req.Url?.PathAndQuery == "/login/"))
            {
                var body = getBodyFromForm(req);
                if (body["login"] == "" && body["password"] == "")
                {
                    res.Redirect("/login/");
                    res.Close();
                    continue;
                }

                string login = body["login"];
                string password = body["password"];
                var user = users.Find(user => user.Login == login && user.Password == password);
                if (user is null)
                {
                    res.Redirect("/login/");
                    res.Close();
                    continue;
                }

                var cookieDate = DateTime.UtcNow.AddMinutes(60d).ToString("dddd, dd-MM-yyyy hh:mm:ss GMT");
                res.Headers.Add("Set-Cookie", $"sessionid={user.SessionId};Path=/;Expires=" + cookieDate + " GMT");
                res.Redirect("/cabinet/");
                res.Close();
            }
            else if ((req.HttpMethod == "GET") && (req.Url?.PathAndQuery == "/cabinet/"))
            {
                var page = File.ReadAllText(cabinetPage).ToString();
                var sessionUser = UserManager.GetUserBySession(getSession(req), users);
                if (sessionUser is null)
                {
                    res.Redirect("/login/");
                    res.Close();
                    continue;
                }
                byte[] data = Encoding.UTF8.GetBytes(String.Format(page, sessionUser.Balance));
                res.ContentType = "text/html";
                res.ContentEncoding = Encoding.UTF8;
                res.ContentLength64 = data.LongLength;
                res.OutputStream.WriteAsync(data, 0, data.Length);
                res.Close();
            }
            else
            {
                res.Redirect("/login/");
                res.Close();
            }
        }
    }

    private string? getSession(HttpListenerRequest req) => req.Cookies.ToList()?.FirstOrDefault()?.ToString();

    private NameValueCollection getBodyFromForm(HttpListenerRequest req)
    {
        Stream body = req.InputStream;
        Encoding encoding = req.ContentEncoding;
        StreamReader reader = new StreamReader(body, encoding);
        return HttpUtility.ParseQueryString(reader.ReadToEnd());
    }

    private void WriteStream(HttpListenerResponse res, string page)
    {
        byte[] data = File.ReadAllBytes(page);
        res.ContentType = "text/html";
        res.ContentEncoding = Encoding.UTF8;
        res.ContentLength64 = data.LongLength;
        res.OutputStream.WriteAsync(data, 0, data.Length);
    }

    private void responseHandler(HttpListenerResponse res, HttpListenerRequest req, string page)
    {
        WriteStream(res, page);
        log(req);
    }

    private void log(HttpListenerRequest request)
    {
        Console.WriteLine("############### Логи ##################");
        Console.WriteLine($"адрес приложения: {request.LocalEndPoint}");
        Console.WriteLine($"адрес клиента: {request.RemoteEndPoint}");
        Console.WriteLine(request.RawUrl);
        Console.WriteLine($"Запрошен адрес: {request.Url}");
        Console.WriteLine("Выполнено");
        Console.WriteLine("#######################################");
    }

    public void StartServer()
    {
        listener.Prefixes.Add(baseUrl);
        listener.Start();
        Console.WriteLine($"Listening for connection on port = {port}");

        Task listenTask = HandleIncomingConnections();
        listenTask.GetAwaiter().GetResult();

        listener.Close();
    }
}