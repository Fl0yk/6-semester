using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TCP_Hacker_lab_3_
{
    internal class Client
    {
        private readonly IPEndPoint _ip = new(IPAddress.Parse("127.0.0.1"), Constans.ClientPort);

        public IPEndPoint ClientIP { get => _ip; }

        public Task ConnectToServer(IPEndPoint serverIP, CancellationTokenSource cancelTokenSource)
        {
            //Создаем сокет и привязывааем к конкретному адресу
            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clientSocket.Bind(_ip);

            try
            {
                //Подключаемся к серверу
                clientSocket.Connect(serverIP);


                //Наачинаем отправлять пакеты для подтверждения соединения
                SendPacket(clientSocket, TCPPacket.GetEmptyPacket(4, Constans.ClientPort, serverIP.Port, 0, 0, syn: true)); 
                
                TCPPacket packet = ReadPacket(clientSocket);

                if (!packet.SYN || !packet.ACK)
                    throw new Exception("Некорректный первый пакет от сервера");

                SendPacket(clientSocket, TCPPacket.GetEmptyPacket(4, Constans.ClientPort, serverIP.Port, 1, 1, ack: true));

                //Подключение установлено. Теперь читаем сообщение

                StringBuilder serverMessage = new();

                packet = ReadPacket(clientSocket);
                do
                {
                    serverMessage.Append(packet.Data.GetString());

                    Thread.Sleep(200);
                    SendPacket(clientSocket, TCPPacket.GetEmptyPacket(4, Constans.ClientPort, serverIP.Port, 1, packet.SequenceNumber + (uint)packet.Data.Length, ack: true));
                    packet = ReadPacket(clientSocket);
                }
                while (!packet.FIN);

                Console.WriteLine("\n======================================\n" +
                    "Сообщение от сервера получено: " + serverMessage.ToString() +
                    "\n======================================\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка у клиента при работе с сервером: {ex.Message}");
                //throw;
            }
            finally
            {
                if (clientSocket.Connected)
                    clientSocket.Close();
            }

            cancelTokenSource.Cancel();
            return Task.CompletedTask;
        }

        private void SendPacket(Socket socket, TCPPacket packet)
        {
            byte[] responseBuffer = packet.GetBytes();
            socket.Send(responseBuffer);
        }

        public TCPPacket ReadPacket(Socket socket)
        {
            // Буфер для хранения данных
            byte[] messageBuffer = new byte[4096];
            List<byte> res = new();

            // Читаем данные из клиента
            int bytesRead = socket.Receive(messageBuffer);
            res.AddRange(messageBuffer[0..bytesRead]);

            return res.ToArray().GetTcpPacket();
        }
    }
}
