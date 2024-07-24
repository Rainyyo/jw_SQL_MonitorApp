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
using System.Timers;
using System.Formats.Asn1;
using Org.BouncyCastle.Utilities.IO.Pem;
using Serilog;
using System.Xml;

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
        bool mPreLoadState = false;//待料
        bool mPreProducting = false;//生产
        bool mPrePause = false;//暂停
        bool mPreStop = false;//停止
        bool IsMonitor = false;

        private System.Timers.Timer MonitorMachineStatustimer;
        private System.Timers.Timer timer;
        private DateTime NextMorningReset;
        private DateTime NextEveningReset;
        private int RunTimes = 0;
        private int StopTimes = 0;
        private int WaitTimes = 0;
        #endregion
        #region 属性
        private string _title = "MonitorApp";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }
        public string Version { get; set; } = "1.0.0";
        private string appName;
        public string AppName
        {
            get { return appName; }
            set { SetProperty(ref appName, value); }
        }
        //日志打印
        private string messageStr = string.Empty;
        public string MessageStr
        {
            get { return messageStr; }
            set { SetProperty(ref messageStr, value); }
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

        private string banci;
        public string Banci
        {
            get { return banci; }
            set { SetProperty(ref banci, value); }
        }

        private string lOT;
        public string LOT
        {
            get { return lOT; }
            set { SetProperty(ref lOT, value); }
        }
        private string sETID;
        public string SETID
        {
            get { return sETID; }
            set { SetProperty(ref sETID, value); }
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
        private DelegateCommand<object> textBoxLostFocusCommand;
        public DelegateCommand<object> TextBoxLostFocusCommand =>
            textBoxLostFocusCommand ?? (textBoxLostFocusCommand = new DelegateCommand<object>(ExecuteTextBoxLostFocusCommand));
        void ExecuteTextBoxLostFocusCommand(object obj)
        {
            if (obj is string str)
            {
                switch (str)
                {
                    case "批次号":
                        Settings.Default.LOT = LOT;
                        break;

                    case "板号":
                        Settings.Default.SETID = SETID;
                        break;
                }
                Properties.Settings.Default.Save();
            }
        }
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
            if (ID == null)
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
                case "test":
                    var a = pLC.ReadDRegisters(1, 1);
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
            LOT = Settings.Default.LOT;
            SETID=Settings.Default.SETID;
            addMessage("软件打开！");
            Task.Run(() =>
            {
                var r = pLC.Connect(Properties.Settings.Default.PLC);
                PLCState = r;

                if (!r)
                {
                    logger.Error($"PLC：" + Properties.Settings.Default.PLC + "连接失败");
                }
                else
                {
                    IsMonitor = true;
                    //读PLC
                    addMessage("PLC：" + Properties.Settings.Default.PLC + "连接成功!");

                    Task.Run(() => Monitor_PLC_State());//设备运行状态
                    Task.Run(() => PLC_ReadWarningAction());//设备报警信号
                    //PLC_ReadWarningAction();

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
                    addMessage("连接到数据库时出错！");
                }
            });
            var time1 = DateTime.Now;
            var time2 = time1.ToString("yyyyMMddhhmmssff");
            var value = time1.ToString("yyMMdd");
            var res1 =  Global.Insert_pc_data_craft("VER_DATE", "软件版本", "/", value, time2, time1, LOT,SETID);
            
        }
        void ExecuteAppClosedEventCommand()
        {
            if (ID != null)
            {
                var res = Global.Insert_pc_data_emp(ID, Name, "OUTTIME", "下机");
            }
            Settings.Default.Save();
            addMessage("软件关闭!");
            pLC.Close();
        }
        #endregion
        #region 构造函数
        public MainWindowViewModel(IContainerProvider containerProvider)
        {
            LOT = Settings.Default.LOT;
            SETID = Settings.Default.SETID;
            pLC = containerProvider.Resolve<IPLCService>("PLC");//初始化
            _dialogService = containerProvider.Resolve<IDialogService>();
            AppName = Settings.Default.AppName;
            MySQL_ConnectionMsg();
            Global.Ini();

            //Banci = DXH.Ini.DXHIni.ContentReader("System", "Banci", "Null", System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Count.ini"));
            //RunTimes = Convert.ToInt32(DXH.Ini.DXHIni.ContentReader("System", "RunTimes", "Null", System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Count.ini")));
            //StopTimes = Convert.ToInt32(DXH.Ini.DXHIni.ContentReader("System", "StopTimes", "Null", System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Count.ini")));
            //WaitTimes = Convert.ToInt32(DXH.Ini.DXHIni.ContentReader("System", "WaitTimes", "Null", System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Count.ini")));


            Add_SQL_data_alarm();
            //SetTimer(); //换班计时器
            SetTimer2();//设备状态监控计时器
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
        private void PLC_ReadWarningAction()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    if (pLC.Connected)
                    {
                        try
                        {
                            起始地址 = Settings.Default.报警起始地址;
                            地址个数 = Settings.Default.报警点位个数;
                            //读报警地址
                            var WarningFileMsg = File.ReadAllLines(FileName, Encoding.UTF8);
                            var WarringAddr = pLC.ReadMCoils(起始地址, 地址个数);

                            if (mPreWarning1 != null)
                            {
                                for (int i = 0; i < WarringAddr.Length; i++)
                                {
                                    if (WarringAddr[i] != mPreWarning1[i])
                                    {
                                        if (WarringAddr[i] == true)
                                        {
                                            addMessage("M" + (i + 起始地址) + "|报警触发");
                                            foreach (var item in WarningFileMsg)
                                            {
                                                string plcAddr = item.Split(",")[0].Replace("M", "");
                                                //string plcAddr = item.Split(",")[0];
                                                string errorMsg = item.Split(",")[1];
                                                if ((i + 起始地址).ToString() == plcAddr && errorMsg != "")
                                                {
                                                    addMessage(errorMsg);
                                                    WarningQueue.Enqueue(new SignalMsg()
                                                    {
                                                        WarningAddr = "M" + plcAddr,
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
                                                string plcAddr = item.Split(",")[0].Replace("M", "");
                                                string errorMsg = item.Split(",")[1];
                                                if ((i + 起始地址).ToString() == plcAddr && errorMsg != "")
                                                {
                                                    addMessage(errorMsg);
                                                    WarningQueue.Enqueue(new SignalMsg()
                                                    {

                                                        WarningAddr = "M" + plcAddr,
                                                        WarningMsg = errorMsg,
                                                        WarningTrigger = 0
                                                    }); ;
                                                }
                                            }
                                            addMessage("M" + (i + 起始地址) + "|报警解除");
                                        }
                                    }
                                }
                            }
                            mPreWarning1 = WarringAddr;
                        }
                        catch (Exception ex)
                        {
                            Thread.Sleep(50000);
                            addMessage("PLC读报警文件失败！" + ex);
                        }
                    }
                    Thread.Sleep(200);
                }
            });

        }
        private void Add_SQL_data_alarm()
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
                            else { addMessage("报警信息写入数据库失败！"); }
                        }
                    }
                    Thread.Sleep(200);
                }
            });

        }
        private void Monitor_PLC_State()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        if (pLC.Connected)
                        {
                            var LoadSatet = pLC.ReadMCoils(Settings.Default.待料, 1);
                            var Producting = pLC.ReadMCoils(Settings.Default.生产, 1);
                            var Pause = pLC.ReadMCoils(Settings.Default.暂停, 1);
                            var Stop = pLC.ReadMCoils(Settings.Default.急停, 1);
                            #region 待料
                            if (LoadSatet[0] != mPreLoadState)
                            {
                                if (LoadSatet[0])
                                {
                                    addMessage("待料中");
                                    var res = await Global.Insert_pc_signal_status("WAITMATERIAL", "设备待料", 1);
                                    if (res == "ok")
                                    {
                                        addMessage("触发|待料信息写入成功！");
                                    }
                                    else
                                    {
                                        addMessage("触发|待料信息写入失败！");
                                    }
                                }
                                else
                                {
                                    var res = await Global.Insert_pc_signal_status("WAITMATERIAL", "设备待料", 0);
                                    if (res == "ok")
                                    {
                                        addMessage("复位|待料信息写入成功！");
                                    }
                                    else
                                    {
                                        addMessage("复位|待料信息写入失败！");
                                    }
                                }
                                mPreLoadState = LoadSatet[0];
                            }
                            #endregion
                            #region 生产
                            if (Producting[0] != mPreProducting)
                            {
                                if (Producting[0])
                                {
                                    addMessage("生产中");
                                    var res = await Global.Insert_pc_signal_status("PRODUCT", "生产开始", 1);
                                    if (res == "ok")
                                    {
                                        addMessage("触发|生产信息写入成功！");
                                    }
                                    else
                                    {
                                        addMessage("触发|生产信息写入失败！");
                                    }
                                }
                                else
                                {
                                    var res = await Global.Insert_pc_signal_status("PRODUCT", "生产结束", 0);
                                    if (res == "ok")
                                    {
                                        addMessage("复位|生产信息写入成功！");
                                    }
                                    else
                                    {
                                        addMessage("复位|生产信息写入失败！");
                                    }
                                }
                                mPreProducting = Producting[0];
                            }

                            #endregion
                            #region 暂停
                            //if (Pause[0] != mPrePause)
                            //{
                            //    if (Pause[0])
                            //    {
                            //        addMessage("暂停中");
                            //        var res = await Global.Insert_pc_signal_status((Settings.Default.暂停).ToString(), "暂停", 1);
                            //        if (res == "ok")
                            //        {
                            //            addMessage("触发|暂停信息写入成功！");
                            //        }
                            //        else
                            //        {
                            //            addMessage("触发|暂停信息写入失败！");
                            //        }
                            //    }
                            //    else
                            //    {
                            //        var res = await Global.Insert_pc_signal_status((Settings.Default.生产).ToString(), "暂停", 0);
                            //        if (res == "ok")
                            //        {
                            //            addMessage("复位|暂停信息写入成功！");
                            //        }
                            //        else
                            //        {
                            //            addMessage("复位|暂停信息写入失败！");
                            //        }
                            //    }
                            //    mPrePause = Pause[0];
                            //}
                            #endregion
                            #region 急停
                            if (Stop[0] != mPreStop)
                            {
                                if (Stop[0])
                                {
                                    addMessage("急停中");
                                    var res = await Global.Insert_pc_signal_status("EMERGENCYSTOP", "紧急停止", 1);
                                    if (res == "ok")
                                    {
                                        addMessage("触发|急停信息写入成功！");
                                    }
                                    else
                                    {
                                        addMessage("触发|急停信息写入失败！");
                                    }

                                }
                                else
                                {
                                    var res = await Global.Insert_pc_signal_status("EMERGENCYSTOP", "紧急停止", 0);
                                    if (res == "ok")
                                    {
                                        addMessage("复位|急停信息写入成功！");
                                    }
                                    else
                                    {
                                        addMessage("复位|急停信息写入失败！");
                                    }
                                }
                                mPreStop = Stop[0];
                            }
                            #endregion
                        }
                        Thread.Sleep(200);
                    }
                    catch (Exception ex)
                    {
                        Thread.Sleep(5000);
                        addMessage("监控PLC机台信号失败：" + ex);
                    }

                }
            });
        }
        private void SetTimer()
        {
            timer = new System.Timers.Timer(1);
            timer.Elapsed += new System.Timers.ElapsedEventHandler(Timer_Elapsed);
            timer.AutoReset = true;
            timer.Enabled = true;
        }

        private void SetTimer2()
        {
            MonitorMachineStatustimer = new System.Timers.Timer(60000);
            MonitorMachineStatustimer.Elapsed += MonitorMachineStatus_Elapsed;
            MonitorMachineStatustimer.AutoReset = true;
            MonitorMachineStatustimer.Enabled = true;
        }

        private async void MonitorMachineStatus_Elapsed(object sender, ElapsedEventArgs e)
        {
            var time1 = DateTime.Now;
            var time2 = time1.ToString("yyyyMMddhhmmssff");

            //var LoadSatet = pLC.ReadMCoils(Settings.Default.待料, 1);
            //var Producting = pLC.ReadMCoils(Settings.Default.生产, 1);
            //var Pause = pLC.ReadMCoils(Settings.Default.暂停, 1);
            //var Stop = pLC.ReadMCoils(Settings.Default.急停, 1);
            var Producting = pLC.ReadDRegisters(7062,1);//运行
            var LoadSatet = pLC.ReadDRegisters(7063,1);//待机
            var Pause = pLC.ReadDRegisters(7064,1);//报警
            //if (LoadSatet[0])
            //{
            //    WaitTimes++;
            //}
            //if (Producting[0])
            //{
            //    RunTimes++;
            //}
            //if (Pause[0])
            //{
            //    StopTimes++;
            //}
            //WriteFile();
            var res1 = await Global.Insert_pc_data_craft("WAIT_TIME", "当班待机时间", "min", LoadSatet[0].ToString(), time2, time1,LOT,SETID);
            if (res1 == "ok")
            {
                //addMessage("当班待机时间写入成功！");
            }
            else
            {
                addMessage("当班待机时间写入成功失败！" + res1);
            }
            var res2 = await Global.Insert_pc_data_craft("RUN_TIME", "当班运行时间", "min", Producting[0].ToString(), time2, time1, LOT, SETID);
            if (res2 == "ok")
            {
                //addMessage("当班运行时间写入成功！");
            }
            else
            {
                addMessage("当班运行时间写入成功失败！" + res2);
            }
            var res3 = await Global.Insert_pc_data_craft("ALARM_TIME", "当班报警时间", "min", Pause[0].ToString(), time2, time1, LOT, SETID);
            if (res3 == "ok")
            {
                //addMessage("当班报警时间写入成功！");
            }
            else
            {
                addMessage("当班报警时间写入成功失败！" + res3);
            }

        }
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var _banci = GetBanci();
            if (_banci == "08000000" || _banci == "20000000")
            {
                //Banci = _banci;
                addMessage($"换班数据清零");
                RunTimes = 0;
                StopTimes = 0;
                WaitTimes = 0;
                WriteFile();
            }
        }

        void WriteFile()
        {
            try
            {
                DXH.Ini.DXHIni.WritePrivateProfileString("System", "RunTimes", RunTimes.ToString(), System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Count.ini"));
                DXH.Ini.DXHIni.WritePrivateProfileString("System", "StopTimes", StopTimes.ToString(), System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Count.ini"));
                DXH.Ini.DXHIni.WritePrivateProfileString("System", "WaitTimes", WaitTimes.ToString(), System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Count.ini"));
            }
            catch (Exception ex)
            {
                addMessage("WriteFile_Ini"+ex.Message);
            }
        }
        public string GetBanci()
        {
            string rs = "";
            if (DateTime.Now.Hour >= 8 && DateTime.Now.Hour < 20)
            {
                //rs += DateTime.Now.ToString("yyyyMMddHHmmssff") ;
                rs += DateTime.Now.ToString("HHmmssff");
            }
            else
            {
                if (DateTime.Now.Hour >= 0 && DateTime.Now.Hour < 8)
                {
                    //rs += DateTime.Now.AddDays(-1).ToString("yyyyMMddHHmmssff") ;
                    rs += DateTime.Now.AddDays(-1).ToString("HHmmssff");
                }
                else
                {
                    //rs += DateTime.Now.ToString("yyyyMMddHHmmssff");
                    rs += DateTime.Now.ToString("HHmmssff");
                }
            }
            return rs;
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
            builder.CharacterSet = "gb2312";
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


