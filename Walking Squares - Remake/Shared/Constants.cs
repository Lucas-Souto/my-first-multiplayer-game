using System.Net;
using System.Net.Sockets;

namespace Shared
{
    public static class Constants
    {
        public static class Game
        {
            public const int EntitySize = 32, MapSize = EntitySize * 12;
        }

        public static class Net
        {
            public const int Port = 4000, Backlog = 15, BufferSize = 4096;
            public const string Host = "localhost";

            public static void CreateSocket(out Socket socket, out IPEndPoint endPoint, bool isClient = false)
            {
                IPHostEntry host = Dns.GetHostEntry(Host);
                IPAddress address = host.AddressList[0];
                endPoint = new IPEndPoint(address, isClient ? 0 : Port);
                socket = new Socket(address.AddressFamily, SocketType.Dgram, ProtocolType.Udp);

                socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.PacketInformation, true);
            }
        }
    }
}