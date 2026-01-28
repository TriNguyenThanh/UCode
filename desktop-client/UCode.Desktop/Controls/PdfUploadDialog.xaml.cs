using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using UCode.Desktop.Helpers;
using UCode.Desktop.Services;

namespace UCode.Desktop.Controls
{
    public partial class PdfUploadDialog : INotifyPropertyChanged
    {
        private int _selectedTab;
        private string _pdfUrl = string.Empty;
        private string _linkText = "Download PDF";
        private bool _isUploading;
        private bool _isUploadSuccess;

        public int SelectedTab
        {
            get => _selectedTab;
            set
            {
                _selectedTab = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsUrlTab));
                OnPropertyChanged(nameof(IsUploadTab));
            }
        }

        public string PdfUrl
        {
            get => _pdfUrl;
            set
            {
                _pdfUrl = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasValidUrl));
            }
        }

        public string LinkText
        {
            get => _linkText;
            set
            {
                _linkText = value;
                OnPropertyChanged();
            }
        }

        public bool IsUploading
        {
            get => _isUploading;
            set
            {
                _isUploading = value;
                OnPropertyChanged();
            }
        }

        public bool IsUploadSuccess
        {
            get => _isUploadSuccess;
            set
            {
                _isUploadSuccess = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasValidUrl));
            }
        }

        public bool IsUrlTab => SelectedTab == 0;
        public bool IsUploadTab => SelectedTab == 1;
        public bool HasValidUrl => !string.IsNullOrWhiteSpace(PdfUrl);

        public ICommand BrowseFileCommand { get; }

        public PdfUploadDialog()
        {
            InitializeComponent();
            DataContext = this;
            BrowseFileCommand = new RelayCommand(_ => BrowseFile());
        }

        private async void BrowseFile()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "PDF Files|*.pdf|All Files|*.*",
                Title = "Select PDF"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    IsUploading = true;
                    IsUploadSuccess = false;

                    var apiService = App.ServiceProvider.GetService(typeof(ApiService)) as ApiService;
                    var fileService = new FileUploadService(apiService);
                    var result = await fileService.UploadFileAsync(dialog.FileName, FileCategory.Document);

                    PdfUrl = result.FileUrl;
                    LinkText = System.IO.Path.GetFileNameWithoutExtension(dialog.FileName);
                    IsUploadSuccess = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to upload PDF: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsUploading = false;
                }
            }
        }

        private void OnInsertClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
