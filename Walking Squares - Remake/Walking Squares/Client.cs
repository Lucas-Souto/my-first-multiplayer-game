using System;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Shared;
using WS.Objects;
using Microsoft.Xna.Framework;

namespace WS
{
    public class Client
    {
        public bool Connected = false;
        public GameState Game = null;

        private Thread _listenThread;
        private IPEndPoint _endPoint;
        private Socket _socket;

        private EndPoint server = new IPEndPoint(Dns.GetHostEntry(Constants.Net.Host).AddressList[0], Constants.Net.Port);

        public Client()
        {
            try
            {
                Constants.Net.CreateSocket(out _socket, out _endPoint, true);
                _socket.Bind(_endPoint);

                _listenThread = new Thread(Listen);

                _listenThread.Start();

                Console.WriteLine("Cliente iniciado!");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void Close()
        {
            _listenThread.Abort();
            _socket.Dispose();

            _socket = null;
        }

        private void Listen()
        {
            try
            {
                byte[] buffer = new byte[Constants.Net.BufferSize];

                while (true)
                {
                    if (_socket == null) return;

                    Message message = _socket.ReceiveMessage(ref server);
                    string ePString = (_socket.LocalEndPoint as IPEndPoint).Stringfy();

                    switch (message.Type)
                    {
                        case MessageType.GameState:
                            if (message.Properties.ContainsKey("state"))
                            {
                                Game = message.Get("state", Game);

                                if (Game != null && Game.Players.ContainsKey(ePString)) Connected = true;
                                
                                foreach (KeyValuePair<string, Player> player in Game.Players)
                                {
                                    if (!PlayerPuppet.Players.ContainsKey(player.Key)) new PlayerPuppet(player.Key, ePString) { Position = player.Value.Position };
                                    else
                                    {
                                        PlayerPuppet.Players[player.Key].Position = player.Value.Position;
                                        PlayerPuppet.Players[player.Key].Score = player.Value.Score;
                                    }
                                }

                                foreach (KeyValuePair<string, Fruit> fruit in Game.Fruits)
                                {
                                    if (!FruitPuppet.Fruits.ContainsKey(fruit.Key)) new FruitPuppet(fruit.Value.IsSuper, fruit.Key) { Position = fruit.Value.Position };
                                }
                            }
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);

                if (_socket != null) Listen();
            }
        }

        public void Send(Message message) => _socket?.SendMessage(server, message);
    }
}