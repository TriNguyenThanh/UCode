using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using UCode.Desktop.Helpers;
using UCode.Desktop.Models;
using UCode.Desktop.Models.Enums;
using UCode.Desktop.Services;
using UCode.Desktop.Views;

namespace UCode.Desktop.ViewModels
{
    public class DatasetEditViewModel : ViewModelBase
    {
        private readonly string _problemId;
        private readonly DatasetService _datasetService;
        private string _datasetId = string.Empty;
        private string _datasetName = string.Empty;
        private DatasetKind _datasetKind = DatasetKind.SAMPLE;
        private bool _isEditing;
        private bool _isSaving;
        private bool _allTestCasesSelected;

        public ObservableCollection<TestCaseItemViewModel> TestCases { get; } = new();
        public ObservableCollection<DatasetKind> DatasetKindOptions { get; } = new()
        {
            DatasetKind.SAMPLE,
            DatasetKind.PUBLIC,
            DatasetKind.PRIVATE,
            DatasetKind.OFFICIAL
        };

        public string DatasetName
        {
            get => _datasetName;
            set => SetProperty(ref _datasetName, value);
        }

        public DatasetKind DatasetKind
        {
            get => _datasetKind;
            set => SetProperty(ref _datasetKind, value);
        }

        public bool IsEditing
        {
            get => _isEditing;
            set => SetProperty(ref _isEditing, value);
        }

        public bool IsSaving
        {
            get => _isSaving;
            set => SetProperty(ref _isSaving, value);
        }

        public bool AllTestCasesSelected
        {
            get => _allTestCasesSelected;
            set
            {
                if (SetProperty(ref _allTestCasesSelected, value))
                {
                    foreach (var tc in TestCases)
                    {
                        tc.IsSelected = value;
                    }
                    OnPropertyChanged(nameof(HasSelectedTestCases));
                    OnPropertyChanged(nameof(SelectedTestCases));
                }
            }
        }

        public bool HasNoTestCases => TestCases.Count == 0;
        public bool HasTestCases => TestCases.Count > 0;
        public List<TestCaseItemViewModel> SelectedTestCases => TestCases.Where(tc => tc.IsSelected).ToList();
        public bool HasSelectedTestCases => SelectedTestCases.Count > 0;
        public string SaveButtonText => IsSaving ? "Đang lưu..." : (IsEditing ? "Cập nhật Dataset" : "Lưu Dataset");

        public ICommand AddTestCaseCommand { get; }
        public ICommand EditTestCaseCommand { get; }
        public ICommand DeleteTestCaseCommand { get; }
        public ICommand DeleteSelectedTestCasesCommand { get; }
        public ICommand DownloadTemplateCommand { get; }
        public ICommand ImportExcelCommand { get; }
        public ICommand SaveDatasetCommand { get; }

        public DatasetEditViewModel(string problemId, DatasetService datasetService)
        {
            _problemId = problemId;
            _datasetService = datasetService;

            AddTestCaseCommand = new RelayCommand(_ => AddTestCase());
            EditTestCaseCommand = new RelayCommand(param => EditTestCase(param as TestCaseItemViewModel));
            DeleteTestCaseCommand = new RelayCommand(param => DeleteTestCase(param as TestCaseItemViewModel));
            DeleteSelectedTestCasesCommand = new RelayCommand(_ => DeleteSelectedTestCases());
            DownloadTemplateCommand = new RelayCommand(_ => DownloadTemplate());
            ImportExcelCommand = new RelayCommand(_ => ImportExcel());
            SaveDatasetCommand = new RelayCommand(async _ => await SaveDatasetAsync());
        }

        public void Initialize(Dataset? dataset = null)
        {
            if (dataset != null)
            {
                IsEditing = true;
                _datasetId = dataset.DatasetId ?? string.Empty;
                DatasetName = dataset.Name;
                DatasetKind = dataset.Kind;

                TestCases.Clear();
                foreach (var tc in dataset.TestCases.OrderBy(t => t.IndexNo))
                {
                    TestCases.Add(new TestCaseItemViewModel
                    {
                        InputRef = tc.InputRef,
                        OutputRef = tc.OutputRef,
                        IndexNo = tc.IndexNo,
                        IsSelected = false
                    });
                }
            }
            else
            {
                IsEditing = false;
                DatasetName = "";
                DatasetKind = DatasetKind.SAMPLE;
                TestCases.Clear();
            }

            OnPropertyChanged(nameof(HasNoTestCases));
            OnPropertyChanged(nameof(HasTestCases));
        }

        private void AddTestCase()
        {
            var viewModel = new TestCaseEditViewModel();
            viewModel.Initialize();

            var dialog = new TestCaseEditDialog(viewModel);
            dialog.Owner = Application.Current.MainWindow;

            if (dialog.ShowDialog() == true)
            {
                TestCases.Add(new TestCaseItemViewModel
                {
                    InputRef = viewModel.InputText,
                    OutputRef = viewModel.OutputText,
                    IndexNo = TestCases.Count + 1,
                    IsSelected = false
                });

                OnPropertyChanged(nameof(HasNoTestCases));
                OnPropertyChanged(nameof(HasTestCases));
            }
        }

