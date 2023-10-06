using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;


namespace SysLab2WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ProjectParameters ProjectParameters;

        private Dictionary<double, double> plotData = new Dictionary<double, double>();

        public MainWindow()
        {
            InitializeComponent();
            programStatusBar.Visibility = Visibility.Collapsed;
            var s = new ManagementObjectSearcher(@"SELECT * FROM Win32_processor");
            var o = s.Get().OfType<ManagementObject>().First();
			ClockSpeedBlock.Text = "Тактовая частота: " + o["CurrentClockSpeed"];
			//RefreshClockSpeed();
            CoreCountBlock.Text += 6;
            WpfPlot1.Plot.YLabel("Время выполнения, ms");
        }

        private async void RefreshClockSpeed()
        {
            await Task.Run(() =>
            {
                while (true)
                {
                    var s = new ManagementObjectSearcher(@"SELECT * FROM Win32_processor");
                    var o = s.Get().OfType<ManagementObject>().First();
                    this.Dispatcher.Invoke(new Action(() => {
                        
                    }));
                    Thread.Sleep(2000);
                }
            });
        }

        private async void ButtonSubmit_Click(object sender, RoutedEventArgs e)
        {
			plotData.Clear();
			WpfPlot1.Plot.Clear();
			WpfPlot1.Refresh();
			programStatusBar.Visibility = Visibility.Visible;

			if (!string.IsNullOrEmpty(BoxArrayN.Text)
                && !string.IsNullOrEmpty(BoxArrayK.Text)
                && !string.IsNullOrEmpty(BoxThreadsN.Text))
            {
                ProjectParameters = 
                    new ProjectParameters
                    (BoxArrayN.Text, BoxArrayK.Text, BoxThreadsN.Text, BoxDeltaK.Text, BoxDeltaThreads.Text);

                ButtonSubmit.IsEnabled = false;

                double oneThread = ProjectParameters.TestAlgo();

                plotData = await ProjectParameters.ProgramStart(ProjectParameters.ProjectMode);

                programStatusBar.Visibility = Visibility.Collapsed;
                
                WpfPlot1.Plot.AddHorizontalLine(oneThread, label: "One thread");
                
                WpfPlot1.Plot.AddScatter(plotData.Keys.ToArray(), plotData.Values.ToArray(), label: "Multi-thread");
                WpfPlot1.Plot.Legend();
                WpfPlot1.Refresh();
				ButtonSubmit.IsEnabled = true;
			}
        }

        private void ButtonReset_Click(object sender, RoutedEventArgs e)
        {
            BoxArrayN.Text = "";
            BoxArrayK.Text = "";
            BoxDeltaK.Text = "";
            BoxDeltaThreads.Text = "";
            BoxThreadsN.Text = "";
            plotData.Clear();
            WpfPlot1.Plot.Clear();
            WpfPlot1.Refresh();
            ButtonSubmit.IsEnabled = true;
        }

        private void ModeDeltaThreads_Checked(object sender, RoutedEventArgs e)
        {
            BoxDeltaThreads.IsEnabled = true;
            BoxDeltaK.IsEnabled = false;
            WpfPlot1.Plot.XLabel("Кол-во потоков");
        }

        private void ModeDeltaK_Checked(object sender, RoutedEventArgs e)
        {
            BoxDeltaK.IsEnabled = true;
            BoxDeltaThreads.IsEnabled = false;
            WpfPlot1.Plot.XLabel("Сложность операций");
        }
    }
}
