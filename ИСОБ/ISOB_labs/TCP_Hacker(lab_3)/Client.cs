using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TCP_Hacker_lab_3_
{
    internal class Client
    {
        private readonly IPEndPoint _ip = new(IPAddress.Parse("127.0.0.1"), 1001);

        public IPEndPoint ClientIP { get => _ip; }

        public Task ConnectToServer(IPEndPoint serverIP, CancellationTokenSource cancelTokenSource)
        {
            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                clientSocket.Connect(serverIP);
                Console.WriteLine($"Подключено к серверу {serverIP}");
  
                // Отправляем сообщение серверу
                string message = "Привет, сервер!";
                byte[] messageBuffer = message.GetBytes();
                clientSocket.Send(messageBuffer);
                Console.WriteLine($"Отправлено сообщение серверу: {message}");
                //Thread.Sleep(2000);
                // Читаем ответ от сервера
                byte[] responseBuffer = new byte[4096];
                int bytesRead = clientSocket.Receive(responseBuffer);
                string responseMessage = responseBuffer.GetString(bytesRead);
                Console.WriteLine($"Ответ от сервера: {responseMessage}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при работе с сервером: {ex.Message}");
            }
            finally
            {
                clientSocket.Close();
            }

            cancelTokenSource.Cancel();
            return Task.CompletedTask;
        }
    }
}
