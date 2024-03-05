using Kerberos_lab_2_;
using Kerberos_lab_2_.Servers;
using System.Collections;

CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
CancellationToken token = cancelTokenSource.Token;

AuthenticationServer authenticationServer = new();
ClientServer clientServer = new();
TicketGrantingServer ticketGrantingServer = new();
ServiceServer serviceServer = new();
Thread.Sleep(1000);
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
