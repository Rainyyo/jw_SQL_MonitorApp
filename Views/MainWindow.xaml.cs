using System.ComponentModel;
using System.Windows;
using NLog;
namespace MonitorApp.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent(); 
            Logger logger = LogManager.GetCurrentClassLogger();

    }
        protected override void OnClosing(CancelEventArgs e)
        {

            if (MessageBox.Show("你确定关闭软件吗？", "确认", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
            {
            }   
            else
                e.Cancel = true;
            base.OnClosing(e);
        }

        private void MsgTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            MsgTextBox.ScrollToEnd();
        }
    }
}
