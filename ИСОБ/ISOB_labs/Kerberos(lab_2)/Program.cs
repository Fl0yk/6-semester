using Kerberos_lab_2_;
using Kerberos_lab_2_.Servers;
using System.Collections;

//string key = "fvhtracfvbjtrxdj";
//string message = "aaaaaaaaaa bbbb rrr fdgdfg sedfsdf seeeeeed gghgh 345435 fhrgh t45fg hefg wt434 rdfd";

//var tmp = AlgorithmDES.Encrypt(message.GetBytes(), key.GetBytes());
//Console.WriteLine(tmp.GetJsonString());
//Console.WriteLine("=========================");
//Console.WriteLine(AlgorithmDES.Decrypt(tmp, key.GetBytes()).GetJsonString());


//return;

CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
CancellationToken token = cancelTokenSource.Token;

AuthenticationServer authenticationServer = new();
ClientServer clientServer = new();
TicketGrantingServer ticketGrantingServer = new();
ServiceServer serviceServer = new();

try
{
    Task.WaitAll([authenticationServer.Listen(token), 
        ticketGrantingServer.Listen(token), 
        serviceServer.Listen(token), 
        clientServer.Listen(cancelTokenSource)]);

    Thread.Sleep(1000);
}
catch(AggregateException ex) when (ex.InnerException is TaskCanceledException)
{ }
