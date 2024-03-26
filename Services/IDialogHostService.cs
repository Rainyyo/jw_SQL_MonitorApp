

using Prism.Services.Dialogs;

namespace MonitorApp.Services
{
    public interface IDialogHostService : IDialogService
    {
        ButtonResult ShowDialog(string name, IDialogParameters parameters);
    }
}
