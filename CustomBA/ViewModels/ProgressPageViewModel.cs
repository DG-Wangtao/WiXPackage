using CustomBA.Models;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace CustomBA.ViewModels
{
    public class ProgressPageViewModel:BaseViewModel
    {
        private InstallViewModel installViewModel
        {
            get { return InstallViewModel.GetViewModel(); }
        }
        private BootstrapperApplicationModel BootstrapperModel
        {
            get
            {
                return BootstrapperApplicationModel.GetBootstrapperAppModel();
            }
        }
        private int cacheProgress;
        private int executeProgress;
        private int progress;
        public int Progress
        {
            get { return progress; }
            set {
                progress = value;
                OnPropertyChanged("Progress");
            }
        }
        private string message;
        public string Message
        {
            get { return message; }
            set
            {
                message = value;
                OnPropertyChanged("Message");
            }
        }

        public BitmapSource BackImage
        {
            get
            {
                Bitmap bmp = CustomBA.Properties.Resources.page2;
                return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bmp.GetHbitmap(),
                    IntPtr.Zero, System.Windows.Int32Rect.Empty,
                    BitmapSizeOptions.FromWidthAndHeight(bmp.Width, bmp.Height));
            }
        }
        private DelegateCommand RollBackCommand;

        public ICommand btn_cancel
        {
            get { return RollBackCommand; }
        }
        public ProgressPageViewModel()
        {
            RollBackCommand = new DelegateCommand(RollBack,IsValid);
            this.BootstrapperModel.BootstrapperApplication.ApplyComplete += this.ApplyComplete;
            this.BootstrapperModel.BootstrapperApplication.ExecutePackageBegin += this.ExecutePackageBegin;
            this.BootstrapperModel.BootstrapperApplication.ExecutePackageComplete += this.ExecutePackageComplete;
            this.BootstrapperModel.BootstrapperApplication.CacheAcquireProgress +=
               (sender, args) =>
               {
                   this.cacheProgress = args.OverallPercentage;
                   this.Progress =
                      (this.cacheProgress + this.executeProgress) / 2;
               };
            this.BootstrapperModel.BootstrapperApplication.ExecuteProgress +=
               (sender, args) =>
               {
                   this.executeProgress = args.OverallPercentage;
                   this.Progress =
                      (this.cacheProgress + this.executeProgress) / 2;
               };
            this.BootstrapperModel.BootstrapperApplication.ExecuteMsiMessage += BootstrapperApplication_ExecuteMsiMessage;
        }

        private void BootstrapperApplication_ExecuteMsiMessage(object sender, ExecuteMsiMessageEventArgs e)
        {
            lock (this)
            {
                if (e.MessageType == InstallMessage.ActionStart)
                {
                    this.Message = e.Message;
                }
            }
        }
        protected void ExecutePackageBegin(object sender, ExecutePackageBeginEventArgs e)
        {
            if (installViewModel.State == InstallState.Cancelled)
            {
                e.Result = Result.Cancel;
            }
        }

        protected void ExecutePackageComplete(object sender, ExecutePackageCompleteEventArgs e)
        {
            if (installViewModel.State == InstallState.Cancelled)
            {
                e.Result = Result.Cancel;
            }
        }
        public void RollBack()
        {
            installViewModel.State = InstallState.Cancelled;
            CustomBootstrapperApplication.Dispatcher.InvokeShutdown();
        }
        public bool IsValid()
        {
            return true;
        }
        protected void ApplyComplete(object sender, ApplyCompleteEventArgs e)
        {
            this.BootstrapperModel.FinalResult = e.Status;
            installViewModel.State = InstallState.Applied;
        }
    }
}
