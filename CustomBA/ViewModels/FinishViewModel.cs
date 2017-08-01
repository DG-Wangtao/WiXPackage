using CustomBA.Models;
using Microsoft.Practices.Prism.Commands;
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
    public class FinishViewModel
    {
        public FinishViewModel()
        {
            CloseCommand = new DelegateCommand(Close, IsValid);

        }
        private DelegateCommand CloseCommand;
        public ICommand btn_close
        {
            get { return CloseCommand; }
        }
        public BitmapSource BackImage
        {
            get
            {
                Bitmap bmp = CustomBA.Properties.Resources.page3;
                return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bmp.GetHbitmap(),
                    IntPtr.Zero, System.Windows.Int32Rect.Empty,
                    BitmapSizeOptions.FromWidthAndHeight(bmp.Width, bmp.Height));
            }
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
