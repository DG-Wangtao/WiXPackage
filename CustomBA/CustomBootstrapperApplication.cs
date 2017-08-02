using CustomBA.Models;
using CustomBA.ViewModels;
using CustomBA.Views;
using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
using System;
using System.Windows.Threading;

namespace CustomBA
{
    public class CustomBootstrapperApplication :BootstrapperApplication
    {
        public static Dispatcher Dispatcher { get; set; }
        private static CustomBootstrapperApplication customBootstrapperApplication;
        public static CustomBootstrapperApplication GetApplication()
        {
            if (customBootstrapperApplication == null)
                customBootstrapperApplication = new CustomBootstrapperApplication();
            return customBootstrapperApplication;
        }
        public CustomBootstrapperApplication()
        {
            customBootstrapperApplication = this;
        }
        protected override void Run()
        {
            Dispatcher = Dispatcher.CurrentDispatcher;

            var model = BootstrapperApplicationModel.GetBootstrapperAppModel(this);
            var view = new InstallView();

            model.SetWindowHandle(view);

            this.Engine.Detect();

            view.Show();
            Dispatcher.Run();
            this.Engine.Quit(model.FinalResult);
        }
    }
}
