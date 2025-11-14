using System.Windows;
using UCode.Desktop.Services;

namespace UCode.Desktop.Views
{
    public partial class TeacherHomeWindow : Window
    {
        private readonly NavigationService _navigationService;

        public TeacherHomeWindow(NavigationService navigationService)
        {
            InitializeComponent();
            
            _navigationService = navigationService;
            
            // Set up navigation frame
            _navigationService.SetFrame(NavigationFrame);

            // Handle back button visibility
            _navigationService.CanGoBackChanged += (s, canGoBack) =>
            {
                BackButton.Visibility = canGoBack ? Visibility.Visible : Visibility.Collapsed;
            };

            // Navigate to home page on load
            Loaded += (s, e) =>
            {
                var homePage = App.ServiceProvider?.GetService(typeof(Pages.TeacherHomePage)) as Pages.TeacherHomePage;
                if (homePage != null)
                {
                    // Set user name from ViewModel
                    if (homePage.DataContext is ViewModels.TeacherHomeViewModel viewModel)
                    {
                        UserNameText.Text = viewModel.TeacherName;
                    }
                    
                    _navigationService.NavigateTo(homePage);
                }
            };
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            _navigationService.GoBack();
        }

        private void DashboardTab_Click(object sender, RoutedEventArgs e)
        {
            // Clear navigation stack and go to home
            _navigationService.ClearNavigationStack();
            
            var homePage = App.ServiceProvider?.GetService(typeof(Pages.TeacherHomePage)) as Pages.TeacherHomePage;
            if (homePage != null)
            {
                _navigationService.NavigateTo(homePage);
            }

            // Update tab styles
            DashboardTab.Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FACB01"));
            DashboardTab.BorderBrush = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FACB01"));
            DashboardTab.BorderThickness = new Thickness(0, 0, 0, 3);
            
            ProblemsTab.Foreground = System.Windows.Media.Brushes.White;
            ProblemsTab.BorderThickness = new Thickness(0);
        }

        private void ProblemsTab_Click(object sender, RoutedEventArgs e)
        {
            // Clear navigation stack and go to problems page
            _navigationService.ClearNavigationStack();
            
            var problemsPage = App.ServiceProvider?.GetService(typeof(Pages.TeacherProblemsPage)) as Pages.TeacherProblemsPage;
            if (problemsPage != null)
            {
                _navigationService.NavigateTo(problemsPage);
            }

            // Update tab styles
            ProblemsTab.Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FACB01"));
            ProblemsTab.BorderBrush = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FACB01"));
            ProblemsTab.BorderThickness = new Thickness(0, 0, 0, 3);
            
            DashboardTab.Foreground = System.Windows.Media.Brushes.White;
            DashboardTab.BorderThickness = new Thickness(0);
        }
    }
}

