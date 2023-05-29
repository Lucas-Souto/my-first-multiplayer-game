using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Shared;
using System.Text;
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace Server
{
    public class Server
    {
        public static Server Instance;

        private Thread _listenThread, _updateThread;
        private IPEndPoint _endPoint;
        private Socket _socket;

        private List<EndPoint> clients = new List<EndPoint>();
        private GameState game = new GameState();
        private Stopwatch stopwatch = new Stopwatch();

        private const float UpdateTime = 1 / 60;

        public Server()
        {
            try
            {
                Instance = this;
                GameState.AFKAction += RemovePlayer;

                Constants.Net.CreateSocket(out _socket, out _endPoint);
                _socket.Bind(_endPoint);

                _socket.EnableBroadcast = true;
                _listenThread = new Thread(Listen);
                _updateThread = new Thread(Update);

                _listenThread.Start();
                _updateThread.Start();

                uint IOC_IN = 0x80000000;
                uint IOC_VENDOR = 0x18000000;
                uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;
                _socket.IOControl((int)SIO_UDP_CONNRESET, new byte[] { Convert.ToByte(false) }, null);

                Console.WriteLine("Servidor iniciado!");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void RemovePlayer(string id)
        {
            EndPoint eP = clients.FirstOrDefault(e => (e is IPEndPoint endPoint) && endPoint.Stringfy() == id);

            game.RemovePlayer(id);
            clients.Remove(eP);
        }

        private void Listen()
        {
            try
            {
                byte[] buffer = new byte[Constants.Net.BufferSize];

                while (true)
                {
                    EndPoint client = new IPEndPoint(IPAddress.Any, 0);
                    Message message = _socket.ReceiveMessage(ref client);
                    
                    if (message.Type != MessageType.None)
                    {
                        IPEndPoint clientEP = client as IPEndPoint;
                        string clientEPString = clientEP.Stringfy();
                        bool playerExists = game.Players.ContainsKey(clientEPString);

                        switch (message.Type)
                        {
                            case MessageType.Login:
                                if (!playerExists)
                                {
                                    clients.Add(client);
                                    game.AddPlayer(clientEP);
                                    Console.WriteLine("Conexão vinda do {0}", clientEPString);
                                }
                                break;
                            case MessageType.PlayerInput:
                                if (playerExists) game.Players[clientEPString].Direction = message.GetVector("direction");
                                break;
                        }
                    }
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.Message);
                Listen();
            }
        }

        private void Update()
        {
            float deltaTime = 0, updateTimer = 0;

            while (true)
            {
                stopwatch.Restart();

                if (game.Players.Count > 0 && updateTimer >= UpdateTime)
                {
                    updateTimer = 0;
                    
                    game.Update(deltaTime);
                    BroadCast(new Message(MessageType.GameState, new Dictionary<string, object>() { { "state", game } }));
                }

                stopwatch.Stop();

                deltaTime = (float)stopwatch.Elapsed.TotalSeconds;
            }
        }

        public void Send(Message message, EndPoint endPoint) => _socket.SendMessage(endPoint, message);
        public void BroadCast(Message message)
        {
            for (int i = clients.Count - 1; i >= 0; i--)
            {
                try
                {
                    Send(message, clients[i]);
                }
                catch
                {
                    string id = ((IPEndPoint)clients[i]).Stringfy();

                    Console.WriteLine("O {0} se desconectou!", id);
                    RemovePlayer(id);
                }
            }
        }
    }
}