using MonitorApp.Services;
using NLog;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using MonitorApp.Properties;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Runtime.CompilerServices;
using System.IO;
using Prism.Regions;
using System.Text;
using MySql.Data.MySqlClient;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using MonitorApp.Model;
using System.Windows;
using OfficeOpenXml;
using System.Collections.Concurrent;

namespace MonitorApp.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        #region 变量
        private readonly IPLCService pLC;
        private readonly IDialogService _dialogService;
        private static Logger logger = LogManager.GetCurrentClassLogger();
        MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder();
        private bool isParameterDialogShow = false;
        int 起始地址;
        int 地址个数;
        private ConcurrentQueue<SignalMsg> WarningQueue = new();
        bool[] mPreWarning1 = null;

        #endregion
        #region 属性
        private string _title = "MonitorApp";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }
        public string Version { get; set; } = "1.0.0";

        //日志打印
        private string messageStr = string.Empty;
        public string MessageStr
        {
            get { return messageStr; }
            set { SetProperty(ref messageStr, value); }
        }
        private string ip;
        public string IP
        {
            get { return ip; }
            set { SetProperty(ref ip, value); }
        }
        private bool pLCState;
        public bool PLCState
        {
            get { return pLCState; }
            set { SetProperty(ref pLCState, value); }
        }
        private bool mysqlState;
        public bool MysqlState
        {
            get { return mysqlState; }
            set { SetProperty(ref mysqlState, value); }
        }
        private bool isWindowFocused;
        public bool IsWindowFocused
        {
            get { return isWindowFocused; }
            set { SetProperty(ref isWindowFocused, value); }
        }
        private string fieldName;
        public string FileName
        {
            get { return fieldName; }
            set { SetProperty(ref fieldName, value); }
        }

        private bool buttonState = false;
        public bool ButtonState
        {
            get { return buttonState; }
            set { SetProperty(ref buttonState, value); }
        }
        private string inOutTime = "下机";
        public string InOutTime
        {
            get { return inOutTime; }
            set { SetProperty(ref inOutTime, value); }
        }
        private string id;
        public string ID
        {
            get { return id; }
            set { SetProperty(ref id, value); }
        }
        private string name = "ZhangSan";
        public string Name
        {
            get { return name; }
            set { SetProperty(ref name, value); }
        }
        #endregion
        #region 命令绑定
        private DelegateCommand appLoadedEventCommand;
        public DelegateCommand AppLoadedEventCommand =>
            appLoadedEventCommand ?? (appLoadedEventCommand = new DelegateCommand(ExecuteAppLoadedEventCommand));
        private DelegateCommand appClosedEventCommand;
        public DelegateCommand AppClosedEventCommand =>
            appClosedEventCommand ?? (appClosedEventCommand = new DelegateCommand(ExecuteAppClosedEventCommand));
        private DelegateCommand<object> _menuCommand;
        public DelegateCommand<object> MenuCommand =>
            _menuCommand ?? (_menuCommand = new DelegateCommand<object>(ExecuteMenuCommand));
        private DelegateCommand openFileCommand;
        public DelegateCommand OpenFileCommand =>
            openFileCommand ?? (openFileCommand = new DelegateCommand(ExecuteOpenFileCommand));
        private DelegateCommand<string> testCommand;
        public DelegateCommand<string> TestCommand =>
            testCommand ?? (testCommand = new DelegateCommand<string>(ExecuteTestCommand));
        private DelegateCommand clickState;
        public DelegateCommand ClickState =>
           clickState ?? (clickState = new DelegateCommand(ExecuteClickState));
        void ExecuteMenuCommand(object obj)
        {
            switch (obj.ToString())
            {
                case "参数":
                    if (!isParameterDialogShow)
                    {
                        isParameterDialogShow = true;
                        DialogParameters param = new DialogParameters();
                        _dialogService.Show("ParameterDialog", param, arg =>
                        {
                            isParameterDialogShow = false;
                            IsWindowFocused = !IsWindowFocused;
                        });
                    }
                    break;
            }
        }
        void ExecuteClickState()
        {
            if (ID==null)
            {
                MessageBox.Show("请输入工号再进行上机/下机操作！！！");
                return;
            }
            ButtonState = !ButtonState;
            if (ButtonState == true)
            {
                Task.Run(() =>
                {
                    if (ID != null)
                    {
                        InOutTime = "上机";
                        var res = Global.Insert_pc_data_emp(ID, Name, "INTIME", "上机");
                        if (res == "ok")
                        {
                            addMessage($"上机操作：\t{ID}\t{Name}");
                        }
                        else
                        {
                            addMessage("数据库错误:" + res);
                        }

                    }
                });

            }
            if (ButtonState == false)
            {
                Task.Run(() =>
                {
                    if (ID != null)
                    {
                        InOutTime = "下机";
                        var res = Global.Insert_pc_data_emp(ID, Name, "OUTTIME", "下机");
                        if (res == "ok")
                        {
                            addMessage($"下机操作：\t{ID}\t{Name}");
                            ID = null;
                        }
                        else
                        {
                            addMessage("数据库错误:" + res);
                        }
                    }
                });
            }
        }
        async void ExecuteTestCommand(string obj)
        {
            //初始化连接实例
            var connection = new MySqlConnection(builder.ConnectionString);
            switch (obj)
            {
                case "新建数据库":

                    connection.Open();

                    string sql = "SELECT * FROM pc_data_alarm";

                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int col1 = reader.GetInt32(0); // 第一列，假设是整数类型
                                string col2 = reader.GetString(1); // 第二列，假设是字符串类型
                                string col3 = reader.GetString(2); // 第三列，假设是字符串类型
                                string col4 = reader.GetString(3); // 第四列，假设是字符串类型
                                DateTime col5 = reader.GetDateTime(4); // 最后一列，
                                addMessage($"\t{col1}\t{col2}\t{col3}\t{col4}\t{col5}");
                            }
                        }
                    }


                    break;
                case "写数据报警描述数据":
                    var res = await Global.Insert_pc_data_alarm("3213", "wangring", "1");
                    if (res == "ok")
                    {
                        addMessage($"警描述数据写入成功");
                    }
                    else
                    {
                        addMessage(res);
                    }
                    break;
                case "写工艺质量数据":

                    break;


            }
        }
        void ExecuteOpenFileCommand()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            //openFileDialog.Filter = "CSV文件 (*.csv)|*.csv";
            openFileDialog.Filter = "CSV 文件 (*.csv)|*.csv|所有文件 (*.*)|*.*";

            openFileDialog.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory + "报警文件"; // 可选：设置初始目录
            bool? result = openFileDialog.ShowDialog();

            if (result == true)
            {
                FileName = openFileDialog.FileName;
                // 这里可以继续处理选择的文件，比如读取文件内容等操作
                Properties.Settings.Default.Filer = FileName;
                Properties.Settings.Default.Save();
            }
        }
        void ExecuteAppLoadedEventCommand()
        {
            FileName = Properties.Settings.Default.Filer;
            addMessage("软件打开！");
            IP = Properties.Settings.Default.PLC;
            Task.Run(() =>
            {
                var r = pLC.Connect(Properties.Settings.Default.PLC);
                PLCState = r;

                if (!r)
                {
                    logger.Error("PLC连接失败");
                }
                else
                {
                    //读PLC
                    addMessage("PLC连接成功!");
                    Task.Run(() => PLCReadAction());
                }
            });
            Task.Run(() =>
            {
                MySqlConnection connection = new MySqlConnection(builder.ConnectionString);
                bool r = false;
                try
                {
                    // 打开连接
                    connection.Open();
                    // 检查连接状态
                    if (connection.State == System.Data.ConnectionState.Open)
                    {
                        r = true;
                        addMessage("数据库连接成功！");
                    }
                    else
                    {
                        addMessage("数据库连接失败！");
                    }
                    MysqlState = r;
                }

                catch (Exception ex)
                {
                    addMessage("连接到数据库时出错：" + ex.Message);
                }
            });
        }
        void ExecuteAppClosedEventCommand()
        {
            var res = Global.Insert_pc_data_emp(ID, Name, "OUTTIME", "下机");
            Settings.Default.Save();
            addMessage("软件关闭!");
            //pLC.Close();
        }
        #endregion
        #region 构造函数
        public MainWindowViewModel(IContainerProvider containerProvider)
        {
            pLC = containerProvider.Resolve<IPLCService>("PLC");//初始化
            _dialogService = containerProvider.Resolve<IDialogService>();
            MySQL_ConnectionMsg();
            Global.Ini();
            Add_SQL_data_alarm();
        }
        #endregion
        #region 功能函数
        private void addMessage(string str)
        {
            logger.Info(str);
            Debug.WriteLine(str);
            string[] s = MessageStr.Split('\n');
            if (s.Length > 1000)
            {
                MessageStr = "";
            }
            if (MessageStr != "")
            {
                MessageStr += "\n";
            }
            MessageStr += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " " + str;
        }
        private void PLCReadAction()
        {
            Task.Run(() => {
                while (true)
                {
                    if (pLC.Connected)
                    {
                        try
                        {
                            起始地址 = Settings.Default.报警起始地址;
                            地址个数 = Settings.Default.报警点位个数;
                            //读报警地址
                            var WarringAddr = pLC.ReadMCoils(起始地址, 地址个数);
                            var WarningFileMsg = File.ReadAllLines(FileName, Encoding.UTF8);
                            if (mPreWarning1 != null)
                            {
                                for (int i = 0; i < WarringAddr.Length; i++)
                                {
                                    if (WarringAddr[i] != mPreWarning1[i])
                                    {
                                        if (WarringAddr[i] == true)
                                        {
                                            addMessage(i + 起始地址 + "|报警触发");
                                            foreach (var item in WarningFileMsg)
                                            {
                                                string plcAddr = item.Split("\t")[0].Replace("M", "");
                                                string errorMsg = item.Split("\t")[1];
                                                if ((i + 起始地址).ToString() == plcAddr && errorMsg != "")
                                                {
                                                    addMessage(errorMsg);
                                                    WarningQueue.Enqueue(new SignalMsg()
                                                    {
                                                        WarningAddr = plcAddr,
                                                        WarningMsg = errorMsg,
                                                        WarningTrigger = 1
                                                    });
                                                }
                                            }
                                        }
                                        else
                                        {

                                            foreach (var item in WarningFileMsg)
                                            {
                                                string plcAddr = item.Split("\t")[0].Replace("M", "");
                                                string errorMsg = item.Split("\t")[1];
                                                if ((i + 起始地址).ToString() == plcAddr && errorMsg != "")
                                                {
                                                    addMessage(errorMsg);
                                                    WarningQueue.Enqueue(new SignalMsg()
                                                    {

                                                        WarningAddr = plcAddr,
                                                        WarningMsg = errorMsg,
                                                        WarningTrigger = 0
                                                    }); ;
                                                }
                                            }
                                            addMessage(i + 起始地址 + "|报警解除");
                                        }
                                    }
                                }
                            }
                            mPreWarning1 = WarringAddr;
                        }
                        catch (Exception ex)
                        {
                            addMessage(ex.ToString());
                        }
                    }
                    Thread.Sleep(100);
                }
            });
          
        }
        private  void Add_SQL_data_alarm()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    if (WarningQueue.Count > 0)
                    {
                        if (WarningQueue.TryDequeue(out var msg))
                        {
                            var res = await Global.Insert_pc_data_alarm(msg.WarningAddr, msg.WarningMsg, msg.WarningTrigger.ToString());
                            if (res == "ok")
                            {
                                addMessage("报警信息写入数据库成功！");
                            }
                            else { addMessage(res); }
                        }
                    }
                }
            });

        }
        #endregion
        #region 数据库功能函数
        public void MySQL_ConnectionMsg()//数据库连接的构建
        {
            //构建连接字符串
            builder.Server = Settings.Default.MySQL_Server;
            builder.Port = Convert.ToUInt32(Settings.Default.MySQL_Port);
            builder.UserID = Settings.Default.MySQL_UserID;
            builder.Password = Settings.Default.MySQL_Password;
            builder.Database = Settings.Default.MySQL_Database;
        }

        static bool CheckIfDatabaseExists(MySqlConnection connection, string databaseName)
        {
            string sql = $"SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '{databaseName}'";
            MySqlCommand command = new MySqlCommand(sql, connection);
            object result = command.ExecuteScalar();
            return result != null;
        }
        // 创建新的数据库
        static void CreateDatabase(MySqlConnection connection, string databaseName)
        {
            string sql = $"CREATE DATABASE `{databaseName}`";
            MySqlCommand command = new MySqlCommand(sql, connection);
            command.ExecuteNonQuery();
        }
        #endregion           
    }
    public class SignalMsg
    {
        public string WarningAddr { get; set; }
        public string WarningMsg { get; set; }
        public int WarningTrigger { get; set; }

    }
}


