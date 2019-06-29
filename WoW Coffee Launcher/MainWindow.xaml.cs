using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
            await Task.Run(async() =>
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
            Test();
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
