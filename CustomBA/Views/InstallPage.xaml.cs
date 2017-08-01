using CustomBA.Models;
using CustomBA.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CustomBA.Views
{
    /// <summary>
    /// Interaction logic for SetUpPage.xaml
    /// </summary>
    public partial class InstallPage : UserControl
    {
        public InstallPage()
        {
            InitializeComponent();
            this.DataContext = new InstallPageViewModel();
        }
        
    }
}
