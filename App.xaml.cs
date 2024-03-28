using MonitorApp.Services;
using MonitorApp.ViewModels;
using MonitorApp.Views;
using Prism.Ioc;
using System.Windows;

namespace MonitorApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
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
