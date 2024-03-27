using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonitorApp.Properties;
using MySql.Data.MySqlClient;
using System.Windows.Controls;
using NLog;

namespace MonitorApp.Model
{
    public static class Global
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public static  MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder();
        public static void Ini()
        {
            NlogConfig();
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
       
        public static async void Insert_pc_data_alarm(string 报警代码, string 报警描述, string value)//设备报警描述
        {
            await Task.Delay(100);
            Task data_alarm = Task.Run(() => {
                var connection = new MySqlConnection(builder.ConnectionString);
                try
                {
                    // 打开连接
                    connection.Open();
                    // 执行插入数据的 SQL 语句
                    string sql = "INSERT INTO pc_data_alarm (ALARMCODE, ALARMDES, ALARMVALUE, CREATETIME) " +
                                 "VALUES (@AlarmCode, @AlarmDes, @AlarmValue, @CreateTime)";
                    MySqlCommand command = new MySqlCommand(sql, connection);
                    // 添加参数
                    command.Parameters.AddWithValue("@AlarmCode", 报警代码);
                    command.Parameters.AddWithValue("@AlarmDes", 报警描述);
                    command.Parameters.AddWithValue("@AlarmValue", value);//报警1， 复位0
                    command.Parameters.AddWithValue("@CreateTime", DateTime.Now);

                    command.ExecuteNonQuery();

                    logger.Debug("设备报警描述数据添加成功！");
                }
                catch (Exception ex)
                {
                    logger.Error("设备报警描述异常" + ex);
                }
                finally
                {
                    // 关闭连接
                    if (connection.State == System.Data.ConnectionState.Open)
                        connection.Close();
                }
            });
           
        }
    }
}
