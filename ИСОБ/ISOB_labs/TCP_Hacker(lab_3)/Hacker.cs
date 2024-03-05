using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TCP_Hacker_lab_3_
{
    internal class Hacker
    {
        private readonly IPEndPoint _ip = new(IPAddress.Parse("127.0.0.1"), 1001);

        public async Task ConnectToServer(IPEndPoint clientIP, CancellationToken token)
        {
            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            await Task.Delay(1000);
            try
            {
                clientSocket.Connect(clientIP);
                Console.WriteLine($"Хакер подключен к серверу {clientIP}");

                //// Отправляем сообщение серверу
                //string message = "Привет от хакера!";
                //byte[] messageBuffer = message.GetBytes();
                ////await clientSocket.SendAsync(messageBuffer, token);
                //await clientSocket.SendToAsync(messageBuffer, clientIP, token);
                //Console.WriteLine($"Отправлено сообщение клиенту: {message}");

                byte[] responseBuffer = new byte[4096];
                int bytesRead = clientSocket.Receive(responseBuffer);
                string responseMessage = responseBuffer.GetString(bytesRead);
                Console.WriteLine($"Ответ от сервера хакеру: {responseMessage}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка со стороны хакера: {ex.Message}");
            }
            finally
            {
                clientSocket.Close();
            }
        }
    }
}
