using System;
using System.Timers;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using Shared;
using Shared.Game;
using System.Windows;
using System.Windows.Threading;

namespace Server
{
    public class ServerController
    {
        public static int Backlog = 15;
        public System.Timers.Timer SpawnTimer = new System.Timers.Timer(200 * 1000);
        private DispatcherTimer UpdateTimer = new DispatcherTimer();
        private Socket server;
        private Thread listenThread;
        private Hashtable players;
        
        private GameState state;

        public ServerController()
        {
            try
            {
                IPHostEntry host = Dns.GetHostEntry(ServerData.IP);
                IPAddress addres = host.AddressList[0];
                IPEndPoint endPoint = new IPEndPoint(addres, ServerData.Port);
                server = new Socket(addres.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                players = new Hashtable();
                state = new GameState();

                server.Bind(endPoint);
                server.Listen(Backlog);

                listenThread = new Thread(Listen);

                listenThread.Start();

                SpawnTimer.Enabled = true;
                SpawnTimer.Elapsed += new ElapsedEventHandler((object send, ElapsedEventArgs e) => state.AddFruit());
                SpawnTimer.AutoReset = true;

                UpdateTimer.IsEnabled = true;
                UpdateTimer.Interval = TimeSpan.FromMilliseconds(1000 / 60);
                UpdateTimer.Tick += (s, e) => UpdateGame();

                Log("Servidor iniciado!");
            }
            catch (Exception e)
            {
                Log("Construtor: " + e.Message);
            }
        }

        private void Log(object content)
        {
            MainWindow.Main.Console.Text += string.Format("{0}{1}", MainWindow.Main.Console.Text.Length > 0 ? "\n" : "", content);

            MainWindow.Main.ConsoleScroll.ScrollToBottom();
        }
        private void OutThreadLog(object content) => Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => Log(content)));

        public void Close()
        {
            SpawnTimer.Stop();
            UpdateTimer.Stop();
            server.Close();
            listenThread.Abort();
            Log("Servidor fechado!");
        }

        private void Listen()
        {
            Socket client;
            Thread clientThreads;

            while (true)
            {
                client = server.Accept();
                clientThreads = new Thread(ListenClient);

                clientThreads.Start(client);
            }
        }
        private void ListenClient(object socket)
        {
            Socket client = (Socket)socket;

            try
            {
                while (true)
                {
                    Message message = null;

                    do
                    {
                        message = client.ReceiveMessage();
                    } while (message.Type == MessageType.None || message.Properties.Count < 1);

                    string user = message.Get("user_id", string.Empty);
                    
                    if (user.Length > 0)
                    {
                        switch (message.Type)
                        {
                            case MessageType.Login:
                                if (!players.ContainsKey(user)) AddPlayer(user, client);
                                break;
                            case MessageType.Disconect:
                                if (players.ContainsKey(user)) RemovePlayer(user);
                                break;
                            case MessageType.PlayerInput:
                                if (players.ContainsKey(user)) MovePlayer(user, message.Get("dir_x", 0), message.Get("dir_y", 0));
                                break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                OutThreadLog("ListenClient: " + e.Message);
                client.Close();

                foreach (DictionaryEntry player in players)
                {
                    RemovePlayer(player.Key.ToString());

                    return;
                }
            }
        }

        public void ClearScore()
        {
            if (state.Players.Count > 0)
            {
                foreach (KeyValuePair<string, Player> player in state.Players) player.Value.Score = 0;

                Log("Score resetado!");
            }
        }

        private void UpdateGame()
        {
            try
            {
                Dictionary<string, object> properties = new Dictionary<string, object>();

                properties.Add("players", state.Players);
                properties.Add("fruits", state.Fruits);
                Broadcast(new Message(MessageType.GameData, properties));
                RenderPlayerList();
            }
            catch (Exception e)
            {
                OutThreadLog("UpdateGame: " + e.Message);
            }
        }

        private void Broadcast(Message message)
        {
            foreach (DictionaryEntry player in players) ((Socket)player.Value).SendMessage(message);
        }

        private void RenderPlayerList()
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                MainWindow.Main.PlayerList.Items.Clear();

                foreach (DictionaryEntry player in players)
                {
                    MainWindow.Main.PlayerList.Items.Add(new System.Windows.Controls.ListBoxItem() { Content = ResumeString((string)player.Key, 12) + " | " + state.Players[(string)player.Key].Score });
                }
            }));
        }

        private string ResumeString(string text, int maxLength)
        {
            if (text.Length > maxLength) text = text.Substring(0, maxLength) + "...";

            return text;
        }

        private void AddPlayer(string user, Socket client)
        {
            players.Add(user, client);
            state.AddPlayer(user);
            OutThreadLog(string.Format("O usurário {0} entrou!", user));
        }
        private void RemovePlayer(string user)
        {
            players.Remove(user);
            state.RemovePlayer(user);
            OutThreadLog(string.Format("O usurário {0} saiu!", user));
        }
        private void MovePlayer(string id, int dirX, int dirY)
        {
            Position pos = state.Players[id].Position;

            if (dirX != 0) pos.X += dirX;
            else if (dirY != 0) pos.Y += dirY;

            if (pos.X >= GameState.MapWidth) pos.X = 0;
            else if (pos.X < 0) pos.X = GameState.MapWidth - 1;

            if (pos.Y >= GameState.MapHeight) pos.Y = 0;
            else if (pos.Y < 0) pos.Y = GameState.MapHeight - 1;

            for (int i = state.Fruits.Count - 1; i >= 0; i--)
            {
                if (state.Fruits[i].Position == pos)
                {
                    state.Players[id].Score += state.Fruits[i].IsSuper ? GameState.SuperFruitValue : GameState.FruitValue;

                    state.Fruits.RemoveAt(i);
                }
            }

            state.Players[id].Position = pos;
        }
    }
}