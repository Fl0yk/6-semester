using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TCP_Hacker_lab_3_
{
    public class TCPPacket
    {
        public int SourcePort { get; set; }
        public int DestinationPort { get; set; }
        public uint SequenceNumber { get; set; }
        public uint AcknowledgmentNumber { get; set; }
        public bool ACK {  get; set; }
        public bool FIN {  get; set; }
        public bool RST { get; set; }
        public bool SYN {  get; set; }
        public ushort WindowSize { get; set; }
        public byte[] Data { get; set; } = [];

        public override string ToString()
        {
            return $"Порт источника: {SourcePort}\n" +
                $"Порт отправителя: {DestinationPort}\n" +
                $"Порядковый номер: {SequenceNumber}\n" +
                $"Номер подтверждния: {AcknowledgmentNumber}\n" +
                $"Размер окна: {WindowSize}\n" +
                $"Данные: {String.Join(' ', Data)}\n" +
                $"Флаги: ACK-{ACK}, FIN-{FIN}, RST-{RST}, SYN-{SYN}";
        }

        /// <summary>
        /// Метод для создания пустого пакета с конкретными флагами.
        /// Например, подтверждение получения, сброс соединения и т.к.
        /// </summary>
        public static TCPPacket GetEmptyPacket(ushort windowsSize, int sourcePort, int destinationPort, 
                                                uint seqNum, uint ackNum, 
                                                bool ack = false, bool syn = false, bool rst = false, bool fin = false)
        {
            return new TCPPacket()
            {
                SourcePort = sourcePort,
                DestinationPort = destinationPort,
                SequenceNumber = seqNum,
                AcknowledgmentNumber = ackNum,
                WindowSize = windowsSize,
                ACK = ack,
                RST = rst,
                SYN = syn,
                FIN = fin
            };
        }

        /// <summary>
        /// Преобразование набора в байт в массив пакетов, в заависимости от размера окна пользователя
        /// </summary>
        public static IEnumerable<TCPPacket> GetPackets(byte[] data, ushort windowsSize, int sourcePort, int destinationPort, uint seqNum, uint ackNum)
        {
            List<TCPPacket> packets = new();
            int id = 0;

            do
            {
                packets.Add(new TCPPacket()
                {
                    SourcePort = sourcePort,
                    DestinationPort = destinationPort,
                    SequenceNumber = seqNum,
                    AcknowledgmentNumber = ackNum,
                    WindowSize = windowsSize,
                    Data = data[id..(id + windowsSize < data.Length ? id + windowsSize : data.Length)],
                }) ;

                seqNum += windowsSize;
            }
            while ((id += windowsSize) < data.Length);

            //packets.Last().FIN = true;

            return packets;
        }
    }
}
