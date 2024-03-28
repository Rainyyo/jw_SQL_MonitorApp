
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using MonitorApp.Services;

namespace MonitorApp.ViewModels
{
    public class DialogViewModel : BindableBase, IDialogAware
    {
        private readonly IContainerProvider containerProvider;
        public readonly IEventAggregator aggregator;
        public readonly IDialogService dialogService;
        private readonly IDialogHostService _dialogHostService;


        public event Action<IDialogResult> RequestClose;

        public string Title { set; get; }

        public virtual bool CanCloseDialog()
        {
            return true;
        }

        public virtual void OnDialogClosed()
        {

        }

        public virtual void OnDialogOpened(IDialogParameters parameters)
        {

        }
        protected void OnRequestClose(DialogResult dr)
        {
            RequestClose?.Invoke(dr);
        }
        public DialogViewModel(IContainerProvider containerProvider)
        {
            this.containerProvider = containerProvider;
            aggregator = containerProvider.Resolve<IEventAggregator>();
            dialogService = containerProvider.Resolve<IDialogService>();
            _dialogHostService = containerProvider.Resolve<IDialogHostService>();
        }
    }
}
