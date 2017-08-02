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
        private InstallState state;
        private static InstallViewModel viewmodel;
        /// <summary>
        /// 构造函数，使用单例模式
        /// </summary>
        private InstallViewModel()
        {
            this.State = InstallState.Initializing;
            this.WireUpEventHandlers();
            ///检查安装文件是否为空
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
        public static InstallViewModel GetViewModel()
        {
            if (viewmodel == null)
                viewmodel = new InstallViewModel();
            return viewmodel;
        }
        private BootstrapperApplicationModel model
        {
            get
            {
                return BootstrapperApplicationModel.GetBootstrapperAppModel();
            }
        }
        /// <summary>
        /// 当前产品状态
        /// </summary>
        
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
        /// <summary>
        /// 与 InstallEnabled，UninstallEnabled,ProgressEnabled,FinishEnabled
        /// 根据State的值，判断该显示哪个界面
        /// </summary>
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
            //重新定义`BootstrapperApplication`的安装包验证方式，判断是当前msi安装包是否已经安装过
            this.model.BootstrapperApplication.DetectPackageComplete += this.DetectPackageComplete;
        }
        /// <summary>
        /// 查找注册表看是否已经安装本产品的其他版本
        /// </summary>
        /// <returns></returns>
        protected bool ChekExistFromRegistry()
        {
            try
            {
                //64位
                using (RegistryKey pathKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(bit3264))
                {
                    var strs = pathKey.GetSubKeyNames();
                    foreach (string str in strs)
                        if (str.Equals(productName))
                        {
                            return true;
                        }
                }
                //32位
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