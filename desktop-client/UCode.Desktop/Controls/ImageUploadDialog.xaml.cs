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
    public partial class ImageUploadDialog : INotifyPropertyChanged
    {
        private int _selectedTab;
        private string _imageUrl = string.Empty;
        private string _altText = string.Empty;
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

        public string ImageUrl
        {
            get => _imageUrl;
            set
            {
                _imageUrl = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasValidUrl));
            }
        }

        public string AltText
        {
            get => _altText;
            set
            {
                _altText = value;
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
        public bool HasValidUrl => !string.IsNullOrWhiteSpace(ImageUrl);

        public ICommand BrowseFileCommand { get; }

        public ImageUploadDialog()
        {
            InitializeComponent();
            DataContext = this;
            BrowseFileCommand = new RelayCommand(_ => BrowseFile());
        }

        private async void BrowseFile()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp;*.webp|All Files|*.*",
                Title = "Select Image"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    IsUploading = true;
                    IsUploadSuccess = false;

                    var apiService = App.ServiceProvider.GetService(typeof(ApiService)) as ApiService;
                    var fileService = new FileUploadService(apiService);
                    var result = await fileService.UploadFileAsync(dialog.FileName, FileCategory.Image);

                    ImageUrl = result.FileUrl;
                    AltText = System.IO.Path.GetFileNameWithoutExtension(dialog.FileName);
                    IsUploadSuccess = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to upload image: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
