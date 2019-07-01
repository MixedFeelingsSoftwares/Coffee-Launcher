using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WoW_Coffee_Launcher.Core.Downloader;

namespace WoW_Coffee_Launcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }


        public async void Test()
        {
            await Task.Run(async () =>
            {
                for (int i = 0; i <= 100; i++)
                {
                    await PB_DownloadProgress.Dispatcher.BeginInvoke(new Action(() => PB_DownloadProgress.SetPercent(i, 0.5f)));
                    await Task.Delay(100);
                }
            });
        }

        private void Btn_Patcher_Click(object sender, RoutedEventArgs e)
        {
            switch (State.CurrentState)
            {
                case State.state.Deleting:
                    break;
                case State.state.Installing:
                    break;
                case State.state.Patching:
                    break;
                case State.state.Default:
                    Patcher.DownloadPatches();
                    break;
                case State.state.Ready:
                    Process.Start($"{Environment.CurrentDirectory}/Wow.exe");
                    break;
                default:
                    break;
            }
        }

        private void FrmMain_Loaded(object sender, RoutedEventArgs e)
        {
            State.OnStateChanged += State_OnStateChanged;
            Installer.downloadBar = PB_DownloadProgress;
        }

        private void State_OnStateChanged(object sender, State.state e)
        {
            int num = (int)e;
            double percent = (double)num / (typeof(State.state).GetEnumValues().Length - 1) * 100;

            PB_DownloadProgress.Dispatcher.BeginInvoke(new Action(() => PB_DownloadProgress.SetPercent(percent, 0.5f)));
            if (e == State.state.Ready || e == State.state.Default)
            {
                switch (e)
                {
                    case State.state.Default:
                        btn_Patcher.Dispatcher.BeginInvoke(new Action(() => btn_Patcher.Content = "Download Patches"));
                        break;
                    case State.state.Ready:
                        btn_Patcher.Dispatcher.BeginInvoke(new Action(() => btn_Patcher.Content = "Launch WoW"));
                        break;
                }

                btn_Patcher.Dispatcher.BeginInvoke(new Action(() => btn_Patcher.IsEnabled = true));
            }
            else
            {
                btn_Patcher.Dispatcher.BeginInvoke(new Action(() => btn_Patcher.Content = e));
                btn_Patcher.Dispatcher.BeginInvoke(new Action(() => btn_Patcher.IsEnabled = false));
            }
        }

        private void Txt_path_Loaded(object sender, RoutedEventArgs e)
        {
            if (Environment.CurrentDirectory.isWoWDiretory())
            {
                txt_path.Text = Environment.CurrentDirectory;
            }
        }

        private void LB_ServerStatus_Loaded(object sender, RoutedEventArgs e)
        {
            PingInterval();
        }

        public void PingInterval()
        {
            SendPing();
            {
                System.Timers.Timer tmr = new System.Timers.Timer();
                tmr.Interval = 10000;
                tmr.Elapsed += (s, g) =>
                {
                    SendPing();
                };
                tmr.AutoReset = true;
                tmr.Start();
            }
        }

        public async void SendPing()
        {
            try
            {
                string IP = await Patcher.GetRealmlistIP();

                bool serverStatus = await PingHost(IP, 8085, 5000);
                string status = serverStatus ? "Online" : "Offline";

                await LB_ServerStatus.Dispatcher.BeginInvoke(new Action(() => LB_ServerStatus.Content = status));


                await Dispatcher.BeginInvoke(new Action(() =>
                {
                    Console.WriteLine(status);
                     LB_ServerStatus.Foreground.BeginAnimation(SolidColorBrush.ColorProperty, LB_ServerStatus.AnimationByStatus(serverStatus));


                     Icon_StatusGlobe.Foreground.BeginAnimation(SolidColorBrush.ColorProperty, LB_ServerStatus.AnimationByStatus(serverStatus));
                }));
            }
            catch
            {

            }
        }


        /// <summary>
        /// Ping Host IP address
        /// </summary>
        /// <param name="hostUri">Address</param>
        /// <param name="portNumber">Port Number</param>
        /// <param name="timeout">Timeout</param>
        /// <returns>Ping Success</returns>
        public static async Task<bool> PingHost(string hostUri, int portNumber, int timeout)
        {
            return await Task.Run(() =>
            {
                var client = new TcpClient();
                var result = client.BeginConnect(hostUri, portNumber, null, null);

                var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(timeout));

                if (success)
                {
                    // we have connected
                    client.EndConnect(result);
                }

                return success;
            });
        }
    }

    public static class ProgressBarExtensions
    {

        public static void SetPercent(this System.Windows.Controls.ProgressBar progressBar, double percentage, double _duration)
        {
            var duration = TimeSpan.FromSeconds(_duration);
            DoubleAnimation animation = new DoubleAnimation(percentage, duration);

            progressBar.BeginAnimation(System.Windows.Controls.ProgressBar.ValueProperty, animation);
        }
    }
}
