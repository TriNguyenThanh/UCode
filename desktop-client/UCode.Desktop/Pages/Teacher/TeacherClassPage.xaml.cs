using System.Windows.Controls;
using UCode.Desktop.Services;
using UCode.Desktop.ViewModels;

namespace UCode.Desktop.Pages
{
    public partial class TeacherClassPage : UserControl
    {
        public TeacherClassPage(TeacherClassViewModel viewModel, NavigationService navigationService)
        {
            InitializeComponent();
            
            // Inject navigation service into ViewModel
            viewModel.SetNavigationService(navigationService);
            
            DataContext = viewModel;
        }
    }
}
