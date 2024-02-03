using System.Net.Sockets;
using System.Net;
using Kerberos_lab_2_.Models;
using System.Text.Json;

namespace Kerberos_lab_2_.Servers
{
    internal class ServiceServer
    {
        private string _message = "Hello!";

        public async Task Listen(CancellationToken token)
        {
            UdpClient udpClient = new(Configuration.ServiceServerEP);
            while (true)
            {
                if (token.IsCancellationRequested)
                    return;

                //Ожидаем и получаем сообщение
                var result = await udpClient.ReceiveAsync(token);
                string message = result.Buffer.GetJsonString();
                IPEndPoint endPoint = result.RemoteEndPoint;

                //Пытаемся его разобрать
                ResponseData<AppServerRequest>? appRequest
                    = JsonSerializer.Deserialize<ResponseData<AppServerRequest>>(message);

                //Если не получилось десериализовать
                if (appRequest is null)
                {
                    await Console.Out.WriteLineAsync("Сервер сервиса. Не получилось преобразовать полученный запрос");
                    continue;
                }

                AppServerRequest data = appRequest.Data!;

                //Расшифровываем tgt при помощи ключа kdc
                TicketGrantingTicket tgs = JsonSerializer.Deserialize<TicketGrantingTicket>(data.TGSEncryptByServiceKey.GetJsonString())!;
                string serviceSessionKey = tgs.SessionKey;

                Authenticator userAuth = JsonSerializer.Deserialize<Authenticator>(data.AuthEncryptBySessionServiceKey.GetJsonString())!;
                

                if (tgs.TimeStamp.AddSeconds(tgs.Duration) < DateTime.Now   //Если билет протух
                    || tgs.Principal != userAuth.Principal)                 //Если принципалы не совпадают
                {
                    byte[] notFound = JsonSerializer.Serialize(
                            new ResponseData<AppServerResponse>()
                            { IsSuccess = false, ErrorMessage = "Билет не действителен" }).GetBytes();

                    await udpClient.SendAsync(notFound, endPoint);
                    continue;
                }

                AppServerResponse response = new(JsonSerializer.Serialize(_message).GetBytes());

                await udpClient.SendAsync(new ResponseData<AppServerResponse>() { Data = response, IsSuccess = true }.GetBytes(), endPoint);
            }
        }
    }
}
