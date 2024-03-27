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



namespace MonitorApp.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        #region 变量
        private readonly IPLCService pLC;
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private CancellationTokenSource source1;
        MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder();
        List<报警信息> 报警信息List = new List<报警信息>();
        List<上升沿> 上升沿List = new List<上升沿>();
        string cmd = "";
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

        private bool ButtonState;
        public bool buttonState
        {
            get { return ButtonState; }
            set { SetProperty(ref ButtonState, value); }
        }
        #endregion
        #region 命令绑定
        private DelegateCommand appLoadedEventCommand;
        public DelegateCommand AppLoadedEventCommand =>
            appLoadedEventCommand ?? (appLoadedEventCommand = new DelegateCommand(ExecuteAppLoadedEventCommand));
        private DelegateCommand appClosedEventCommand;
        public DelegateCommand AppClosedEventCommand =>
            appClosedEventCommand ?? (appClosedEventCommand = new DelegateCommand(ExecuteAppClosedEventCommand));
        private DelegateCommand openFileCommand;
        public DelegateCommand OpenFileCommand =>
            openFileCommand ?? (openFileCommand = new DelegateCommand(ExecuteOpenFileCommand));
        private DelegateCommand<string> testCommand;
        public DelegateCommand<string> TestCommand =>
            testCommand ?? (testCommand = new DelegateCommand<string>(ExecuteTestCommand));
        private DelegateCommand inOutTime;
        public DelegateCommand InOutTime =>
            inOutTime ?? (inOutTime = new DelegateCommand(ExecuteInOutTime));

        void ExecuteInOutTime()
        {

        }

        

        void ExecuteTestCommand(string obj)
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
                    Global.Insert_pc_data_alarm("3", "3", "3");
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
                    source1 = new CancellationTokenSource();
                    CancellationToken token = source1.Token;
                    Task.Run(() => PLCReadAction(token), token);
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
            addMessage("软件关闭!");
            pLC.Close();
        }
        #endregion
        #region 构造函数
        public MainWindowViewModel(IContainerProvider containerProvider)
        {
            pLC = containerProvider.Resolve<IPLCService>("PLC");//初始化
            MySQL_ConnectionMsg();
            Global.Ini();
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
        private void PLCReadAction(CancellationToken token)
        {

        }
        private void ReadWaring(string path)
        {
            if (pLC.Connected)
            {
                var lines = File.ReadAllLines(path);
                foreach (var line in lines)
                {
                    string[] res = line.Split('\t');
                    报警信息List.Add(new 报警信息()
                    {
                        报警地址 = ushort.Parse(line.Split('\t')[0].Replace("M", "")),
                        报警内容 = line.Split('\t')[1],
                    });
                    上升沿List.Add(new 上升沿());
                }
                ushort 起始地址 = 300;
                ushort 点位个数 = 200;
                while (true)
                {
                    var bools = pLC.ReadMCoils(起始地址, 点位个数);
                    for (int i = 0; i < 点位个数; i++)
                    {
                        上升沿List[i].SetValue(bools[i]);
                        if (上升沿List[i].GetResult())
                        {
                            var 报警信息 = 报警信息List.FirstOrDefault(a => a.报警地址 == i + 起始地址);
                            addMessage($"有过{报警信息.报警地址}:{报警信息.报警内容}报警");
                        }
                    }

                }
            }
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
        private void Insert_pc_data_craft(string 批次号, string 版号, string 项目代码, string 项目描述, string 单位, string 采集值, string isok)//工艺质量
        {
            var connection = new MySqlConnection(builder.ConnectionString);
            try
            {
                // 打开连接
                connection.Open();
                // 执行插入数据的 SQL 语句
                string sql = "INSERT INTO pc_data_craft (CONTAINER, PNLIDORSETID, ITEMCODE, ITEMDES, UOM, ITEMVALUE, ISOK, GROUPNUM, CREATETIME) " +
                             "VALUES (@Container, @PnlIdOrSetId, @ItemCode, @ItemDes, @Uom, @ItemValue, @IsOk, @GroupNum, @CreateTime)";
                MySqlCommand command = new MySqlCommand(sql, connection);

                // 添加参数
                command.Parameters.AddWithValue("@Container", 批次号);
                command.Parameters.AddWithValue("@PnlIdOrSetId", 版号);
                command.Parameters.AddWithValue("@ItemCode", 项目代码);
                command.Parameters.AddWithValue("@ItemDes", 项目描述);
                command.Parameters.AddWithValue("@Uom", 单位);
                command.Parameters.AddWithValue("@ItemValue", 采集值);
                command.Parameters.AddWithValue("@IsOk", isok);
                command.Parameters.AddWithValue("@GroupNum", DateTime.Now.ToString("yyyyMMddhhmmssff"));
                command.Parameters.AddWithValue("@CreateTime", DateTime.Now);

                command.ExecuteNonQuery();

                Console.WriteLine("工艺质量添加成功！");
            }
            catch (Exception ex)
            {
                Console.WriteLine("工艺质量添加数据时出错：" + ex.Message);
            }
            finally
            {
                // 关闭连接
                if (connection.State == System.Data.ConnectionState.Open)
                    connection.Close();
            }
        }
        private void Insert_pc_data_emp(string 员工号, string 员工姓名, string 操作代码, string 操作名称)//员工上下机记录上传
        {
            var connection = new MySqlConnection(builder.ConnectionString);
            try
            {
                // 打开连接
                connection.Open();
                // 执行插入数据的 SQL 语句
                string sql = "INSERT INTO pc_data_emp (EMPNO, EMPNAME, EMPCODE, EMPDES, EMPTIME, CREATETIME) " +
                             "VALUES (@EmpNo, @EmpName, @EmpCode, @EmpDes, @EmpTime, @CreateTime)";
                MySqlCommand command = new MySqlCommand(sql, connection);

                // 添加参数
                command.Parameters.AddWithValue("@EmpNo", 员工号);
                command.Parameters.AddWithValue("@EmpName", 员工姓名);
                command.Parameters.AddWithValue("@EmpCode", 操作代码);//(上机代码：INTIME  下机代码：OUTTIME)
                command.Parameters.AddWithValue("@EmpDes", 操作名称);//上机时间  下机时间
                command.Parameters.AddWithValue("@EmpTime", DateTime.Now);
                command.Parameters.AddWithValue("@CreateTime", DateTime.Now);

                command.ExecuteNonQuery();

                Console.WriteLine("员工上下机记录数据添加成功！");
            }
            catch (Exception ex)
            {
                Console.WriteLine("员工上下机记录添加数据时出错：" + ex.Message);
            }
            finally
            {
                // 关闭连接
                if (connection.State == System.Data.ConnectionState.Open)
                    connection.Close();
            }
        }
        private void Insert_pc_signal_status(string 信号代码, string 信号描述, string 信号值)//设备实时信号
        {
            // 创建 MySqlConnection 对象
            MySqlConnection connection = new MySqlConnection(builder.ConnectionString);

            try
            {
                // 打开连接
                connection.Open();

                // 执行插入数据的 SQL 语句
                string sql = "INSERT INTO pc_signal_status (SIGNALCODE, SIGNALDES, SIGNALVALUE, CREATETIME) " +
                             "VALUES (@SignalCode, @SignalDes, @SignalValue, @CreateTime)";
                MySqlCommand command = new MySqlCommand(sql, connection);

                // 添加参数
                command.Parameters.AddWithValue("@SignalCode", 信号代码);
                command.Parameters.AddWithValue("@SignalDes", 信号描述);
                command.Parameters.AddWithValue("@SignalValue", 信号值);
                command.Parameters.AddWithValue("@CreateTime", DateTime.Now);

                command.ExecuteNonQuery();

                Console.WriteLine("设备实时信号数据添加成功！");
            }
            catch (Exception ex)
            {
                Console.WriteLine("设备实时信号添加数据时出错：" + ex.Message);
            }
            finally
            {
                // 关闭连接
                if (connection.State == System.Data.ConnectionState.Open)
                    connection.Close();
            }
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
}
public class 报警信息
{
    public ushort 报警地址 { get; set; }
    public string 报警内容 { get; set; }
    public bool value { get; set; }
}

public class 上升沿
{
    private bool lastValue = false;
    private bool currentValue = false;

    public void SetValue(bool value)
    {
        lastValue = currentValue;
        currentValue = value;
    }

    public bool GetResult()
    {
        return currentValue && currentValue != lastValue;
    }
}

public class 下升沿
{
    private bool lastValue = false;
    private bool currentValue = false;

    public void SetValue(bool value)
    {
        lastValue = currentValue;
        currentValue = value;
    }

    public bool GetResult()
    {
        return !currentValue && currentValue != lastValue;
    }
}
