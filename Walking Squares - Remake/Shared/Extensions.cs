using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;

namespace Shared
{
    public static class Extensions
    {
        private static JsonSerializerSettings Settings = new JsonSerializerSettings()
        {
            NullValueHandling = NullValueHandling.Ignore,
            TypeNameHandling = TypeNameHandling.None,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            Formatting = Formatting.None
        };

        public static void GetWarp(this List<Rectangle> warpRects, Rectangle slice, Rectangle area)
        {
            if (slice.Size == Point.Zero || area.Size == Point.Zero) return;

            if (slice.Right > area.Right || slice.Left < area.Left)
            {
                if (slice.Right > area.Right) warpRects.Add(new Rectangle(new Point(0, slice.Top), new Point(MathHelper.Clamp(slice.Right - area.Right, 0, slice.Size.X), slice.Size.Y)));
                else if (slice.Left < area.Left) warpRects.Add(new Rectangle(new Point(area.Right - Math.Abs(area.Left - slice.Left), slice.Top), new Point(MathHelper.Clamp(Math.Abs(area.Left - slice.Left), 0, slice.Size.X), slice.Size.Y)));

                GetWarp(warpRects, warpRects[warpRects.Count - 1], area);
            }

            if (slice.Bottom > area.Bottom || slice.Top < area.Top)
            {
                if (slice.Bottom > area.Bottom) warpRects.Add(new Rectangle(new Point(slice.Left, 0), new Point(slice.Size.X, MathHelper.Clamp(slice.Bottom - area.Bottom, 0, slice.Size.Y))));
                else if (slice.Top < area.Top) warpRects.Add(new Rectangle(new Point(slice.Left, area.Bottom - Math.Abs(area.Top - slice.Top)), new Point(slice.Size.X, MathHelper.Clamp(Math.Abs(area.Top - slice.Top), 0, slice.Size.Y))));

                GetWarp(warpRects, warpRects[warpRects.Count - 1], area);
            }
        }

        public static void SendMessage(this Socket socket, EndPoint endPoint, Message message)
        {
            string json = JsonConvert.SerializeObject(message, Settings);
            byte[] buffer = json.Serializate();

            socket.SendTo(buffer, SocketFlags.None, endPoint);
        }
        public static Message ReceiveMessage(this Socket socket, ref EndPoint endPoint)
        {
            byte[] buffer = new byte[Constants.Net.BufferSize];

            socket.ReceiveFrom(buffer, SocketFlags.None, ref endPoint);
            buffer.Deserializate(out object obj);

            if (obj is string json) return JsonConvert.DeserializeObject<Message>(json, Settings);

            return new Message();
        }

        public static string Stringfy(this IPEndPoint endPoint) => string.Format("{0}:{1}", endPoint.Address, endPoint.Port);

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