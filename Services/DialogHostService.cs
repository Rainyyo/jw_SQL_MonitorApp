

using Prism.Ioc;
using Prism.Services.Dialogs;

namespace MonitorApp.Services
{
    public class DialogHostService : DialogService, IDialogHostService
    {
        private readonly IContainerExtension containerExtension;
        public DialogHostService(IContainerExtension containerExtension) : base(containerExtension)
        {
            this.containerExtension = containerExtension;
        }

        public ButtonResult ShowDialog(string name, IDialogParameters parameters)
        {
            if (parameters == null)
                parameters = new DialogParameters();

            //从容器当中取出弹出窗口的实例
            var dialogService = containerExtension.Resolve<IDialogService>();
            ButtonResult dialogResult = ButtonResult.None;
            dialogService.ShowDialog(name, parameters, arg => {
                dialogResult = arg.Result;
            });
            return dialogResult;
        }
    }
}
