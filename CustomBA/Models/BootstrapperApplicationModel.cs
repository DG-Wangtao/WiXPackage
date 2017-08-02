using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
using System;
using System.Windows;
using System.Windows.Interop;

namespace CustomBA.Models
{
    public class BootstrapperApplicationModel
    {
        private IntPtr hwnd;
        private static BootstrapperApplicationModel bootstrapperAppModel;
        public static BootstrapperApplicationModel GetBootstrapperAppModel(BootstrapperApplication bootstrapperApplication)
        {
            if (bootstrapperAppModel == null)
                bootstrapperAppModel = new BootstrapperApplicationModel(bootstrapperApplication);
            return bootstrapperAppModel;
        }
        public static BootstrapperApplicationModel GetBootstrapperAppModel()
        {
             return bootstrapperAppModel;
        }
        private BootstrapperApplicationModel(BootstrapperApplication bootstrapperApplication)
        {
            this.BootstrapperApplication =
              bootstrapperApplication;
            this.hwnd = IntPtr.Zero;
            string[] strs = GetCommandLine();
        }

        public BootstrapperApplication BootstrapperApplication { get; private set; }

        public int FinalResult { get; set; }

        public void SetWindowHandle(Window view)
        {
            this.hwnd = new WindowInteropHelper(view).Handle;
        }

        public void PlanAction(LaunchAction action)
        {
            this.BootstrapperApplication.Engine.Plan(action);
        }

        public void ApplyAction()
        {
            this.BootstrapperApplication.Engine.Apply(this.hwnd);
        }

        public void LogMessage(string message)
        {
            this.BootstrapperApplication.Engine.Log(
              LogLevel.Standard,
              message);
        }
        public void SetBurnVariable(string variableName, string value)
        {
            this.BootstrapperApplication.Engine
               .StringVariables[variableName] = value;
        }
        public string[] GetCommandLine()
        {
            return this.BootstrapperApplication.Command
               .GetCommandLineArgs();
        }
        public bool HelpRequested()
        {
            return this.BootstrapperApplication.Command.Action ==
               LaunchAction.Help;
        }
    }
    /// <summary>
    /// Present(已安装)
    /// NotPresent(未安装)
    /// Applying(正在应用)
    /// Applied(已配置完成)
    /// Cancelled(取消操作)
    /// Failed(配置失败)
    /// </summary>
    public enum InstallState
    {
        Initializing,
        Present,
        NotPresent,
        Applying,
        Cancelled,
        Applied,
        Failed,
    }
}