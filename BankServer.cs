using System.Net;
using System.Text;

class BankServer
{
    public BankServer()
    {
        listener = new HttpListener();
    }

    private HttpListener listener;
    private const int port = 3000;
    private const string loginPage = "PageTemplates/login.html";
    private const string cabinetPage = "PageTemplates/personalCabinet.html";
    private string url = $"http://localhost:{port}/login/";

    public async Task HandleIncomingConnections()
    {
        bool isServer = true;

        while (isServer)
        {
            HttpListenerContext listenCont = await listener.GetContextAsync();

            HttpListenerRequest req = listenCont.Request;
            HttpListenerResponse res = listenCont.Response;

            if ((req.HttpMethod == "POST") && (req.Url?.AbsolutePath == "/login"))
            {
                byte[] data = File.ReadAllBytes(cabinetPage);
                res.ContentType = "text/html";
                res.ContentEncoding = Encoding.UTF8;
                res.ContentLength64 = data.LongLength;

                await res.OutputStream.WriteAsync(data, 0, data.Length);

                res.Close();
            }
            else
            {
                byte[] data = File.ReadAllBytes(loginPage);
                res.ContentType = "text/html";
                res.ContentEncoding = Encoding.UTF8;
                res.ContentLength64 = data.LongLength;

                await res.OutputStream.WriteAsync(data, 0, data.Length);

                res.Close();
            }
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