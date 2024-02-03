using System.Net.Sockets;
using System.Net;
using System.Text;
using Kerberos_lab_2_.Models;
using System.Text.Json;

namespace Kerberos_lab_2_.Servers
{
    //Будет выдавать TGS
    internal class TicketGrantingServer
    {
        private const string _key = "";
        public async Task Listen(CancellationToken token)
        {
            UdpClient udpClient = new(Configuration.TGServerEP);
            while (true)
            {
                if (token.IsCancellationRequested)
                    return;

                //Ожидаем и получаем сообщение
                var result = await udpClient.ReceiveAsync(token);
                string message = result.Buffer.GetJsonString();
                IPEndPoint endPoint = result.RemoteEndPoint;

                //Пытаемся его разобрать
                ResponseData<TGServerRequest>? tgsRequest
                    = JsonSerializer.Deserialize<ResponseData<TGServerRequest>>(message);

                //Если не получилось десериализовать
                if (tgsRequest is null)
                {
                    await Console.Out.WriteLineAsync("Сервер выдачи разрешений. Не получилось преобразовать полученный запрос");
                    continue;
                }

                TGServerRequest data = tgsRequest.Data!;
                
                string servicePrincipal = data.ServicePrincipal;
                Authenticator userAuth = JsonSerializer.Deserialize<Authenticator>(data.AuthEncryptBySessionKey.GetJsonString())!;
                //Расшифровываем tgt при помощи ключа kdc
                TicketGrantingTicket tgt = JsonSerializer.Deserialize<TicketGrantingTicket>(data.TGTEncryptByKDC.GetJsonString())!;
                
                if (tgt.TimeStamp.AddSeconds(tgt.Duration)  < DateTime.Now   //Если билет протух
                    || tgt.Principal != userAuth.Principal)                 //Если принципалы не совпадают
                {
                    byte[] notFound = JsonSerializer.Serialize(
                            new ResponseData<TGServerResponse>()
                            { IsSuccess = false, ErrorMessage = "Билет не действителен" }).GetBytes();

                    await udpClient.SendAsync(notFound, endPoint);
                    continue;
                }

                //Генерируем сессионый ключ сервиса(надо)

                ServiceTicket st = new("sessionservicekey", userAuth.Principal, Configuration.BaseDuration);

                byte[] stByServiceKey = JsonSerializer.Serialize(st).GetBytes();
                byte[] stBySessionKey = JsonSerializer.Serialize(st).GetBytes();

                TGServerResponse response = new(servicePrincipal, stBySessionKey, stByServiceKey);

                await udpClient.SendAsync(new ResponseData<TGServerResponse>() { Data = response, IsSuccess = true }.GetBytes(), endPoint);
            }
        }
    }
}
