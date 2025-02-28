using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DocStack
{
    /// <summary>
    /// Interaction logic for SplashScreen.xaml
    /// </summary>
    public partial class SplashScreen : Window , INotifyPropertyChanged
    {

        private double _percentage;

        public double Percentage
        {
            get => _percentage;
            set
            {
                if(_percentage != value)
                {
                    _percentage = value;
                    OnPropertyChanged(nameof(Percentage));
                }
            }
        }

        private BackgroundWorker _worker;

        private Random _random;
        public SplashScreen()
        {
            InitializeComponent();
            _worker = new();
            _worker.WorkerReportsProgress = true;
            _worker.WorkerSupportsCancellation = true;
            _worker.DoWork += Worker_DoWork!;
            _worker.ProgressChanged += Worker_ProgressChanged!;
            _worker.RunWorkerCompleted += NavigateMainScreen!;
            _worker.RunWorkerAsync();
            _random = new();
            DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = (sender as BackgroundWorker)!;
            for (int i = 0; i <= 100; i++)
            {
                int sleep = _random.Next(20, 50);
                System.Threading.Thread.Sleep(sleep);
                worker.ReportProgress(i);

                if (i == 100)
                    worker.CancelAsync();
                
            }
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e) => Percentage = e.ProgressPercentage;

        private void NavigateMainScreen(object sender, EventArgs e)
        {
            _worker.Dispose();
            MainWindow mainWindow = new();
            mainWindow.Show();
            this.Close();
        }
    }
}
