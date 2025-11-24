using System;
using System.Windows;
using System.Windows.Controls;
using UCode.Desktop.ViewModels;

namespace UCode.Desktop.Pages
{
    public partial class TeacherProfilePage : UserControl
    {
        public TeacherProfilePage(TeacherProfileViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            // Load teacher data when page loads
            Loaded += async (s, e) =>
            {
                try
                {
                    await viewModel.LoadTeacherDataAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading teacher profile: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };
        }
    }
}
