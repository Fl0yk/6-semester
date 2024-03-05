using PacketDotNet;
using SharpPcap;
using SharpPcap.LibPcap;
using System.Net.Sockets;
using TCP_Hacker_lab_3_;

// Задаем фильтр
string filter = "ip and udp and ((src host 127.0.0.1 and src port 1001) or (dst host 127.0.0.1 and dst port 1001) or (src host 127.0.0.1 and src port 1002) or (dst host 127.0.0.1 and dst port 1002))";

// Получаем доступные сетевые устройства
CaptureDeviceList devices = CaptureDeviceList.Instance;

// Выбираем сетевое устройство (может потребоваться права администратора)
LibPcapLiveDevice device = devices[3] as LibPcapLiveDevice;

// Открываем устройство для захвата пакетов
device.Open(DeviceModes.Promiscuous, 1000);

// Устанавливаем фильтр
device.Filter = filter;

// Устанавливаем обработчик событий для каждого захваченного пакета
device.OnPacketArrival += (sender, e) =>
{
    // Обработка пакета
    var packet = e.GetPacket().GetPacket();
    var tcpPacket = packet.Extract<TcpPacket>();

    if (tcpPacket != null)
    {
        // Обработка TCP-пакета
        Console.WriteLine($"TCP Packet: {tcpPacket.ToString()}");
    }
};

// Начинаем захват пакетов
device.StartCapture();

Console.WriteLine("Press Enter to stop capturing...");
Console.ReadLine();

// Останавливаем захват при нажатии Enter
device.StopCapture();
device.Close();


//CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
//CancellationToken token = cancelTokenSource.Token;

//Client client = new Client();
//Server server = new Server();
//Hacker hacker = new Hacker();

//try
//{
//    Task.WaitAll([server.Listen(token),
//        hacker.ConnectToServer(Server.ServerIP, token),
//        client.ConnectToServer(Server.ServerIP, cancelTokenSource)]);

//    Thread.Sleep(1000);
//}
//catch (AggregateException ex) when (ex.InnerException is TaskCanceledException)
//{ }