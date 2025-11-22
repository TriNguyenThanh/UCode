using System.Windows.Controls;
using UCode.Desktop.ViewModels;

namespace UCode.Desktop.Views.Students
{
    public partial class AssignmentDetailPage : UserControl
    {
        public AssignmentDetailPage()
        {
            InitializeComponent();
            DataContext = App.ServiceProvider.GetService(typeof(AssignmentDetailViewModel));
        }
    }
}
