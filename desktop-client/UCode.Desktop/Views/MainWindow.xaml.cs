using System;
using System.Windows;
using MahApps.Metro.Controls;
using UCode.Desktop.ViewModels;

namespace UCode.Desktop.Views
{
    public partial class MainWindow : MetroWindow
    {
        public MainWindow(MainViewModel viewModel)
        {
            try
            {
                var logPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mainwindow.log");
                System.IO.File.AppendAllText(logPath, $"MainWindow constructor started at {DateTime.Now}\n");

                InitializeComponent();
                System.IO.File.AppendAllText(logPath, "InitializeComponent completed\n");

                DataContext = viewModel;
                System.IO.File.AppendAllText(logPath, "DataContext set\n");

                // Load data when window loads
                Loaded += async (s, e) =>
                {
                    try
                    {
                        System.IO.File.AppendAllText(logPath, "Window Loaded event fired. Loading data...\n");
                        await viewModel.LoadDataAsync();
                        System.IO.File.AppendAllText(logPath, "LoadDataAsync completed\n");
                    }
                    catch (Exception ex)
                    {
                        System.IO.File.AppendAllText(logPath, $"ERROR in Loaded event: {ex.Message}\n{ex.StackTrace}\n");
                        MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                };

                System.IO.File.AppendAllText(logPath, "MainWindow constructor completed\n");
            }
            catch (Exception ex)
            {
                var logPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mainwindow_error.log");
                System.IO.File.WriteAllText(logPath, $"FATAL ERROR in MainWindow constructor: {ex.Message}\n{ex.StackTrace}\n\nInner: {ex.InnerException?.Message}\n{ex.InnerException?.StackTrace}");
                MessageBox.Show($"Fatal error creating main window: {ex.Message}", "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }
    }
}
