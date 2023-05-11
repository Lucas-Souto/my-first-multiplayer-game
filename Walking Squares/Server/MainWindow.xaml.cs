using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Shared;

namespace Server
{
    /// <summary>
    /// Interação lógica para MainWindow.xam
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow Main;
        private ServerController server;

        public MainWindow()
        {
            InitializeComponent();

            Main = this;
            PlayerLimit.Text = ServerController.Backlog.ToString();
            Closing += (s, e) =>
            {
                if (server != null)
                {
                    server.Close();

                    server = null;
                }
            };
        }

        private void FruitTime_TextChanged(object sender, TextChangedEventArgs e)
        {
            double time = Convert.ToDouble(FruitTime.Text);

            if (server != null && time > 0) server.SpawnTimer.Interval = time * 1000;
        }

        private void PlayerLimit_TextChanged(object sender, TextChangedEventArgs e)
        {
            ServerController.Backlog = Convert.ToInt32(PlayerLimit.Text);
        }

        private void ToggleServer_Click(object sender, RoutedEventArgs e)
        {
            if (server == null)
            {
                server = new ServerController();
                server.SpawnTimer.Interval = Convert.ToDouble(FruitTime.Text) * 1000;
                server.SpawnTimer.Enabled = SpawnFruits.IsChecked.Value;
                ToggleServer.Content = "Stop";
            }
            else
            {
                server.Close();

                server = null;
                ToggleServer.Content = "Play";
            }
        }

        private void ClearScore_Click(object sender, RoutedEventArgs e)
        {
            if (server != null) server.ClearScore();
        }

        private void ClearConsole_Click(object sender, RoutedEventArgs e)
        {
            Console.Text = string.Empty;
        }

        private void SpawnFruits_Toggle(object sender, RoutedEventArgs e)
        {
            if (server != null) server.SpawnTimer.Enabled = SpawnFruits.IsChecked.Value;
        }
    }
}
