using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TCP_Hacker_lab_3_
{
    internal class Server
    {
        private static readonly IPEndPoint _ip = new(IPAddress.Parse("127.0.0.1"), 1000);

        public static IPEndPoint ServerIP { get => _ip; }

        public async Task Listen(CancellationToken token)
        {
            // Создаем объект Socket для прослушивания указанного IP адреса и порта
            Socket listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //Время ожиданий ответа равно 5 секундам
            //listenerSocket.ReceiveTimeout = 5000;

            try
            {
                // Привязываем сокет к конечной точке
                listenerSocket.Bind(_ip);
                // Начинаем прослушивание с максимальной длиной очереди подключений в 10
                listenerSocket.Listen(1);
                Console.WriteLine($"Сервер запущен на {_ip}");

                // Бесконечный цикл для прослушивания новых подключений
                while (true)
                {
                    // Принимаем новое подключение
                    Socket clientSocket = await listenerSocket.AcceptAsync(token);

                    // Создаем новый поток для обработки клиента
                    Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClient));
                    clientThread.Start(clientSocket);
                }
            }
            catch (OperationCanceledException)
            { }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }

        private async void HandleClient(object obj)
        {
            // Получаем объект Socket из параметра
            Socket clientSocket = (Socket)obj;
            //var port = (clientSocket.LocalEndPoint as IPEndPoint)!.Port;

            try
            {
                // Преобразуем данные в строку
                string message = ReadBlock(clientSocket).GetString();
                Console.WriteLine($"Получено сообщение от клиента: {message}");

                await Task.Delay(10000);
                // Отправляем ответ клиенту
                string responseMessage = "Сообщение получено успешно!";
                byte[] responseBuffer = responseMessage.GetBytes();
                clientSocket.Send(responseBuffer);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка обработки клиента: {ex.Message}");
                Console.WriteLine(ex.GetType());
            }
            finally
            {
                // Закрываем сокет при завершении работы с клиентом
                clientSocket.Close();
            }
        }


        private bool ClientIsConnected(out int windowSize)
        {


            windowSize = -1;
            return false;
        }

        private byte[] ReadBlock(Socket socket)
        {
            // Буфер для хранения данных
            byte[] messageBuffer = new byte[4096];
            List<byte> res = new();
            int bytesRead;

            // Читаем данные из клиента
            while (socket.Available > 0)
            {
                bytesRead = socket.Receive(messageBuffer);
                res.AddRange(messageBuffer[0..bytesRead]);
            }

            return res.ToArray();
        }
    }
}
