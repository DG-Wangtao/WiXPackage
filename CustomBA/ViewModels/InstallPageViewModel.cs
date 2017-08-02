using CustomBA.HelpClass;
using CustomBA.Models;
using CustomBA.Views;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace CustomBA.ViewModels
{
    public class InstallPageViewModel : BaseViewModel
    {
        private static string SoftWareName = "wpfapptopackage";
        private InstallViewModel installViewModel
        {
            get { return InstallViewModel.GetViewModel(); }
        }
        private BootstrapperApplicationModel BootstrapperModel
        {
            get
            {
                return BootstrapperApplicationModel.GetBootstrapperAppModel(CustomBootstrapperApplication.GetApplication());
            }
        }
        public InstallPageViewModel()
        {
            InitialCommand();
            SeleFileVisibility = Visibility.Collapsed;
            CreateShortCut = true;
            InstallFolder = @"C:\Program Files (x86)\DeepGlin\" + SoftWareName;
            WireUpEventHandlers();
        }
        public BitmapSource BackImage
        {
            get
            {
                Bitmap bmp = CustomBA.Properties.Resources.page1;
                return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bmp.GetHbitmap(),
                    IntPtr.Zero, System.Windows.Int32Rect.Empty,
                    BitmapSizeOptions.FromWidthAndHeight(bmp.Width, bmp.Height));
            }
        }
        public BitmapSource LogoImage
        {
            get
            {
                Bitmap bmp = CustomBA.Properties.Resources.logo;
                return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bmp.GetHbitmap(),
                    IntPtr.Zero, System.Windows.Int32Rect.Empty,
                    BitmapSizeOptions.FromWidthAndHeight(bmp.Width, bmp.Height));
            }
        }
        private bool createShortCut;
        private string installFolder;
        private Visibility selectFileVisibility;
        public Visibility SeleFileVisibility
        {
            get { return selectFileVisibility; }
            set
            {
                selectFileVisibility = value;
                OnPropertyChanged("SeleFileVisibility");
            }
        }
       
        public bool CreateShortCut
        {
            get { return createShortCut; }
            set
            {
                createShortCut = value;
                OnPropertyChanged("CreateShortCut");
                string bol = "0";
                if (createShortCut)
                    bol = "1";
                this.SetBurnVariable("CreateShortCut", bol);
            }
        }
       
        public string InstallFolder
        {
            get { return installFolder; }
            set
            {
                try {
                    if (value != installFolder && ValidDir(value))
                    {
                        string[] para = value.Split('\\');
                        bool hassoftwarename = false;
                        foreach (string pa in para)
                        {
                            if (pa == SoftWareName)
                                hassoftwarename = true;
                        }
                        if (hassoftwarename)
                            installFolder = value;
                        else
                            installFolder = value + "\\" + SoftWareName;
                        OnPropertyChanged("InstallFolder");
                        this.SetBurnVariable("InstallFolder", installFolder);
                    }
                }
                catch {
                    installFolder = value;
                }
            }
        }

        private DelegateCommand BrowseCommand;
        private DelegateCommand InstallCommand;
        private DelegateCommand CloseCommand;
        private DelegateCommand ShowSelecFileCommand;

        public ICommand btn_browse
        {
            get { return BrowseCommand; }
        }
        public ICommand btn_install
        {
            get { return InstallCommand; }
        }
        public ICommand btn_cancel
        {
            get { return CloseCommand; }
        }
        public ICommand btn_show
        {
            get { return ShowSelecFileCommand; }
        }
        private void InitialCommand()
        {
            BrowseCommand = new DelegateCommand(Browse, IsValid);
            InstallCommand = new DelegateCommand(Install, IsValid);
            CloseCommand = new DelegateCommand(Close, IsValid);
            ShowSelecFileCommand = new DelegateCommand(Show, IsValid);
        }
        public void Browse()
        {
            var folderBrowserDialog = new FolderBrowserDialog { SelectedPath = InstallFolder };

            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                InstallFolder = folderBrowserDialog.SelectedPath;
            }
        }
        public void Install()
        {
            this.BootstrapperModel.PlanAction(LaunchAction.Install);
        }
        public void Close()
        {
              CustomBootstrapperApplication.Dispatcher.InvokeShutdown();
        }

        public void Show()
        {
            if (SeleFileVisibility == Visibility.Collapsed)
                SeleFileVisibility = Visibility.Visible;
            else
                SeleFileVisibility = Visibility.Collapsed;
        }

        protected void ApplyBegin(object sender, ApplyBeginEventArgs e)
        {
            this.installViewModel.State = InstallState.Applying;
        }

        protected void PlanComplete(object sender, PlanCompleteEventArgs e)
        {
            if (installViewModel.State == InstallState.Cancelled)
            {
                CustomBootstrapperApplication.Dispatcher
                  .InvokeShutdown();
                return;
            }
            installViewModel.State = InstallState.Applying;
            this.BootstrapperModel.ApplyAction();
        }


        private void WireUpEventHandlers()
        {
            this.BootstrapperModel.BootstrapperApplication.PlanComplete += this.PlanComplete;
            this.BootstrapperModel.BootstrapperApplication.ApplyBegin += this.ApplyBegin;
        }
        public bool ValidDir(string path)
        {
            try
            {
                string p = new DirectoryInfo(path).FullName;
                return true;
            }
            catch
            {
                return false;
            }
        }
        public void SetBurnVariable(string variableName, string value)
        {
            this.BootstrapperModel.SetBurnVariable(variableName, value);
        }
        public bool IsValid()
        {
            return true;
        }
    }
}
