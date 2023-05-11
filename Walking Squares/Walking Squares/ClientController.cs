using System;
using System.Timers;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using Shared;
using Shared.Game;
using Microsoft.Xna.Framework;

namespace Walking_Squares
{
    public class ClientController
    {
        public bool IsConnected => client != null && client.Connected;
        public string PlayerId { get; private set; }
        public GameState CurrentState { get; private set; } = new GameState();

        private IPHostEntry host;
        private IPAddress address;
        private IPEndPoint endPoint;
        private Socket client;
        private Thread listenThread;

        public ClientController()
        {
            try
            {
                host = Dns.GetHostEntry(ServerData.IP);
                address = host.AddressList[0];
                endPoint = new IPEndPoint(address, ServerData.Port);
                client = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void Start()
        {
            try
            {
                PlayerId = Environment.MachineName + Guid.NewGuid().ToString("N");

                client.Connect(endPoint);
                client.SendMessage(new Message(MessageType.Login, new Dictionary<string, object>() { { "user_id", PlayerId } }));

                listenThread = new Thread(Listen);

                listenThread.SetApartmentState(ApartmentState.STA);
                listenThread.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Main.Game.Exit();
            }
        }

        public void Close()
        {
            client.SendMessage(new Message(MessageType.Disconect, new Dictionary<string, object>() { { "user_id", PlayerId } }));
            client.Shutdown(SocketShutdown.Both);
            client.Close();
            listenThread.Abort();
            Main.Game.Exit();
        }

        public void Move(Point direction)
        {
            if (direction != Point.Zero)
            {
                client.SendMessage(new Message(MessageType.PlayerInput, new Dictionary<string, object>() { { "user_id", PlayerId }, { "dir_x", direction.X }, { "dir_y", direction.Y } }));

                CurrentState.Players[PlayerId].Position = new Position(CurrentState.Players[PlayerId].Position.X + direction.X, CurrentState.Players[PlayerId].Position.Y + direction.Y);
            }
        }

        private void Listen()
        {
            try
            {
                Message message;

                while (true)
                {
                    message = client.ReceiveMessage();

                    switch (message.Type)
                    {
                        case MessageType.GameData:
                            CurrentState = new GameState
                            {
                                Players = message.Get("players", new Dictionary<string, Player>()),
                                Fruits = message.Get("fruits", new List<Fruit>())
                            };
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}