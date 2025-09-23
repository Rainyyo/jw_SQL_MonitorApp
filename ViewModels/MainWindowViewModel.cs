using Google.Protobuf;
using Microsoft.VisualBasic.FileIO;
using Microsoft.Win32;
using MonitorApp.Model;
using MonitorApp.Properties;
using MonitorApp.Services;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using OfficeOpenXml;
using Org.BouncyCastle.Utilities.IO.Pem;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using RestSharp;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Formats.Asn1;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Markup;
using System.Xml;

namespace MonitorApp.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        #region 变量

        private readonly IDialogService _dialogService;
        private readonly IRegionManager regionManager;
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static Logger ngDataLogger = NLog.LogManager.GetLogger("NgDataLogger");
        private bool isParameterDialogShow = false;
        #endregion
        #region 属性
        private string _title = "手动上传MES";
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

        private bool isWindowFocused;
        public bool IsWindowFocused
        {
            get { return isWindowFocused; }
            set { SetProperty(ref isWindowFocused, value); }
        }


        private bool buttonState = false;
        public bool ButtonState
        {
            get { return buttonState; }
            set { SetProperty(ref buttonState, value); }
        }

        private string filePath;
        public string FilePath
        {
            get { return filePath; }
            set { SetProperty(ref filePath, value); }
        }


        private ObservableCollection<DataListDisp> dataList1;
        public ObservableCollection<DataListDisp> DataList1
        {
            get { return dataList1; }
            set { SetProperty(ref dataList1, value); }
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

        private DelegateCommand<object> buttonOperation;
        public DelegateCommand<object> ButtonOperation =>
            buttonOperation ?? (buttonOperation = new DelegateCommand<object>(ExecuteButtonOperation));

        private DelegateCommand<object> dataList11SelectCommand;
        public DelegateCommand<object> DataList11SelectCommand =>
            dataList11SelectCommand ?? (dataList11SelectCommand = new DelegateCommand<object>(ExecuteDataList11SelectCommand));

        void ExecuteDataList11SelectCommand(object obj)
        {

        }
        async void ExecuteButtonOperation(object obj)
        {
            if (obj is string str)
            {
                try
                {
                    switch (str)
                    {
                        case "未上传文件选择":

                            OpenFileDialog openFileDialog = new OpenFileDialog();
                            openFileDialog.Filter = "CSV文件 (*.csv)|*.csv";
                            openFileDialog.Filter = "CSV 文件 (*.csv)|*.csv|所有文件 (*.*)|*.*";

                            //openFileDialog.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory + "报警文件"; // 可选：设置初始目录
                            bool? result = openFileDialog.ShowDialog();
                            if (result == true)
                            {
                                FilePath = openFileDialog.FileName;
                            }
                            LoadDatainfo(FilePath);
                            break;
                        case "上传":
                            {
                                if (DataList1.Count > 0)
                                {
                                    string[] csvFiles = Directory.GetFiles(Settings.Default.FolderPath, "*.csv");
                                    var itemsToRemove = new List<DataListDisp>(); // 先记录要删除的项

                                    foreach (var bar in DataList1.ToList())
                                    {
                                        foreach (var barFileName in csvFiles)
                                        {
                                            string fileNameinfo = Path.GetFileName(barFileName);
                                            var fileName = fileNameinfo.Split("_");

                                            if (bar.Barcode == fileName[0])
                                            {
                                                var check_rst = JObject.Parse(await TestPostIn(bar.Barcode));

                                                if (check_rst["isSuccess"]?.ToString().Trim() == "True")
                                                {
                                                    var Electrical_dData = AnalyzeCSV(barFileName);
                                                    var postResult = JObject.Parse(await TestPostOut(bar.Barcode, Electrical_dData));
                                                    if (postResult["isSuccess"]?.ToString().Trim() == "True")
                                                    {
                                                        bool deleteSuccess = DeleteRowFromCSV(FilePath, bar.Barcode);
                                                        if (deleteSuccess)
                                                        {
                                                            itemsToRemove.Add(bar); // 记录要删除的项
                                                            addMessage($"条码：{bar.Barcode}上传成功");
                                                            logger.Info($"条码：{bar.Barcode}数据{postResult["message"]}");
                                                        }
                                                    }
                                                    foreach (var item in itemsToRemove)
                                                    {
                                                        DataList1.Remove(item);

                                                    }
                                                    // 如果需要，重新加载数据
                                                    if (itemsToRemove.Count > 0)
                                                    {
                                                        LoadDatainfo(FilePath);
                                                    }
                                                }
                                                else
                                                {
                                                    addMessage($"上传失败:{check_rst["message"]?.ToString()}");
                                                }

                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("请加载数据文件");
                                }
                            }
                            break;
                    }
                }
                catch (Exception ex)
                {
                    addMessage(ex.Message);
                    ngDataLogger.Error(ex.Message);
                }
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
        void ExecuteAppLoadedEventCommand()
        {
            regionManager.Regions["ParameterRegion"].RequestNavigate("Param");
            addMessage("软件打开！");
        }
        void ExecuteAppClosedEventCommand()
        {
            addMessage("软件关闭!");
        }
        #endregion
        #region 构造函数
        public MainWindowViewModel(IContainerProvider containerProvider, IRegionManager _regionManager)
        {
            _dialogService = containerProvider.Resolve<IDialogService>();
            regionManager = _regionManager;
            NlogConfig();
            DataList1 = new ObservableCollection<DataListDisp>();
        }
        #endregion
        #region 功能函数

        private void NlogConfig()
        {
            var config = new NLog.Config.LoggingConfiguration();

            // Targets where to log to: File and Console
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = "${basedir}/logs/${shortdate}.log", Layout = "${longdate}|${level:uppercase=true}|${message}" };
            //NG数据日志
            var ngDataLogfile = new NLog.Targets.FileTarget("ngDataLolog")
            {
                FileName = "${basedir}/NgDataLogger/${shortdate}.log",//日志文件名
                Layout = "${longdate}|${level:uppercase=true}|${logger}|${message}"
            };
            var logconsole = new NLog.Targets.ConsoleTarget("logconsole");

            // Rules for mapping loggers to targets            
            config.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, logconsole);
            config.AddRule(NLog.LogLevel.Debug, NLog.LogLevel.Fatal, logfile);


            config.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, ngDataLogfile, "NgDataLogger");

            // Apply config           
            NLog.LogManager.Configuration = config;
        }
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
        /// <summary>
        /// 加载NG数据
        /// </summary>
        /// <param name="filepath"></param>
        private void LoadDatainfo(string filepath)
        {
            DataList1.Clear();
            int index = 1;
            using (var parser = new TextFieldParser(filePath))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                parser.HasFieldsEnclosedInQuotes = true;

                // 跳过标题行
                if (!parser.EndOfData)
                    parser.ReadFields();

                while (!parser.EndOfData)
                {
                    string[] fields = parser.ReadFields();

                    if (fields.Length >= 2)
                    {
                        var data = new DataListDisp
                        {
                            Index = index++,
                            // 条码为空时赋null
                            Barcode = string.IsNullOrWhiteSpace(fields[0]) ? null : fields[0].Trim('"'),
                            Time = string.IsNullOrWhiteSpace(fields[1]) ?
                                DateTime.MinValue : // 或者使用 null，如果Time属性是DateTime?类型
                                DateTime.ParseExact(fields[1].Trim(), "yyyy/M/d HH:mm", CultureInfo.InvariantCulture),
                            State = "未上传"
                        };
                        DataList1.Add(data);
                    }

                }
            }
        }
        /// <summary>
        /// 上传成功删除对应条码
        /// </summary>
        /// <param name="csvFilePath"></param>
        /// <param name="barcode"></param>
        /// <returns></returns>
        private bool DeleteRowFromCSV(string csvFilePath, string barcode)
        {
            try
            {
                var lines = new List<string>();
                bool headerSkipped = false;
                bool found = false;

                // 读取所有行
                using (var reader = new StreamReader(csvFilePath))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (!headerSkipped)
                        {
                            // 保留标题行
                            lines.Add(line);
                            headerSkipped = true;
                            continue;
                        }

                        // 检查是否包含目标条码
                        if (line.Contains(barcode))
                        {
                            found = true;
                            continue; // 跳过这一行（即删除）
                        }

                        lines.Add(line);
                    }
                }

                // 如果找到了目标行，重新写入文件
                if (found)
                {
                    using (var writer = new StreamWriter(csvFilePath, false, Encoding.UTF8))
                    {
                        foreach (var line in lines)
                        {
                            writer.WriteLine(line);
                        }
                    }
                    return true;
                }

                return false; // 没有找到目标行
            }
            catch (Exception ex)
            {
                ngDataLogger.Error($"删除CSV行失败: {ex.Message}");
                return false;
            }
        }
        /// <summary>
        /// MES进站检
        /// </summary>
        /// <param name="bar"></param>
        /// <param name="postMethod"></param>
        /// <returns></returns>
        public async Task<string> TestPostIn(string bar)
        {
            string postData = "";
            JObject resoult = new JObject();
            resoult["isSuccess"] = "True";

            //Uri uri = new Uri(Settings.Default.URL_PostIn);
            //string baseUrl = $"{uri.Scheme}://{uri.Host}:{uri.Port}";
            //string apiPath = uri.AbsolutePath;
            //var MHToptions = new RestClientOptions(baseUrl);
            //var MHTclient = new RestClient(MHToptions);
            //var mHTrequest = new RestRequest(apiPath, RestSharp.Method.Post);
            //mHTrequest.AddHeader("Content-Type", "application/json");
            //mHTrequest.AddHeader("tenant-id", "1");
            JObject mhtjob = new JObject();
            mhtjob["locationNo"] = "1";//位置码
            mhtjob["equipmentId"] = Settings.Default.Tary;//设备编码
            mhtjob["testId"] = Settings.Default.ID;//工序码
            mhtjob["materialSerinalNo"] = bar;//SN码

            //mHTrequest.AddBody(JsonConvert.SerializeObject(mhtjob));

            postData = JsonConvert.SerializeObject(mhtjob);
            //RestResponse responseMHT = await MHTclient.ExecuteAsync(mHTrequest);
            //JObject mht = JObject.Parse(responseMHT.Content);

            //resoult["isSuccess"] = mht["code"].ToString().Trim().ToUpper() == "0" ? "TRUE" : "FALSE";
            //resoult["result"] = mht["code"].ToString().Trim().ToUpper();
            //resoult["message"] = mht.ToString();

            return resoult.ToString();
        }

        public async Task<string> TestPostOut(string bar, TestData data)
        {
            string postData = "";
            JObject resoult = new JObject();
            resoult["isSuccess"] = "True";

            //Uri uri = new Uri(Settings.Default.URL_PostOut);
            //string baseUrl = $"{uri.Scheme}://{uri.Host}:{uri.Port}";
            //string apiPath = uri.AbsolutePath;
            //var MHToptions = new RestClientOptions(baseUrl);
            //var MHTclient = new RestClient(MHToptions);
            //var mHTrequest = new RestRequest(apiPath, RestSharp.Method.Post);
            //mHTrequest.AddHeader("Content-Type", "application/json");
            //mHTrequest.AddHeader("tenant-id", "1");
            JObject mhtjob = new JObject();

            mhtjob["equipmentId"] = Settings.Default.ID;//设备 ID
            mhtjob["materialSerinalNo"] = bar;// SN 码
            mhtjob["qaResult"] = data.Result.Trim().ToUpper() == "PASS" ? "0" : "1"; //结果

            // 创建新的qaDetailList数组
            JArray qaDetailListArray = new JArray();

            // 解析测试详情数据
            for (int i = 0; i < data.TestDetails.Count; i++)
            {
                // 只解析一次
                JObject detailItem = JObject.FromObject(data.TestDetails[i]);

                JObject qaDetail = new JObject();
                string fullName = detailItem["ItemName"].ToString();

                qaDetail["itemName"] = fullName; // 测试项目全名
                qaDetail["itemType"] = fullName.Contains("_") ? fullName.Split('_')[1] : fullName; // 测试项目
                qaDetail["points"] = detailItem["Points"].ToString(); // 测试点位
                qaDetail["value"] = detailItem["Value"].ToString(); // 测试值
                qaDetail["uppLimit"] = detailItem["UppLimit"].ToString(); // 上限
                qaDetail["lowLimit"] = detailItem["LowLimit"].ToString(); // 下限
                qaDetail["dataUnit"] = detailItem["DataUnit"].ToString(); // 单位

                // 处理可能为null的字段
                qaDetail["withstandVoltageTime"] = detailItem["withstandVoltageTime"]?.ToString() ?? string.Empty;
                qaDetail["withstandVoltageValue"] = detailItem["withstandVoltageValue"]?.ToString() ?? string.Empty;

                qaDetail["result"] = detailItem["Result"].ToString(); // 结果

                // 将详情对象添加到新数组中
                qaDetailListArray.Add(qaDetail);
            }

            // 将新数组添加到主对象中
            mhtjob["qaDetailList"] = qaDetailListArray;


            //mHTrequest.AddBody(JsonConvert.SerializeObject(mhtjob));

            postData = JsonConvert.SerializeObject(mhtjob);
            //RestResponse responseMHT1 = await MHTclient.ExecuteAsync(mHTrequest);

            //JObject mht1 = JObject.Parse(responseMHT1.Content);

            //resoult["isSuccess"] = mht1["code"].ToString().Trim().ToUpper() == "0" ? "TRUE" : "FALSE";
            //resoult["result"] = mht1["code"].ToString().Trim().ToUpper();
            //resoult["message"] = mht1.ToString();
            return resoult.ToString();
        }
        private TestData AnalyzeCSV(string file)
        {
            var testData = new TestData
            {
                Result = "",
                TestDetails = new List<TestDetail>()
            };
            string postData = "";

            using (var parser = new TextFieldParser(file))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                parser.HasFieldsEnclosedInQuotes = true;

                int currentLine = 0;
                while (!parser.EndOfData)
                {
                    string[] fields = parser.ReadFields();
                    currentLine++;
                    // 第二行：获取总体测试结果
                    if (currentLine == 2 && fields.Length >= 3)
                    {
                        testData.Result = fields[2].Trim('"').Trim();
                        continue;
                    }

                    // 从第四行开始解析详细测试数据（跳过前三行）
                    if (currentLine >= 4 && fields.Length >= 9)
                    {
                        var testDetail = new TestDetail
                        {
                            ItemName = SafeGetField(fields, 3), // 测试名称
                            ItemType = SafeGetField(fields, 2), // 项目类型
                            Points = SafeGetField(fields, 4), // 测试点位
                            Value = SafeGetField(fields, 8), // 测试数据
                            UppLimit = SafeGetField(fields, 6), // 上限值
                            LowLimit = SafeGetField(fields, 5), // 下限值
                            DataUnit = SafeGetField(fields, 7), // 单位
                            Result = SafeGetField(fields, 9) // 测试结果
                        };
                        testData.TestDetails.Add(testDetail);
                    }
                }
            }
            return testData;
        }

        /// <summary>
        /// 安全的字段获取方法
        /// </summary>
        /// <param name="fields"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private string SafeGetField(string[] fields, int index)
        {
            if (fields == null || index >= fields.Length || string.IsNullOrWhiteSpace(fields[index]))
                return string.Empty;

            return fields[index].Trim('"').Trim();
        }

        #endregion
    }
    /// <summary>
    /// DataGrid数据显示
    /// </summary>
    public class DataListDisp : DispBase
    {
        public int Index { get; set; }
        public string Barcode { get; set; }
        public DateTime? Time { get; set; }
        public string State { get; set; }

    }
    /// <summary>
    /// 电测数据类
    /// </summary>
    public class TestDetail
    {
        public string ItemName { get; set; }        // 测试名称
        public string ItemType { get; set; }        // 项目类型
        public string Points { get; set; }          // 测试点位
        public string Value { get; set; }           // 测试数据
        public string UppLimit { get; set; }        // 上限值
        public string LowLimit { get; set; }        // 下限值
        public string DataUnit { get; set; }        // 单位
        public string Result { get; set; }          // 测试结果
    }

    /// <summary>
    /// 解析电测文件类
    /// </summary>
    public class TestData
    {
        public List<TestDetail> TestDetails { get; set; }
        public string Result { get; set; }
    }
}


