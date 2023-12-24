using System.Net;
using System.Text;

class BankServer
{
    public BankServer()
    {
        listener = new HttpListener();
        user = new Dictionary<string, string>()
        {
            {"user1", "password1"},
            {"user2", "password2"},
            {"tim", "tim"}
        };
    }

    private HttpListener listener;
    private const int port = 3000;
    private const string loginPage = "PageTemplates/login.html";
    private const string cabinetPage = "PageTemplates/personalCabinet.html";
    private string baseUrl = $"http://localhost:{port}/";
    private string url = $"http://localhost:{port}/login/";
    private Dictionary<string, string> user;

    public async Task HandleIncomingConnections()
    {
        bool isServer = true;

        while (isServer)
        {
            HttpListenerContext listenCont = await listener.GetContextAsync();

            HttpListenerRequest req = listenCont.Request;
            HttpListenerResponse res = listenCont.Response;

            if ((req.HttpMethod == "GET") && (req.Url?.PathAndQuery == "/login/"))
            {
                responseHandler(res, loginPage);
                res.Close();
            }
            else if ((req.HttpMethod == "POST") && (req.Url?.PathAndQuery == "/login/"))
            {
                responseHandler(res, cabinetPage, "/cabinet");
                res.Close();
            }
            else if ((req.HttpMethod == "GET") && (req.Url?.PathAndQuery == "/cabinet"))
            {
                responseHandler(res, cabinetPage);
                res.Close();
            }
        }
    }

    private void WriteStream(HttpListenerResponse res, string page)
    {
        byte[] data = File.ReadAllBytes(page);
        res.ContentType = "text/html";
        res.ContentEncoding = Encoding.UTF8;
        res.ContentLength64 = data.LongLength;
        res.OutputStream.WriteAsync(data, 0, data.Length);
    }

    private void responseHandler(HttpListenerResponse res, string page, string? redirectUri = null)
    {
        if (redirectUri is not null)
        {
            res.Redirect(redirectUri);
        }
        else
        {
            WriteStream(res, page);
        }

    }

    public void StartServer()
    {
        listener.Prefixes.Add(url);
        listener.Start();
        Console.WriteLine($"Listening for connection on port = {port}");

        Task listenTask = HandleIncomingConnections();
        listenTask.GetAwaiter().GetResult();

        listener.Close();
    }
}