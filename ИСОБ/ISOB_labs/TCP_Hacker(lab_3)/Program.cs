using TCP_Hacker_lab_3_;


CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
CancellationToken token = cancelTokenSource.Token;

Client client = new Client();
Server server = new Server();
Hacker hacker = new Hacker();

try
{
    Task.WaitAll([server.Listen(token),
        hacker.ConnectToServer(Server.ServerIP, token),
        client.ConnectToServer(Server.ServerIP, cancelTokenSource)]);

    Thread.Sleep(500);
}
catch (AggregateException ex) when (ex.InnerException is TaskCanceledException)
{ }