using MonitorApp.Services;
using MonitorApp.ViewModels;
using MonitorApp.Views;
using Prism.Ioc;
using System.Threading;
using System;
using System.Windows;

namespace MonitorApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private static Mutex _mutex = null;
        protected override void OnStartup(StartupEventArgs e)
        {
            const string appName = "LeaderCCSLaserUI";
            bool createdNew;

            _mutex = new Mutex(true, appName, out createdNew);

            if (!createdNew)
            {
                MessageBox.Show("软件已开启", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                //app is already running! Exiting the application  
                Environment.Exit(-1);
            }

            base.OnStartup(e);
        }
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)//注册页面服务
        {
            containerRegistry.RegisterSingleton<IPLCService, PLCService>("PLC");
            containerRegistry.Register<IDialogHostService, DialogHostService>();
            containerRegistry.RegisterDialog<ParameterDialog, ParameterDialogViewModel>();
        }
    }
}
