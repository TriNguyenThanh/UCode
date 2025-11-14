using System;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using UCode.Desktop.Helpers;

namespace UCode.Desktop.ViewModels
{
    public class TestCaseEditViewModel : ViewModelBase
    {
        private string _inputText = string.Empty;
        private string _outputText = string.Empty;
        private bool _isEditing;

        public string InputText
        {
            get => _inputText;
            set => SetProperty(ref _inputText, value);
        }

        public string OutputText
        {
            get => _outputText;
            set => SetProperty(ref _outputText, value);
        }

        public bool IsEditing
        {
            get => _isEditing;
            set => SetProperty(ref _isEditing, value);
        }

        public string SaveButtonText => IsEditing ? "Cập nhật" : "Thêm";

        public ICommand SaveTestCaseCommand { get; }

        public TestCaseEditViewModel()
        {
            SaveTestCaseCommand = new RelayCommand(_ => ExecuteSave());
        }

        public void Initialize(string input = "", string output = "")
        {
            IsEditing = !string.IsNullOrEmpty(input) || !string.IsNullOrEmpty(output);
            InputText = input;
            OutputText = output;
        }

        private async void ExecuteSave()
        {
            if (string.IsNullOrWhiteSpace(InputText))
            {
                await GetMetroWindow()?.ShowMessageAsync("Thông báo", "Vui lòng nhập Input");
                return;
            }

            if (string.IsNullOrWhiteSpace(OutputText))
            {
                await GetMetroWindow()?.ShowMessageAsync("Thông báo", "Vui lòng nhập Expected Output");
                return;
            }

            // Close dialog with success
            foreach (Window window in Application.Current.Windows)
            {
                if (window.DataContext == this)
                {
                    window.DialogResult = true;
                    window.Close();
                    break;
                }
            }
        }
    }
}

