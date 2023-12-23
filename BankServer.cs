using System.Net;

class BankServer
{
    public BankServer()
    {
        listener = new HttpListener();
    }

    public HttpListener listener;
    public const int port = 3000;
    public string url = $"http://localhost:{port}/";

    public async Task HandleIncomingConnections()
    {
        bool isServer = true;

        while (isServer)
        {
            HttpListenerContext listenCont = await listener.GetContextAsync();

            HttpListenerRequest req = listenCont.Request;
            HttpListenerResponse res = listenCont.Response;


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