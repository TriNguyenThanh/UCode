using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using UCode.Desktop.ViewModels;

namespace UCode.Desktop.Pages
{
    public partial class TeacherAssignmentPage : UserControl
    {
        public TeacherAssignmentPage(TeacherAssignmentViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void Border_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is System.Windows.Controls.Border border)
            {
                border.Background = new SolidColorBrush(Color.FromRgb(233, 236, 239));
            }
        }

        private void Border_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is System.Windows.Controls.Border border)
            {
                border.Background = new SolidColorBrush(Color.FromRgb(248, 249, 250));
            }
        }

        private void ProblemsTab_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var problemsTab = FindName("ProblemsTab") as Button;
            var submissionsTab = FindName("SubmissionsTab") as Button;
            var problemsContent = FindName("ProblemsTabContent") as System.Windows.UIElement;
            var submissionsContent = FindName("SubmissionsTabContent") as System.Windows.UIElement;

            if (problemsTab != null && submissionsTab != null &&
                problemsContent != null && submissionsContent != null)
            {
                // Update tab styles
                problemsTab.Foreground = new SolidColorBrush(Color.FromRgb(250, 203, 1));
                problemsTab.BorderBrush = new SolidColorBrush(Color.FromRgb(250, 203, 1));
                problemsTab.BorderThickness = new System.Windows.Thickness(0, 0, 0, 3);
                
                submissionsTab.Foreground = new SolidColorBrush(Color.FromRgb(102, 102, 102));
                submissionsTab.BorderThickness = new System.Windows.Thickness(0);

                // Show/hide content
                problemsContent.Visibility = System.Windows.Visibility.Visible;
                submissionsContent.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void SubmissionsTab_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var problemsTab = FindName("ProblemsTab") as Button;
            var submissionsTab = FindName("SubmissionsTab") as Button;
            var problemsContent = FindName("ProblemsTabContent") as System.Windows.UIElement;
            var submissionsContent = FindName("SubmissionsTabContent") as System.Windows.UIElement;

            if (problemsTab != null && submissionsTab != null &&
                problemsContent != null && submissionsContent != null)
            {
                // Update tab styles
                problemsTab.Foreground = new SolidColorBrush(Color.FromRgb(102, 102, 102));
                problemsTab.BorderThickness = new System.Windows.Thickness(0);
                
                submissionsTab.Foreground = new SolidColorBrush(Color.FromRgb(250, 203, 1));
                submissionsTab.BorderBrush = new SolidColorBrush(Color.FromRgb(250, 203, 1));
                submissionsTab.BorderThickness = new System.Windows.Thickness(0, 0, 0, 3);

                // Show/hide content
                problemsContent.Visibility = System.Windows.Visibility.Collapsed;
                submissionsContent.Visibility = System.Windows.Visibility.Visible;
            }
        }


    }
}
