using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TCP_Hacker_lab_3_
{
    internal class Server
    {
        private static int _port = 1000;

        private static readonly IPEndPoint _ip = new(IPAddress.Parse("127.0.0.1"), _port);

        public static IPEndPoint ServerIP { get => _ip; }

        //Нужно для того, чтобы раазорвать соединение с конкретным клиентом
        //т.к. шарпы не позволят подключиться к серверу с аналогичными данными
        //ключ - порт
        private readonly Dictionary<int, Socket> _clients = [];

        public async Task Listen(CancellationToken token)
        {
            // Создаем объект Socket для прослушивания указанного IP адреса и порта
            Socket listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //Время ожиданий ответа равно 1 секунду
            listenerSocket.ReceiveTimeout = 1000;

            try
            {
                // Привязываем сокет к конечной точке
                listenerSocket.Bind(_ip);
                // Начинаем прослушивание с максимальной длиной очереди подключений в 10
                listenerSocket.Listen(10);
                Console.WriteLine($"Сервер запущен на {_ip}");

                // Бесконечный цикл для прослушивания новых подключений
                while (true)
                {
                    if (token.IsCancellationRequested)
                        break;
                    // Принимаем новое подключение
                    Socket clientSocket = await listenerSocket.AcceptAsync(token);
                    Console.WriteLine($"Подключается клиент: {clientSocket.RemoteEndPoint}");

                    //Добавляем клентов в словарь и для каждого запускаем свой обработчик
                    _clients.Add((clientSocket.RemoteEndPoint as IPEndPoint).Port, clientSocket);

                    Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClient));
                    clientThread.Start((clientSocket.RemoteEndPoint as IPEndPoint).Port);
                }
            }
            catch (OperationCanceledException)
            { }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }

        private void HandleClient(object obj)
        {
            int port = (int)obj;
            try
            {
                //Переменные, которые нам необходим для работы с клиентом
                uint ackNum, seqNum = 0;
                ushort winSize;

                ///////////////////////////////////////////////////////
                ///                 Подключение клиента             ///
                ///     Клиент --SYN;SEQ=0--> Сервер                ///
                ///     Сервер --SYN+ACK;SEQ=0;ACK=1--> Клиент      ///
                ///     Клиент --ACK;WinSize;SEQ=1;ACK=1--> Сервер  ///
                ///     (последнее - подтверждение подключения +    ///
                ///      сам запрос на сообщение от сервера)        ///
                ///////////////////////////////////////////////////////

                TCPPacket packet = ReadPacket(port);

                //Console.WriteLine($"\nСервер получил пакет на запрос для подключения: { packet}\n");

                if (!packet.SYN || packet.Data.Length != 0)
                    throw new Exception($"Некорректный первый пакет подключения от клиента {port}\n" + packet.ToString());

                ackNum = packet.SequenceNumber + 1;
                winSize = packet.WindowSize;

                SendPacket(port, TCPPacket.GetEmptyPacket(winSize, _port, port, seqNum, ackNum, syn: true, ack: true));
                //Console.WriteLine($"Сервер отправил пакет в качестве подтвержения подключения: {TCPPacket.GetEmptyPacket(winSize, _port, port, seqNum, ackNum, syn: true, ack: true)}\n");

                packet = ReadPacket(port);
                //Console.WriteLine($"Сервер получил пакет, подтверждающий подключение: {packet}\n");

                if (!packet.ACK || packet.Data.Length != 0)
                    throw new Exception("Некорректный второй пакет подключения от клиента");

                Console.WriteLine("\nКлиент успешно подключися. Начиентся отправление сообщения\n");

                ///////////////////////////////////////////////////////
                ///                 Получение сообщения             ///
                ///     Сервер --SEQ=1;ACK=1;data(4 б)--> Клиент    ///
                ///     Клиент --ACK;SEQ=1;ACK=5--> Сервер          ///
                ///     Сервер --SEQ=5;ACK=1;data(4 б)--> Клиент    ///
                ///     ........................................    ///
                ///     Сервер --FIN; SEQ=n;ACK=1--> Клиент         ///
                ///     (последнее - подтверждение, что отправили + ///
                ///      завершение соединения)                     ///
                ///////////////////////////////////////////////////////

                StringBuilder str = new();

                for (int i = 0; i < 10; i++)
                    str.Append(i + ", ");

                List<TCPPacket> packets = TCPPacket.GetPackets(str.ToString().GetBytes(), winSize, _port, port, seqNum, ackNum).ToList();

                foreach (var pack in packets)
                {
                    Thread.Sleep(10);
                    SendPacket(port, pack);
                    //Console.WriteLine($"Сервер отправил клиенту данный пакет: {pack}\n");

                    packet = ReadPacket(port);

                    if (!packet.ACK || packet.Data.Length != 0)
                        throw new Exception("Некорректный пакет подтверждения получения пакета от клиента");

                    if (packet.RST)
                    {
                        Console.WriteLine("Экстренное завершение соединения с клиентом " + packet.SourcePort + $"\nБыл получен пакет: {packet}");
                        _clients[packet.SourcePort].Close();
                        return;
                    }

                    if (pack.SequenceNumber + winSize != packet.AcknowledgmentNumber)
                        throw new Exception("Клиент подтвердил получение не тех данных");
                }

                SendPacket(port, TCPPacket.GetEmptyPacket(winSize, _port, port, seqNum, ackNum, fin: true));

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка обработки клиента {port}: {ex.Message}");
                //throw;
            }
            finally
            {
                // Закрываем сокет при завершении работы с клиентом
                if(_clients[port].Connected)
                    _clients[port].Close();
            }
        }

        private void SendPacket(int port, TCPPacket packet)
        {
            byte[] responseBuffer = packet.GetBytes();
            _clients[port].Send(responseBuffer);
        }

        public TCPPacket ReadPacket(int port)
        {
            // Буфер для хранения данных
            byte[] messageBuffer = new byte[4096];
            List<byte> res = new();

            // Читаем данные из клиента
            int bytesRead = _clients[port].Receive(messageBuffer);
            res.AddRange(messageBuffer[0..bytesRead]);

            return res.ToArray().GetTcpPacket();
        }
    }
}
