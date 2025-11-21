using System.Windows.Controls;
using UCode.Desktop.ViewModels;

namespace UCode.Desktop.Views.Students
{
    public partial class ClassDetailPage : UserControl
    {
        public ClassDetailPage()
        {
            InitializeComponent();
            DataContext = App.ServiceProvider.GetService(typeof(ClassDetailViewModel));
        }
    }
}
