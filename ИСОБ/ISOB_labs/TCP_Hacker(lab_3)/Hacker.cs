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
        public async Task ConnectToServer(IPEndPoint clientIP, CancellationToken token)
        {
            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            await Task.Delay(1000);
            try
            {
                //SynFloodAttack();
                ResetAttack();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка со стороны хакера: {ex.Message}");
            }
            finally
            {
                if (clientSocket.Connected)
                    clientSocket.Close();
            }
        }

        private void ResetAttack()
        {
            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                clientSocket.Connect(Server.ServerIP);

                SendPacket(clientSocket, TCPPacket.GetEmptyPacket(4, Constans.ClientPort, Server.ServerIP.Port, 0, 0, syn: true));

                TCPPacket packet = ReadPacket(clientSocket);

                if (!packet.SYN || !packet.ACK)
                    throw new Exception("Некорректный первый пакет от сервера");

                SendPacket(clientSocket, TCPPacket.GetEmptyPacket(4, Constans.ClientPort, Server.ServerIP.Port, 1, 1, ack: true));

                //Подключение установлено. Теперь пытаемся разорвать соединение

                Parallel.For(0, 1000, (int i) =>
                {
                    try
                    {
                        //Отправляем пакеты с флагом RST и подставляем порт нашего клиента, типа это он отправил
                        SendPacket(clientSocket, TCPPacket.GetEmptyPacket(4, Constans.ClientPort, Server.ServerIP.Port, 0, 0, rst: true, ack: true));
                    }
                    catch { }
                });
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
        }

        //Просто спамим сообщениями о том, что хотим подключиться
        private void SynFloodAttack()
        {
            Parallel.For(5, 20, (int i) =>
            {
                try
                {
                    Socket socket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    socket.Connect(Server.ServerIP);

                    SendPacket(socket, TCPPacket.GetEmptyPacket(4, Constans.ClientPort, 1000 + i, 0, 0, syn: true));
                }
                catch { }
            });
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
            int bytesRead;

            // Читаем данные из клиента
            bytesRead = socket.Receive(messageBuffer);
            res.AddRange(messageBuffer[0..bytesRead]);

            return res.ToArray().GetTcpPacket();
        }
    }
}
