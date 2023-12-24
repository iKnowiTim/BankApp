using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.Web;

class BankServer
{
    public BankServer()
    {
        listener = new HttpListener();
        user = new Dictionary<string, string>() {
            {"tim", "123"}
        };
    }

    private HttpListener listener;
    private const int port = 3000;
    private const string loginPage = "PageTemplates/login.html";
    private const string cabinetPage = "PageTemplates/personalCabinet.html";
    private string baseUrl = $"http://localhost:{port}/";
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
                responseHandler(res, req, loginPage);
                res.Close();
            }
            else if ((req.HttpMethod == "POST") && (req.Url?.PathAndQuery == "/login/"))
            {
                var body = getBodyFromForm(req);
                if (body["login"] == "" || body["password"] == "")
                {
                    responseHandler(res, req, loginPage);
                    res.Close();
                    continue;
                }
                try
                {
                    string userPassword = user[body["login"]];
                    if (userPassword != body["password"])
                        throw new Exception();
                }
                catch
                {
                    res.StatusCode = 400;
                    responseHandler(res, req, loginPage);
                    continue;
                }

                res.Redirect("/cabinet/");
                res.Close();
            }
            else if ((req.HttpMethod == "GET") && (req.Url?.PathAndQuery == "/cabinet/"))
            {
                responseHandler(res, req, cabinetPage);
                res.Close();
            }
        }
    }

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
        Console.WriteLine($"адрес приложения: {request.LocalEndPoint}");
        Console.WriteLine($"адрес клиента: {request.RemoteEndPoint}");
        Console.WriteLine(request.RawUrl);
        Console.WriteLine($"Запрошен адрес: {request.Url}");
        Console.WriteLine("Выполнено");
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