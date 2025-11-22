using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
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
                    var testCase = new TestCaseItemViewModel
                    {
                        InputRef = tc.InputRef,
                        OutputRef = tc.OutputRef,
                        IndexNo = tc.IndexNo,
                        IsSelected = false
                    };
                    
                    testCase.PropertyChanged += TestCase_PropertyChanged;
                    TestCases.Add(testCase);
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
                var testCase = new TestCaseItemViewModel
                {
                    InputRef = viewModel.InputText,
                    OutputRef = viewModel.OutputText,
                    IndexNo = TestCases.Count + 1,
                    IsSelected = false
                };
                
                testCase.PropertyChanged += TestCase_PropertyChanged;
                TestCases.Add(testCase);

                OnPropertyChanged(nameof(HasNoTestCases));
                OnPropertyChanged(nameof(HasTestCases));
            }
        }

        private void TestCase_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TestCaseItemViewModel.IsSelected))
            {
                OnPropertyChanged(nameof(HasSelectedTestCases));
                OnPropertyChanged(nameof(SelectedTestCases));
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

        private async void DeleteTestCase(TestCaseItemViewModel? testCase)
        {
            if (testCase == null) return;

            var result = await GetMetroWindow()?.ShowMessageAsync(
                "Xác nhận",
                $"Bạn có chắc chắn muốn xóa test case #{testCase.IndexNo}?",
                MessageDialogStyle.AffirmativeAndNegative);

            if (result != MessageDialogResult.Affirmative)
            {
                return;
            }

            TestCases.Remove(testCase);
            ReindexTestCases();

            OnPropertyChanged(nameof(HasNoTestCases));
            OnPropertyChanged(nameof(HasTestCases));
        }

        private async void DeleteSelectedTestCases()
        {
            if (!HasSelectedTestCases) return;

            var result = await GetMetroWindow()?.ShowMessageAsync(
                "Xác nhận",
                $"Bạn có chắc chắn muốn xóa {SelectedTestCases.Count} test case(s) đã chọn?",
                MessageDialogStyle.AffirmativeAndNegative);

            if (result != MessageDialogResult.Affirmative)
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

        private async void DownloadTemplate()
        {
            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel Files|*.xlsx",
                    Title = "Lưu template Excel",
                    FileName = "test_cases_template.xlsx"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    using (var workbook = new ClosedXML.Excel.XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("Test Cases");

                        // Header row with styling
                        worksheet.Cell(1, 1).Value = "Input";
                        worksheet.Cell(1, 2).Value = "Output";
                        worksheet.Cell(1, 3).Value = "Score";

                        // Style header
                        var headerRange = worksheet.Range(1, 1, 1, 3);
                        headerRange.Style.Font.Bold = true;
                        headerRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#0071E3");
                        headerRange.Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
                        headerRange.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;

                        // Sample data
                        worksheet.Cell(2, 1).Value = "0";
                        worksheet.Cell(2, 2).Value = "0";
                        worksheet.Cell(2, 3).Value = 100;

                        worksheet.Cell(3, 1).Value = "1";
                        worksheet.Cell(3, 2).Value = "1";
                        worksheet.Cell(3, 3).Value = 100;

                        // Auto-fit columns
                        worksheet.Columns().AdjustToContents();

                        workbook.SaveAs(saveFileDialog.FileName);
                    }

                    await GetMetroWindow()?.ShowMessageAsync("Thành công", "Đã tải template Excel thành công!");
                }
            }
            catch (Exception ex)
            {
                await GetMetroWindow()?.ShowMessageAsync("Lỗi", $"Không thể tải template: {ex.Message}");
            }
        }

        private async void ImportExcel()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Excel Files|*.xlsx;*.xls|All Files|*.*",
                Title = "Chọn file Excel để import test cases"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    using (var workbook = new ClosedXML.Excel.XLWorkbook(openFileDialog.FileName))
                    {
                        var worksheet = workbook.Worksheet(1);
                        var rows = worksheet.RangeUsed().RowsUsed().Skip(1); // Skip header row

                        int importedCount = 0;
                        int startIndex = TestCases.Count + 1;

                        foreach (var row in rows)
                        {
                            var input = row.Cell(1).GetString().Trim();
                            var output = row.Cell(2).GetString().Trim();

                            // Skip empty rows
                            if (string.IsNullOrWhiteSpace(input) && string.IsNullOrWhiteSpace(output))
                                continue;

                            var testCase = new TestCaseItemViewModel
                            {
                                InputRef = input,
                                OutputRef = output,
                                IndexNo = startIndex + importedCount,
                                IsSelected = false
                            };

                            testCase.PropertyChanged += TestCase_PropertyChanged;
                            TestCases.Add(testCase);
                            importedCount++;
                        }

                        OnPropertyChanged(nameof(HasNoTestCases));
                        OnPropertyChanged(nameof(HasTestCases));

                        if (importedCount > 0)
                        {
                            await GetMetroWindow()?.ShowMessageAsync(
                                "Thành công", 
                                $"Đã import {importedCount} test case(s) từ Excel!");
                        }
                        else
                        {
                            await GetMetroWindow()?.ShowMessageAsync(
                                "Thông báo", 
                                "Không tìm thấy test case nào trong file Excel.\n\n" +
                                "Đảm bảo file có định dạng:\n" +
                                "- Dòng 1: Header (Input, Output)\n" +
                                "- Các dòng tiếp theo: Dữ liệu test cases");
                        }
                    }
                }
                catch (Exception ex)
                {
                    await GetMetroWindow()?.ShowMessageAsync(
                        "Lỗi", 
                        $"Không thể đọc file Excel: {ex.Message}\n\n" +
                        "Đảm bảo file Excel có định dạng đúng với 2 cột: Input và Output");
                }
            }
        }

        private async Task SaveDatasetAsync()
        {
            // Validation
            if (string.IsNullOrWhiteSpace(DatasetName))
            {
                await GetMetroWindow()?.ShowMessageAsync("Thông báo", "Vui lòng nhập tên dataset");
                return;
            }

            if (TestCases.Count == 0)
            {
                await GetMetroWindow()?.ShowMessageAsync("Thông báo", "Vui lòng thêm ít nhất một test case");
                return;
            }

            if (TestCases.Any(tc => string.IsNullOrWhiteSpace(tc.InputRef) || string.IsNullOrWhiteSpace(tc.OutputRef)))
            {
                await GetMetroWindow()?.ShowMessageAsync("Thông báo", "Tất cả test cases phải có input và output");
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
                        await GetMetroWindow()?.ShowMessageAsync("Thành công", "Cập nhật dataset thành công!");
                        
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
                        await GetMetroWindow()?.ShowMessageAsync("Lỗi", $"Cập nhật thất bại: {response?.Message}");
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
                        await GetMetroWindow()?.ShowMessageAsync("Thành công", "Tạo dataset thành công!");
                        
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
                        await GetMetroWindow()?.ShowMessageAsync("Lỗi", $"Tạo dataset thất bại: {response?.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                await GetMetroWindow()?.ShowMessageAsync("Lỗi", $"Lỗi: {ex.Message}");
            }
            finally
            {
                IsSaving = false;
            }
        }
    }
}

