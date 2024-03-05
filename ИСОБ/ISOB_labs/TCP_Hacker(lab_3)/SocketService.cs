using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TCP_Hacker_lab_3_
{
    public class SocketService
    {
        private readonly Socket _socket;

        public SocketService(Socket socket) 
        {
            _socket = socket;
        }

        public byte[] ReadMessage()
        {
            throw new NotImplementedException();
        }

        private byte[] ReadBlock()
        {
            // Буфер для хранения данных
            byte[] messageBuffer = new byte[4096];
            List<byte> res = new();
            int bytesRead;

            // Читаем данные из клиента
            while (_socket.Available > 0)
            {
                bytesRead = _socket.Receive(messageBuffer);
                res.AddRange(messageBuffer[0..bytesRead]);
            }

            return res.ToArray();
        }
    }
}
