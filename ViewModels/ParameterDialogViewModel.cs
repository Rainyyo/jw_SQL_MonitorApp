using Microsoft.Win32;
using MonitorApp.Properties;
using Prism.Commands;
using Prism.Ioc;
using Prism.Services.Dialogs;
using System;
using System.Windows;

namespace MonitorApp.ViewModels
{
    public class ParameterDialogViewModel : DialogViewModel
    {
        #region 属性
        private string _ID;
        public string ID
        {
            get { return _ID; }
            set { SetProperty(ref _ID, value); }
        }
        private string _Tary;
        public string Tary
        {
            get { return _Tary; }
            set { SetProperty(ref _Tary, value); }
        }
        private string _URL_PostIn;
        public string URL_PostIn
        {
            get { return _URL_PostIn; }
            set { SetProperty(ref _URL_PostIn, value); }
        }
        private string _URL_PostOut;
        public string URL_PostOut
        {
            get { return _URL_PostOut; }
            set { SetProperty(ref _URL_PostOut, value); }
        }
        private string filePath;
        public string FilePath
        {
            get { return filePath; }
            set { SetProperty(ref filePath, value); }
        }
        #endregion
        #region 方法
        private DelegateCommand<object> loadFile;
        public DelegateCommand<object> LoadFile =>
            loadFile ?? (loadFile = new DelegateCommand<object>(ExecuteLoadFile));

        void ExecuteLoadFile(object obj)
        {
            if (obj is string str)
            {
                switch (str)
                {
                    case "测试文件":
                        var dialog = new System.Windows.Forms.FolderBrowserDialog();
                        dialog.Description = "请选择CSV文件夹";
                        dialog.ShowDialog();
                        if (!string.IsNullOrEmpty(dialog.SelectedPath))
                        {
                            FilePath = dialog.SelectedPath;
                            Settings.Default.FolderPath = FilePath;
                            Settings.Default.Save();
                        }
                        break;
                }
            }
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
                    case "ID":
                        Settings.Default.ID= ID;
                        break;
                    case "Tary":
                        Settings.Default.Tary = Tary;
                        break;
                    case "URL_PostIn":
                        Settings.Default.URL_PostIn = URL_PostIn;
                        break;
                    case "URL_PostOut":
                        Settings.Default.URL_PostOut = URL_PostOut;
                        break;
                }
                Settings.Default.Save();
            }
        }
        #endregion
        #region 构造函数
        public ParameterDialogViewModel(IContainerProvider containerProvider) : base(containerProvider)
        {
            Title = "参数设置";
            ID = Settings.Default.ID;
            URL_PostIn = Settings.Default.URL_PostIn;
            URL_PostOut = Settings.Default.URL_PostOut;
            FilePath=Settings.Default.Filer;
            Tary = Settings.Default.Tary;
        }
        #endregion

    }
}
