using Kerberos_lab_2_.Models;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kerberos_lab_2_.Servers
{
    internal class ClientServer
    {
        private const string _key = "";
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
            TicketGrantingTicket tgt = JsonSerializer.Deserialize<TicketGrantingTicket>(Encoding.UTF8.GetString(authResponse.Data.TGSEncryptByClientKey))!;
            string sessionKey = tgt.SessionKey;

            //////////////////////////////////////////////////////////////////
            //  Отправляем запрос на получение разрешения доступа к сервису //
            //////////////////////////////////////////////////////////////////
            Authenticator authenticator = new(_login);

            //Шифруем аутентификатор сессионным ключем
            byte[] encryptAuth = JsonSerializer.Serialize(authenticator).GetBytes();

            TGServerRequest tgsRequest = new("service 1", tgtByKDCkey, encryptAuth);
            await udpClient.SendAsync(new ResponseData<TGServerRequest>() { Data = tgsRequest, IsSuccess = true }.GetBytes(), Configuration.TGServerEP);
            
            response = await udpClient.ReceiveAsync();
            
            ResponseData<TGServerResponse>? tgsResponse
                = JsonSerializer.Deserialize<ResponseData<TGServerResponse>>(Encoding.UTF8.GetString(response.Buffer));

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
            ServiceTicket st = JsonSerializer.Deserialize<ServiceTicket>(tgsResponse.Data!.STEncryptBySessionKey.GetJsonString())!;
            string sessinServiceKey = st.ServiceSessionKey;
            byte[] encryptST = tgsResponse.Data!.STEncryptByServiceKey;


            /////////////////////////////////////////////////////
            //  Отправляем запрос сервису на получение данных  //
            /////////////////////////////////////////////////////

            //Шифруем сессионным ключем сервиса
            encryptAuth = JsonSerializer.Serialize(new Authenticator(_login)).GetBytes();
            AppServerRequest serviceRequest = new(encryptAuth, encryptST);

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
            string message = JsonSerializer.Deserialize<string>(appResponse.Data!.ServiceResEncryptByServiceSessionKey.GetJsonString())!;
            await Console.Out.WriteLineAsync(message);

            cancelTokenSource.Cancel();
        }
    }
}
