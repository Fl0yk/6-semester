using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        public bool RST { get; set; }
        public bool SYN {  get; set; }
        public ushort WindowSize { get; set; }
        public byte[] Data { get; set; } = [];
    }

    public class PacketsService
    {
        private int _sourcePort { get; set; }
        private int _destinationPort { get; set; }
        private uint _seqNumber { get; set; }
        private uint _ackNumber { get; set; }

        public PacketsService(int sourcePort, int destinationPort, 
                        uint seqNumber, uint ackNumber) 
        {
            _sourcePort = sourcePort;
            _destinationPort = destinationPort;
            _seqNumber = seqNumber;
            _ackNumber = ackNumber;
        }


        public IEnumerable<TCPPacket> GetPackets(byte[] data, ushort windowsSize)
        {
            List<TCPPacket> packets = new();
            int size = data.Length - 1;

            do
            {
                packets.Add(new TCPPacket()
                {
                    SourcePort = _sourcePort,
                    DestinationPort = _destinationPort,

                });
            }
            while (size - windowsSize > 0);            

            return packets;
        }
    }
}
