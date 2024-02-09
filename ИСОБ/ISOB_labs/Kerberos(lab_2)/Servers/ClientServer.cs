using Kerberos_lab_2_.Models;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kerberos_lab_2_.Servers
{
    internal class ClientServer
    {
        private const string _key = Configuration.ClientKey;
        private string _login = "Miha";
        public async Task Listen(CancellationTokenSource cancelTokenSource)
        {
            UdpClient udpClient = new(Configuration.ClientPort);

            ////////////////////////////////////////////
            //  Отправляем запрос для аутентификации  //
            ////////////////////////////////////////////
            AuthServerRequest authData = new(_login, 300);
            ResponseData<AuthServerRequest> authRequest = new() { Data = authData, IsSuccess = true };

            byte[] data = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(authRequest));

            await udpClient.SendAsync(data, Configuration.AuthEP);
            //await Task.Delay(2000);
            await Console.Out.WriteLineAsync("==================================\n" +
                "Клиент отправил запрос на аутентификацию с принипалом Miha и временем жизни 5 минут");
            var response = await udpClient.ReceiveAsync();
            
            ResponseData<AuthServerResponse>? authResponse 
                = JsonSerializer.Deserialize<ResponseData<AuthServerResponse>>(Encoding.UTF8.GetString(response.Buffer));

            if(authResponse is null)
            {
                cancelTokenSource.Cancel();
                await Console.Out.WriteLineAsync("Клиент. Ошибка при получении ответа");
                return;
            }

            if(!authResponse.IsSuccess)
            {
                cancelTokenSource.Cancel();
                await Console.Out.WriteLineAsync("Клиент. Неуспешный ответ при аутентификации: " + authResponse.ErrorMessage);
                return;
            }

            //////////////////////////////////////////
            //    Удачный ответ на аутентификацию   //
            //////////////////////////////////////////
            byte[] tgtByKDCkey = authResponse.Data!.TGSEncryptByKDCKey;
            TicketGrantingTicket tgt = JsonSerializer.Deserialize<TicketGrantingTicket>(authResponse.Data.TGSEncryptByClientKey.GetJsonString(_key))!;
            string sessionKey = tgt.SessionKey;

            await Console.Out.WriteLineAsync("\n===================================\n" +
                "Клиент получил ответ от сервера аутентификации." +
                "\nTGT билет, зашифрованный ключем KDC: " + tgtByKDCkey.GetJsonString() +
                "\nСессионный ключ: " + sessionKey);

            //////////////////////////////////////////////////////////////////
            //  Отправляем запрос на получение разрешения доступа к сервису //
            //////////////////////////////////////////////////////////////////
            Authenticator authenticator = new(_login);

            //Шифруем аутентификатор сессионным ключем
            byte[] encryptAuth = JsonSerializer.Serialize(authenticator).GetDesEncryptBytes(sessionKey);

            await Console.Out.WriteLineAsync("\n===================================\n" +
                "Клиент отправляет запрос на разрешение доступа к сервису (TGS)." +
                "\nЗашифрованный аутентификатор: " + encryptAuth.GetJsonString() +
                "\nПринципал сервиса: service 1");
            TGServerRequest tgsRequest = new("service 1", tgtByKDCkey, encryptAuth);
            await udpClient.SendAsync(new ResponseData<TGServerRequest>() { Data = tgsRequest, IsSuccess = true }.GetBytes(), Configuration.TGServerEP);
            
            response = await udpClient.ReceiveAsync();
            ResponseData<TGServerResponse>? tgsResponse
                = JsonSerializer.Deserialize<ResponseData<TGServerResponse>>(response.Buffer.GetJsonString());

            if (tgsResponse is null)
            {
                cancelTokenSource.Cancel();
                await Console.Out.WriteLineAsync("Клиент. Ошибка при получении ответа");
                return;
            }

            if (!tgsResponse.IsSuccess)
            {
                cancelTokenSource.Cancel();
                await Console.Out.WriteLineAsync("Клиент. Неуспешный ответ при аутентификации: " + tgsResponse.ErrorMessage);
                return;
            }
            //////////////////////////////////////////
            //          Удачный ответ от TGS        //
            //////////////////////////////////////////
            ServiceTicket st = JsonSerializer.Deserialize<ServiceTicket>(tgsResponse.Data!.STEncryptBySessionKey.GetJsonString(sessionKey))!;
            string sessinServiceKey = st.ServiceSessionKey;
            byte[] encryptST = tgsResponse.Data!.STEncryptByServiceKey;

            await Console.Out.WriteLineAsync("\n===================================\n" +
                "Клиент получил ответ от TGS." +
                "\nST билет, зашифрованный ключем сервиса: " + encryptST.GetJsonString() +
                "\nСессионный ключ сервиса: " + sessinServiceKey);

            /////////////////////////////////////////////////////
            //  Отправляем запрос сервису на получение данных  //
            /////////////////////////////////////////////////////

            //Шифруем сессионным ключем сервиса
            encryptAuth = JsonSerializer.Serialize(new Authenticator(_login)).GetDesEncryptBytes(sessinServiceKey);
            AppServerRequest serviceRequest = new(encryptAuth, encryptST);

            await Console.Out.WriteLineAsync("\n===================================\n" +
                "Клиент отправляет запрос на разрешение доступа к сервису (TGS)." +
                "\nЗашифрованный аутентификатор: " + encryptAuth.GetJsonString() +
                "\nЗашифрованный ST: " + encryptST.GetJsonString());

            await udpClient.SendAsync(new ResponseData<AppServerRequest>() { Data = serviceRequest, IsSuccess = true}.GetBytes(), Configuration.ServiceServerEP);
            response = await udpClient.ReceiveAsync();

            ResponseData<AppServerResponse>? appResponse = JsonSerializer.Deserialize<ResponseData<AppServerResponse>>(response.Buffer);

            if (appResponse is null)
            {
                cancelTokenSource.Cancel();
                await Console.Out.WriteLineAsync("Клиент. Ошибка при получении ответа от сервиса");
                return;
            }

            if (!appResponse.IsSuccess)
            {
                cancelTokenSource.Cancel();
                await Console.Out.WriteLineAsync("Клиент. Неуспешный ответ от сервиса: " + appResponse.ErrorMessage);
                return;
            }

            //////////////////////////////////////////
            //       Удачный ответ от сервиса       //
            //////////////////////////////////////////
            string message = JsonSerializer.Deserialize<string>(appResponse.Data!.ServiceResEncryptByServiceSessionKey.GetJsonString(sessinServiceKey))!;
            await Console.Out.WriteLineAsync("\n===================================\n" +
                "Клиент получил ответ от сервиса." +
                "\nПолученное сообщение: " + message);
            cancelTokenSource.Cancel();
        }
    }
}
