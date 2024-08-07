﻿using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonitorApp.Properties;
using System.Windows.Controls;
using NLog;
using Mysqlx;
using MySqlX.XDevAPI.Common;
using System.IO;

namespace MonitorApp.Model
{
    public static class Global
    {
        #region 变量
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public static MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder();
        public static string WarningAddr = "";
        public static string[] WarningMsg = null;

        public static string GlobalFile = AppDomain.CurrentDomain.BaseDirectory + "Count.ini";
        #endregion
        #region 参数属性

        //static string mBanci = "080000";
        //public static string Banci
        //{
        //    get { return mBanci; }
        //    set
        //    {
        //        mBanci = value;
        //        DXH.Ini.DXHIni.WritePrivateProfileString("System", "Banci", mBanci, GlobalFile);
        //    }
        //}
        static int mRunTimes = 0;
        public static int RunTimes
        {
            get { return mRunTimes; }
            set
            {
                mRunTimes = value;
                DXH.Ini.DXHIni.WritePrivateProfileString("System", "RunTimes", mRunTimes.ToString(), GlobalFile);
            }
        }
        static int mStopTimes = 0;
        public static int StopTimes
        {
            get { return mStopTimes; }
            set
            {
                mStopTimes = value;
                DXH.Ini.DXHIni.WritePrivateProfileString("System", "StopTimes", mStopTimes.ToString(), GlobalFile);
            }
        }
        static int mWaitTimes = 0;
        public static int WaitTimes
        {
            get { return mWaitTimes; }
            set
            {
                mWaitTimes = value;
                DXH.Ini.DXHIni.WritePrivateProfileString("System", "WaitTimes", mWaitTimes.ToString(), GlobalFile);
            }
        }
        #endregion
        public static void Ini()
        {
            NlogConfig();
            //LoadIni();
        }
        public static void NlogConfig()
        {
            var config = new NLog.Config.LoggingConfiguration();

            // Targets where to log to: File and Console
            //应用程序当前目录
            //短日期【2022-01-06】

            //长日期【2022-01-06 14:05:20.4023】
            //记录等级【Trace，Debug，Info，Warn，Error，Fatal】
            //调用Nlog时输入的内容
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = "${basedir}/logs/${shortdate}.log", Layout = "${longdate}|${level:uppercase=true}|${message}" };
            var logconsole = new NLog.Targets.ConsoleTarget("logconsole");

            // Rules for mapping loggers to targets
            // 该方法所设置节点的属性必须关联其它的兄弟节点或者属性为布尔值
            config.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, logconsole);
            config.AddRule(NLog.LogLevel.Debug, NLog.LogLevel.Fatal, logfile);