        private void EditTestCase(TestCaseItemViewModel? testCase)
        {
            if (testCase == null) return;

            var viewModel = new TestCaseEditViewModel();
            viewModel.Initialize(testCase.InputRef, testCase.OutputRef);

            var dialog = new TestCaseEditDialog(viewModel);
            dialog.Owner = Application.Current.MainWindow;

            if (dialog.ShowDialog() == true)
            {
                testCase.InputRef = viewModel.InputText;
                testCase.OutputRef = viewModel.OutputText;
            }
        }

        private void DeleteTestCase(TestCaseItemViewModel? testCase)
        {
            if (testCase == null) return;

            if (MessageBox.Show($"Bạn có chắc chắn muốn xóa test case #{testCase.IndexNo}?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            TestCases.Remove(testCase);
            ReindexTestCases();

            OnPropertyChanged(nameof(HasNoTestCases));
            OnPropertyChanged(nameof(HasTestCases));
        }

        private void DeleteSelectedTestCases()
        {
            if (!HasSelectedTestCases) return;

            if (MessageBox.Show($"Bạn có chắc chắn muốn xóa {SelectedTestCases.Count} test case(s) đã chọn?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            var toRemove = SelectedTestCases.ToList();
            foreach (var tc in toRemove)
            {
                TestCases.Remove(tc);
            }

            ReindexTestCases();

            OnPropertyChanged(nameof(HasNoTestCases));
            OnPropertyChanged(nameof(HasTestCases));
            OnPropertyChanged(nameof(HasSelectedTestCases));
            OnPropertyChanged(nameof(SelectedTestCases));
        }

        private void ReindexTestCases()
        {
            for (int i = 0; i < TestCases.Count; i++)
            {
                TestCases[i].IndexNo = i + 1;
            }
        }

        private void DownloadTemplate()
        {
            // TODO: Implement Excel template download using ClosedXML
            // See web implementation for reference (downloadExcelTemplate)
            MessageBox.Show(
                "Excel Template Download\n\n" +
                "TODO: Implement using ClosedXML NuGet package\n\n" +
                "Template should have 2 columns:\n" +
                "- Input\n" +
                "- Output\n\n" +
                "See: client/app/utils/excelImport.ts (downloadExcelTemplate)",
                "Feature Not Implemented",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void ImportExcel()
        {
            // TODO: Implement Excel import using ClosedXML
            // See web implementation for reference (importExcelFile)
            
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Excel Files|*.xlsx;*.xls;*.csv|All Files|*.*",
                Title = "Chọn file Excel để import test cases"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                MessageBox.Show(
                    $"Excel Import\n\n" +
                    $"File: {openFileDialog.FileName}\n\n" +
                    "TODO: Implement using ClosedXML NuGet package\n\n" +
                    "Should parse Excel file and add test cases to TestCases collection\n" +
                    "Expected columns: Input, Output\n\n" +
                    "See: client/app/utils/excelImport.ts (importExcelFile)\n" +
                    "and client/app/components/DatasetManagement.tsx (handleImportExcel)",
                    "Feature Not Implemented",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private async Task SaveDatasetAsync()
        {
            // Validation
            if (string.IsNullOrWhiteSpace(DatasetName))
            {
                MessageBox.Show("Vui lòng nhập tên dataset", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (TestCases.Count == 0)
            {
                MessageBox.Show("Vui lòng thêm ít nhất một test case", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (TestCases.Any(tc => string.IsNullOrWhiteSpace(tc.InputRef) || string.IsNullOrWhiteSpace(tc.OutputRef)))
            {
                MessageBox.Show("Tất cả test cases phải có input và output", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IsSaving = true;

            try
            {
                var testCasesList = TestCases.Select((tc, idx) => new TestCaseRequest
                {
                    InputRef = tc.InputRef,
                    OutputRef = tc.OutputRef,
                    IndexNo = idx + 1
                }).ToList();

                if (IsEditing)
                {
                    var request = new UpdateDatasetRequest
                    {
                        DatasetId = _datasetId,
                        Name = DatasetName.Trim(),
                        Kind = DatasetKind.ToString(),
                        ProblemId = _problemId,
                        TestCases = testCasesList
                    };

                    var response = await _datasetService.UpdateDatasetAsync(request);
                    if (response?.Success == true)
                    {
                        MessageBox.Show("Cập nhật dataset thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                        
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
                    else
                    {
                        MessageBox.Show($"Cập nhật thất bại: {response?.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    var request = new CreateDatasetRequest
                    {
                        ProblemId = _problemId,
                        Name = DatasetName.Trim(),
                        Kind = DatasetKind.ToString(),
                        TestCases = testCasesList
                    };

                    var response = await _datasetService.CreateDatasetAsync(request);
                    if (response?.Success == true)
                    {
                        MessageBox.Show("Tạo dataset thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                        
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
                    else
                    {
                        MessageBox.Show($"Tạo dataset thất bại: {response?.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsSaving = false;
            }
        }
    }
}

