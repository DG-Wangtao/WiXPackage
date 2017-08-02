using CustomBA.Models;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace CustomBA.ViewModels
{
    public class UnistallRepairViewModel
    {
        private InstallViewModel installViewModel
        {
            get { return InstallViewModel.GetViewModel(); }
        }
        private BootstrapperApplicationModel BootstrapperModel
        {
            get
            { return BootstrapperApplicationModel.GetBootstrapperAppModel(); }
        }
        private DelegateCommand CloseCommand;
        private DelegateCommand RepairCommand;
        private DelegateCommand UninstallCommand;
        public ICommand btn_close
        {
            get { return CloseCommand; }
        }
        public ICommand btn_repair
        {
            get { return RepairCommand; }
        }
        public ICommand btn_uninstall
        {
            get { return UninstallCommand; }
        }
        public BitmapSource BackImage
        {
            get
            {
                Bitmap bmp = CustomBA.Properties.Resources.page4;
                return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bmp.GetHbitmap(),
                    IntPtr.Zero, System.Windows.Int32Rect.Empty,
                    BitmapSizeOptions.FromWidthAndHeight(bmp.Width, bmp.Height));
            }
        }
        public UnistallRepairViewModel()
        {
            UninstallCommand = new DelegateCommand(Uninstall, IsValid);
            RepairCommand = new DelegateCommand(Repair, IsValid);
            CloseCommand = new DelegateCommand(Close, IsValid);
        }
        public void Repair()
        {
            BootstrapperModel.PlanAction(LaunchAction.Repair);
        }
        public void Uninstall()
        {
            BootstrapperModel.PlanAction(LaunchAction.Uninstall);
        }
        public void Close()
        {
             CustomBootstrapperApplication.Dispatcher.InvokeShutdown();
        }
        public bool IsValid()
        {
            return true;
        }

    }
}