            // Apply config           
            NLog.LogManager.Configuration = config;
        }
        #region 数据库处理
        public static void MySQL_ConnectionMsg()//数据库连接的构建
        {
            //构建连接字符串
            builder.Server = Settings.Default.MySQL_Server;
            builder.Port = Convert.ToUInt32(Settings.Default.MySQL_Port);
            builder.UserID = Settings.Default.MySQL_UserID;
            builder.Password = Settings.Default.MySQL_Password;
            builder.Database = Settings.Default.MySQL_Database;
            builder.CharacterSet = "gb2312";
        }
        public static string Insert_pc_data_emp(string 员工号, string 员工姓名, string 操作代码, string 操作名称)//员工上下机记录上传
        {
            MySQL_ConnectionMsg();
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

                logger.Debug("员工上下机记录数据添加成功！");
                return "ok";
            }
            catch (Exception ex)
            {
                logger.Debug("员工上下机记录添加数据时出错---" + ex.Message);
                return "ng";
            }
            finally
            {
                // 关闭连接
                if (connection.State == System.Data.ConnectionState.Open)
                    connection.Close();
            }
        }
        public static async Task<string> Insert_pc_data_alarm(string 报警代码, string 报警描述, string value)//设备报警描述
        {
            await Task.Delay(100);
            MySQL_ConnectionMsg();
            try
            {

                using (var connection = new MySqlConnection(builder.ConnectionString))
                {
                    // 打开连接
                    await connection.OpenAsync();

                    // 执行插入数据的 SQL 语句
                    string sql = "INSERT INTO pc_data_alarm (ALARMCODE, ALARMDES, ALARMVALUE, CREATETIME) " +
                                 "VALUES (@AlarmCode, @AlarmDes, @AlarmValue, @CreateTime)";
                    MySqlCommand command = new MySqlCommand(sql, connection);

                    // 添加参数
                    command.Parameters.AddWithValue("@AlarmCode", 报警代码);
                    command.Parameters.AddWithValue("@AlarmDes", 报警描述);
                    command.Parameters.AddWithValue("@AlarmValue", value);//报警1， 复位0
                    command.Parameters.AddWithValue("@CreateTime", DateTime.Now);

                    await command.ExecuteNonQueryAsync();

                    logger.Debug("设备报警描述数据添加成功！");
                    return "ok";
                }
            }
            catch (Exception ex)
            {
                logger.Error("设备报警描述异常---" + ex);
                return ex.ToString();
            }

        }
        public static async Task<string> Insert_pc_signal_status(string 信号代码, string 信号描述, int 信号值)//设备实时信号
        {
            await Task.Delay(100);
            MySQL_ConnectionMsg();
            // 创建 MySqlConnection 对象
            using (var connection = new MySqlConnection(builder.ConnectionString))
            {
                try
                {

                    // 打开连接
                    await connection.OpenAsync();

                    // 执行插入数据的 SQL 语句
                    string sql = "INSERT INTO pc_signal_status (SIGNALCODE, SIGNALDES, SIGNALVALUE, CREATETIME) " +
                                 "VALUES (@SignalCode, @SignalDes, @SignalValue, @CreateTime)";
                    MySqlCommand command = new MySqlCommand(sql, connection);

                    // 添加参数
                    command.Parameters.AddWithValue("@SignalCode", 信号代码);
                    command.Parameters.AddWithValue("@SignalDes", 信号描述);
                    command.Parameters.AddWithValue("@SignalValue", 信号值);
                    command.Parameters.AddWithValue("@CreateTime", DateTime.Now);

                    await command.ExecuteNonQueryAsync();

                    logger.Debug("设备实时信号数据添加成功！");
                    return "ok";
                }
                catch (Exception ex)
                {
                    logger.Error("设备实时信号添加数据时出错---" + ex.Message);
                    return "ng";
                }
            }
        }
        public static async Task<string> Insert_pc_data_craft(string 项目代码, string 项目描述, string 单位, string 采集值, string 时间组号, DateTime 时间, string 批次号 = "", string 版号 = "", bool isok = false)//工艺质量
        {
            await Task.Delay(100);
            MySQL_ConnectionMsg();

            using (var connection = new MySqlConnection(builder.ConnectionString))
            {
                try
                {
                    await connection.OpenAsync();

                    string sql = "INSERT INTO pc_data_craft (CONTAINER, PNLIDORSETID, ITEMCODE, ITEMDES, UOM, ITEMVALUE, ISOK, GROUPNUM, CREATETIME) " +
                                 "VALUES (@Container, @PnlIdOrSetId, @ItemCode, @ItemDes, @Uom, @ItemValue, @IsOk, @GroupNum, @CreateTime)";
                    MySqlCommand command = new MySqlCommand(sql, connection);

                    command.Parameters.AddWithValue("@Container", 批次号);
                    command.Parameters.AddWithValue("@PnlIdOrSetId", 版号);
                    command.Parameters.AddWithValue("@ItemCode", 项目代码);
                    command.Parameters.AddWithValue("@ItemDes", 项目描述);
                    command.Parameters.AddWithValue("@Uom", 单位);
                    command.Parameters.AddWithValue("@ItemValue", 采集值);
                    command.Parameters.AddWithValue("@IsOk", Convert.ToInt16(isok));
                    command.Parameters.AddWithValue("@GroupNum", long.Parse( 时间组号));
                    //command.Parameters.AddWithValue("@GroupNum", DateTime.Now.ToString("yyyyMMddhhmmssff"));
                    //command.Parameters.AddWithValue("@CreateTime", DateTime.Now);
                    command.Parameters.AddWithValue("@CreateTime", 时间);

                    await command.ExecuteNonQueryAsync();
                    logger.Debug("工艺质量添加数据成功！");
                    return "ok"; // 设置为成功
                }
                catch (Exception ex)
                {
                    logger.Error("工艺质量添加数据时出错---" + ex.Message);
                    return ex.ToString();
                }
            }

        }

        public static void LoadIni()
        {
            if (File.Exists(GlobalFile))
            {
                //DXH.Ini.DXHIni.ContentReader("System", "Banci", "Null", GlobalFile);
                DXH.Ini.DXHIni.TryToInt(ref mRunTimes, DXH.Ini.DXHIni.ContentReader("System", "RunTimes", "Null", GlobalFile));
                DXH.Ini.DXHIni.TryToInt(ref mStopTimes, DXH.Ini.DXHIni.ContentReader("System", "StopTimes", "Null", GlobalFile));
                DXH.Ini.DXHIni.TryToInt(ref mWaitTimes, DXH.Ini.DXHIni.ContentReader("System", "WaitTimes", "Null", GlobalFile));
            }
            else
            {
                //Banci = mBanci;
                RunTimes = mRunTimes;
                StopTimes = mStopTimes;
                WaitTimes = mWaitTimes;
            }
        }
        #endregion

    }
}
