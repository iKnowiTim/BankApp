using System.Net;
using System.Text;

class BankServer
{
    public BankServer()
    {
        listener = new HttpListener();
    }

    public HttpListener listener;
    public const int port = 3000;
    private const string loginPage = "PageTemplates/login.html";
    public string url = $"http://localhost:{port}/";

    public async Task HandleIncomingConnections()
    {
        bool isServer = true;

        while (isServer)
        {
            HttpListenerContext listenCont = await listener.GetContextAsync();

            HttpListenerRequest req = listenCont.Request;
            HttpListenerResponse res = listenCont.Response;

            if ((req.HttpMethod == "GET") && (req.Url?.AbsolutePath == "/login"))
            {
                Console.WriteLine("Hello");
            }

            byte[] data = File.ReadAllBytes(loginPage);
            res.ContentType = "text/html";
            res.ContentEncoding = Encoding.UTF8;
            res.ContentLength64 = data.LongLength;

            await res.OutputStream.WriteAsync(data, 0, data.Length);

            res.Close();
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