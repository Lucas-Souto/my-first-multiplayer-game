using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;

namespace Shared
{
    public static class SocketExtensions
    {
        public static void SendMessage(this Socket socket, Message message)
        {
            if (!socket.Connected) return;

            byte[] buffer = new byte[ServerData.BufferSize];
            byte[] serialized = message.Serializate();

            Array.Copy(serialized, buffer, serialized.Length);
            socket.Send(buffer);
        }
        public static Message ReceiveMessage(this Socket socket)
        {
            if (socket.Connected)
            {
                byte[] buffer = new byte[ServerData.BufferSize];

                socket.Receive(buffer);
                buffer.Deserializate(out object message);

                if (message != null && message is Message) return (Message)message;
            }

            return new Message();
        }

        private static byte[] Serializate<T>(this T serialize)
        {
            MemoryStream memory = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();

            formatter.Serialize(memory, serialize);

            return memory.ToArray();
        }
        private static void Deserializate(this byte[] deserialize, out object obj)
        {
            MemoryStream memory = new MemoryStream(deserialize);
            BinaryFormatter formatter = new BinaryFormatter();

            obj = formatter.Deserialize(memory);
        }
    }
}