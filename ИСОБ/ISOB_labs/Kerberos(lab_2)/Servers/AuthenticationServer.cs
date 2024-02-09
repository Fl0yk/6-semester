using Kerberos_lab_2_.Models;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kerberos_lab_2_.Servers
{
    //Будет выдавать TGT
    internal class AuthenticationServer
    {
        private string _sessionKey = Configuration.SessionKey;
        private string _clientKey = Configuration.ClientKey;
        private string _kdcKey = Configuration.KDCKey;
        private List<string> _users = ["Miha"];

        /// <summary>
        /// Сервр аутентификации прослушивает на поучение запроса аутентификации пользователя
        /// и при успешных проверках отправляет TGT(два штука)
        /// </summary>
        public async Task Listen(CancellationToken token)
        {
            UdpClient udpClient = new(Configuration.AuthEP);

            while (true)
            {
                if (token.IsCancellationRequested)
                    return;
                
                //Ожидаем и получаем сообщение
                var result = await udpClient.ReceiveAsync(token);
                string message = result.Buffer.GetJsonString();
                IPEndPoint endPoint = result.RemoteEndPoint;

                //Пытаемся его разобрать
                ResponseData<AuthServerRequest>? authRequest 
                    = JsonSerializer.Deserialize<ResponseData<AuthServerRequest>>(message);

                //Если не получилось десериализовать
                if (authRequest is null)
                {
                    await Console.Out.WriteLineAsync("Сервер аутентификации. Не получилось преобразовать полученный запрос");
                    continue;
                }

                AuthServerRequest data = authRequest.Data!;

                //Если такого пользователя в типа бд не существует
                if (!_users.Contains(data.UserPrincipal))
                {
                    byte[] notFound = JsonSerializer.Serialize(
                            new ResponseData<AuthServerResponse>() 
                            { IsSuccess = false, ErrorMessage = "Пользователь не найден" }).GetBytes();

                    await udpClient.SendAsync(notFound, endPoint);
                    continue;
                }

                //Генерируем сессионный ключ
                string sessionKey = Configuration.SessionKey;

                //Формируем и отправляем ответ пользователю
                int duration = data.Duration < Configuration.BaseDuration
                                ? data.Duration : Configuration.BaseDuration;
                
                TicketGrantingTicket tgt = new(sessionKey, data.UserPrincipal, duration);
                //Получаем две строки зашифрованных тгт разными ключами(надо)
                //Т.е. не просто сериализуем тгт, а еще и шифруем байты(или строку)
                AuthServerResponse response = new(data.UserPrincipal,
                    JsonSerializer.Serialize(tgt).GetDesEncryptBytes(_clientKey),
                    JsonSerializer.Serialize(tgt).GetDesEncryptBytes(_kdcKey));

                await udpClient.SendAsync(new ResponseData<AuthServerResponse>() { Data = response, IsSuccess = true }.GetBytes(), endPoint);
            }
        }
    }
}
