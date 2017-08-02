using CustomBA.Models;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.ViewModel;
using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace CustomBA.ViewModels
{
    public class InstallViewModel : BaseViewModel
    {
        private string bit3264 = @"SOFTWARE\WOW6432Node\Microsoft\DeepGlint";
        private string bit32 = @"SOFTWARE\Microsoft\DeepGlint";
        private string productName = "LibraFClinet";
        private static string MyInstellerName = "DGSetup1.msi";
        private BootstrapperApplicationModel model
        {
            get
            {
                return BootstrapperApplicationModel.GetBootstrapperAppModel(CustomBootstrapperApplication.GetApplication());
            }
        }

        private InstallState state;
        public InstallState State
        {
            get
            {
                return this.state;
            }
            set
            {
                if (this.state != value)
                {
                    this.state = value;
                    if (state == InstallState.NotPresent)
                        if (ChekExistFromRegistry())
                        {
                            state = InstallState.Cancelled;
                        }
                    OnPropertyChanged("State");
                    OnPropertyChanged("CancelEnabled");
                    OnPropertyChanged("InstallEnabled");
                    OnPropertyChanged("UninstallEnabled");
                    OnPropertyChanged("ProgressEnabled");
                    OnPropertyChanged("FinishEnabled");
                }
                
            }
        }
        public bool CancelEnabled
        {
            get
            {
                return State == InstallState.Cancelled;
            }
        }
        public bool InstallEnabled
        {
            get {
                return State == InstallState.NotPresent;
            }
        }

        public bool UninstallEnabled
        {
            get
            {
                return State == InstallState.Present;
            }
        }
        public bool ProgressEnabled
        {
            get
            {
                return State == InstallState.Applying;
            }
        }
        public bool FinishEnabled
        {
            get
            {
                return State == InstallState.Applied;
            }
        }



        private static InstallViewModel viewmodel;
        public static InstallViewModel GetViewModel()
        {
            if (viewmodel == null)
                viewmodel = new InstallViewModel();
            return viewmodel;
        }
        private InstallViewModel()
        {
            this.State = InstallState.Initializing;
            this.WireUpEventHandlers();
            this.model.BootstrapperApplication.ResolveSource +=
               (sender, args) =>
               {
                   if (!string.IsNullOrEmpty(args.DownloadSource))
                   {
                       // Downloadable package found 
                       args.Result = Result.Download;
                   }
                   else
                   {
                       // Not downloadable 
                       args.Result = Result.Ok;
                   }
               };
        }


        protected void DetectPackageComplete(object sender, DetectPackageCompleteEventArgs e)
        {
            if (e.PackageId.Equals(MyInstellerName, StringComparison.Ordinal))
            {
                this.State = e.State == PackageState.Present ?
                  InstallState.Present : InstallState.NotPresent;
            }

        }

        private void WireUpEventHandlers()
        {
            this.model.BootstrapperApplication.DetectPackageComplete += this.DetectPackageComplete;
        }
        public bool ValidDir(string path)
        {
            try
            {
                string p =new DirectoryInfo(path).FullName;
                return true;
            }
            catch
            {
                return false;
            }
        }
        protected bool ChekExistFromRegistry()
        {
            try
            {
                using (RegistryKey pathKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(bit3264))
                {
                    var strs = pathKey.GetSubKeyNames();
                    foreach (string str in strs)
                        if (str.Equals(productName))
                        {
                            return true;
                        }
                }
                using (RegistryKey pathKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(bit32))
                {
                    var strs = pathKey.GetSubKeyNames();
                    foreach (string str in strs)
                        if (str.Equals(productName))
                        {
                            return true;
                        }
                }
            }
            catch { }
            return false;
        }
    }
}