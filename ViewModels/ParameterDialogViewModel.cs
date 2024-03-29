using Prism.Commands;
using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonitorApp.Properties;
using Microsoft.Win32;
using System.Windows;

namespace MonitorApp.ViewModels
{
    public class ParameterDialogViewModel : DialogViewModel
    {
        #region 属性
        private string ip;
        public string IP
        {
            get { return ip; }
            set { SetProperty(ref ip, value); }
        }
        private int startAddr;
        public int StartAddr
        {
            get { return startAddr; }
            set { SetProperty(ref startAddr, value); }
        }
        private int countAddr;
        public int CountAddr
        {
            get { return countAddr; }
            set { SetProperty(ref countAddr, value); }
        }
        private string filePath;
        public string FilePath
        {
            get { return filePath; }
            set { SetProperty(ref filePath, value); }
        }


        private int loadState;
        public int LoadState
        {
            get { return loadState; }
            set { SetProperty(ref loadState, value); }
        }
        private int producting;
        public int Producting
        {
            get { return producting; }
            set { SetProperty(ref producting, value); }
        }
        private int pause;
        public int Pause
        {
            get { return pause; }
            set { SetProperty(ref pause, value); }
        }
        private int stop;
        public int Stop
        {
            get { return stop; }
            set { SetProperty(ref stop, value); }
        }
        
        #endregion
        #region 方法
        private DelegateCommand loadFile;
        public DelegateCommand LoadFile =>
            loadFile ?? (loadFile = new DelegateCommand(ExecuteLoadFile));

        void ExecuteLoadFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "CSV文件 (*.csv)|*.csv";
            //openFileDialog.Filter = "CSV 文件 (*.csv)|*.csv|所有文件 (*.*)|*.*";

            openFileDialog.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory + "报警文件"; // 可选：设置初始目录
            bool? result = openFileDialog.ShowDialog();

            if (result == true)
            {
                FilePath = openFileDialog.FileName;
                // 这里可以继续处理选择的文件，比如读取文件内容等操作
                Properties.Settings.Default.Filer = FilePath;
                Properties.Settings.Default.Save();
            }
            MessageBox.Show("请重启软件加载配置！！！","消息");
        }
        private DelegateCommand<object> textBoxLostFocusCommand;
        public DelegateCommand<object> TextBoxLostFocusCommand =>
            textBoxLostFocusCommand ?? (textBoxLostFocusCommand = new DelegateCommand<object>(ExecuteTextBoxLostFocusCommand));
        void ExecuteTextBoxLostFocusCommand(object obj)
        {
            if (obj is string str)
            {
                switch (str)
                {
                    case "IP":
                        Settings.Default.PLC= IP;
                        break;

                    case "起始地址":
                        Settings.Default.报警起始地址 = StartAddr;
                        break;
                    case "点位个数":
                        Settings.Default.报警点位个数 = CountAddr;
                        break;


                    case "待料":
                        Settings.Default.待料 = LoadState;
                        break;
                    case "生产":
                        Settings.Default.生产 = Producting;
                        break;
                    case "暂停":
                        Settings.Default.暂停 = Pause;
                        break;
                    case "急停":
                        Settings.Default.急停 = Stop;
                        break;
                }
                
            }
        }
        #endregion
        #region 构造函数
        public ParameterDialogViewModel(IContainerProvider containerProvider) : base(containerProvider)
        {
            Title = "参数设置";
            IP = Settings.Default.PLC;

            StartAddr = Settings.Default.报警起始地址;
            CountAddr = Settings.Default.报警点位个数;
            FilePath = Settings.Default.Filer;

            LoadState = Settings.Default.待料;
            Producting = Settings.Default.生产;
            Pause = Settings.Default.暂停;
            Stop = Settings.Default.急停;
        }
        #endregion

    }
}
