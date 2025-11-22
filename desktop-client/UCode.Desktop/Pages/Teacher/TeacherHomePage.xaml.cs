using System.Windows.Controls;
using UCode.Desktop.Services;
using UCode.Desktop.ViewModels;

namespace UCode.Desktop.Pages
{
    public partial class TeacherHomePage : UserControl
    {
        public TeacherHomePage(TeacherHomeViewModel viewModel, NavigationService navigationService)
        {
            InitializeComponent();
            
            // Inject navigation service into ViewModel
            viewModel.SetNavigationService(navigationService);
            
            DataContext = viewModel;
            
            // Load data when page is loaded
            Loaded += async (s, e) => await viewModel.LoadDataAsync();
        }
    }
}
