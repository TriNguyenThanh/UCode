using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using UCode.Desktop.Helpers;
using UCode.Desktop.Models;
using UCode.Desktop.Services;

namespace UCode.Desktop.ViewModels
{
    public class SelectedProblemItem
    {
        public string ProblemId { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Difficulty { get; set; } = string.Empty;
        public int Points { get; set; }
        public int OrderIndex { get; set; }
    }

    public class AvailableProblemItem
    {
        public string ProblemId { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Difficulty { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
    }

    public class TeacherAssignmentEditViewModel : ViewModelBase
    {
        private readonly AssignmentService _assignmentService;
        private readonly ClassService _classService;
        private readonly ProblemService _problemService;
        
        private bool _isLoading;
        private bool _isSaving;
        private string _error = string.Empty;
        private string _assignmentId = string.Empty;
        private bool _isNewAssignment = true;

        // Form fields
        private string _classId = string.Empty;
        private string _title = string.Empty;
        private string _description = string.Empty;
        private string _assignmentType = "HOMEWORK";
        private string _status = "DRAFT";
        private DateTime _startTime = DateTime.Now;
        private DateTime _endTime = DateTime.Now.AddDays(7);
        private bool _allowLateSubmission = false;

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public bool IsSaving
        {
            get => _isSaving;
            set => SetProperty(ref _isSaving, value);
        }

        public string Error
        {
            get => _error;
            set => SetProperty(ref _error, value);
        }

        public string ClassId
        {
            get => _classId;
            set => SetProperty(ref _classId, value);
        }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public string AssignmentType
        {
            get => _assignmentType;
            set => SetProperty(ref _assignmentType, value);
        }

        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public DateTime StartTime
        {
            get => _startTime;
            set => SetProperty(ref _startTime, value);
        }

        public DateTime EndTime
        {
            get => _endTime;
            set => SetProperty(ref _endTime, value);
        }

        public bool AllowLateSubmission
        {
            get => _allowLateSubmission;
            set => SetProperty(ref _allowLateSubmission, value);
        }

        public ObservableCollection<SelectedProblemItem> SelectedProblems { get; } = new();
        public ObservableCollection<AvailableProblemItem> AvailableProblems { get; } = new();
        public ObservableCollection<Class> Classes { get; } = new();

        public bool HasSelectedProblems => SelectedProblems.Count > 0;

        public List<string> AssignmentTypes { get; } = new List<string> { "HOMEWORK", "EXAMINATION", "PRACTICE" };
        public List<string> Statuses { get; } = new List<string> { "DRAFT", "PUBLISHED", "CLOSED" };

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand AddProblemCommand { get; }
        public ICommand RemoveProblemCommand { get; }
        public ICommand MoveProblemUpCommand { get; }
        public ICommand MoveProblemDownCommand { get; }

        public TeacherAssignmentEditViewModel(
            AssignmentService assignmentService,
            ClassService classService,
            ProblemService problemService)
        {
            _assignmentService = assignmentService;
            _classService = classService;
            _problemService = problemService;

            SaveCommand = new RelayCommand(async _ => await SaveAssignmentAsync());
            CancelCommand = new RelayCommand(_ => ExecuteCancel());
            AddProblemCommand = new RelayCommand(_ => ExecuteAddProblem());
            RemoveProblemCommand = new RelayCommand(param => ExecuteRemoveProblem(param as string));
            MoveProblemUpCommand = new RelayCommand(param => ExecuteMoveProblemUp(param as string));
            MoveProblemDownCommand = new RelayCommand(param => ExecuteMoveProblemDown(param as string));

            // Subscribe to collection changed event to update HasSelectedProblems
            SelectedProblems.CollectionChanged += (s, e) => OnPropertyChanged(nameof(HasSelectedProblems));
        }

        public async Task InitializeAsync(string assignmentId = null, string classId = null)
        {
            _assignmentId = assignmentId;
            _isNewAssignment = string.IsNullOrEmpty(assignmentId);

            if (!string.IsNullOrEmpty(classId))
            {
                ClassId = classId;
            }

            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            IsLoading = true;
            Error = string.Empty;

            try
            {
                // Load classes
                await LoadClassesAsync();

                // Load available problems
                await LoadAvailableProblemsAsync();

                // Load assignment if editing
                if (!_isNewAssignment)
                {
                    await LoadAssignmentAsync();
                }
            }
            catch (Exception ex)
            {
                Error = $"Lỗi tải dữ liệu: {ex.Message}";
                await GetMetroWindow()?.ShowMessageAsync("Lỗi", Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadClassesAsync()
        {
            try
            {
                var response = await _classService.GetMyClassesAsync(1, 100);
                Classes.Clear();

                if (response?.Success == true && response.Data?.Items != null)
                {
                    foreach (var cls in response.Data.Items)
                    {
                        Classes.Add(cls);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading classes: {ex.Message}");
            }
        }

        private async Task LoadAvailableProblemsAsync()
        {
            try
            {
                var response = await _problemService.GetMyProblemsAsync(1, 100);
                AvailableProblems.Clear();

                if (response?.Success == true && response.Data?.Items != null)
                {
                    foreach (var problem in response.Data.Items)
                    {
                        AvailableProblems.Add(new AvailableProblemItem
                        {
                            ProblemId = problem.ProblemId,
                            Code = problem.Code,
                            Title = problem.Title,
                            Difficulty = problem.Difficulty.ToString(),
                            IsSelected = false
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading problems: {ex.Message}");
            }
        }

        private async Task LoadAssignmentAsync()
        {
            try
            {
                var response = await _assignmentService.GetAssignmentAsync(_assignmentId);
                if (response?.Success == true && response.Data != null)
                {
                    var assignment = response.Data;
                    
                    ClassId = assignment.ClassId;
                    Title = assignment.Title;
                    Description = assignment.Description;
                    AssignmentType = assignment.AssignmentType.ToString();
                    Status = assignment.Status.ToString();
                    StartTime = assignment.StartTime ?? DateTime.Now;
                    EndTime = assignment.EndTime ?? DateTime.Now.AddDays(7);
                    AllowLateSubmission = assignment.AllowLateSubmission;

                    // Load selected problems
                    SelectedProblems.Clear();
                    if (assignment.Problems != null)
                    {
                        foreach (var assignmentProblem in assignment.Problems.OrderBy(p => p.OrderIndex))
                        {
                            var problemResponse = await _problemService.GetProblemAsync(assignmentProblem.ProblemId);
                            if (problemResponse?.Success == true && problemResponse.Data != null)
                            {
                                var problem = problemResponse.Data;
                                SelectedProblems.Add(new SelectedProblemItem
                                {
                                    ProblemId = problem.ProblemId,
                                    Code = problem.Code,
                                    Title = problem.Title,
                                    Difficulty = problem.Difficulty.ToString(),
                                    Points = assignmentProblem.Points,
                                    OrderIndex = assignmentProblem.OrderIndex
                                });

                                // Mark as selected in available problems
                                var availableProblem = AvailableProblems.FirstOrDefault(p => p.ProblemId == problem.ProblemId);
                                if (availableProblem != null)
                                {
                                    availableProblem.IsSelected = true;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Error = $"Lỗi tải bài tập: {ex.Message}";
                await GetMetroWindow()?.ShowMessageAsync("Lỗi", Error);
            }
        }

        private async Task SaveAssignmentAsync()
        {
            // Validation
            if (string.IsNullOrWhiteSpace(ClassId))
            {
                await GetMetroWindow()?.ShowMessageAsync("Thông báo", "Vui lòng chọn lớp học");
                return;
            }

            if (string.IsNullOrWhiteSpace(Title))
            {
                await GetMetroWindow()?.ShowMessageAsync("Thông báo", "Vui lòng nhập tiêu đề bài tập");
                return;
            }

            if (SelectedProblems.Count == 0)
            {
                await GetMetroWindow()?.ShowMessageAsync("Thông báo", "Vui lòng chọn ít nhất một bài tập");
                return;
            }

            IsSaving = true;
            Error = string.Empty;

            try
            {
                var problems = SelectedProblems.Select((p, index) => new AssignmentProblem
                {
                    ProblemId = p.ProblemId,
                    Points = p.Points,
                    OrderIndex = index + 1
                }).ToList();

                if (_isNewAssignment)
                {
                    var request = new Models.CreateAssignmentRequest
                    {
                        ClassId = ClassId,
                        Title = Title,
                        Description = Description,
                        AssignmentType = AssignmentType,
                        Status = Status,
                        StartTime = StartTime.ToString("o"),
                        EndTime = EndTime.ToString("o"),
                        AllowLateSubmission = AllowLateSubmission,
                        Problems = problems
                    };

                    var response = await _assignmentService.CreateAssignmentAsync(request);
                    if (response?.Success == true)
                    {
                        await GetMetroWindow()?.ShowMessageAsync("Thành công", "Tạo bài tập thành công!");
                        ExecuteCancel();
                    }
                    else
                    {
                        await GetMetroWindow()?.ShowMessageAsync("Lỗi", $"Tạo bài tập thất bại: {response?.Message}");
                    }
                }
                else
                {
                    var request = new Services.UpdateAssignmentRequest
                    {
                        ClassId = ClassId,
                        Title = Title,
                        Description = Description,
                        AssignmentType = AssignmentType,
                        Status = Status,
                        StartTime = StartTime.ToString("o"),
                        EndTime = EndTime.ToString("o"),
                        AllowLateSubmission = AllowLateSubmission,
                        Problems = problems
                    };

                    var response = await _assignmentService.UpdateAssignmentAsync(_assignmentId, request);
                    if (response?.Success == true)
                    {
                        await GetMetroWindow()?.ShowMessageAsync("Thành công", "Cập nhật bài tập thành công!");
                        ExecuteCancel();
                    }
                    else
                    {
                        await GetMetroWindow()?.ShowMessageAsync("Lỗi", $"Cập nhật bài tập thất bại: {response?.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Error = $"Lỗi lưu bài tập: {ex.Message}";
                await GetMetroWindow()?.ShowMessageAsync("Lỗi", Error);
            }
            finally
            {
                IsSaving = false;
            }
        }

        private void ExecuteCancel()
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window.DataContext == this)
                {
                    window.Close();
                    break;
                }
            }
        }

        private async void ExecuteAddProblem()
        {
            try
            {
                // Get existing problems as AssignmentProblem list
                var existingProblems = SelectedProblems.Select(p => new AssignmentProblem
                {
                    ProblemId = p.ProblemId,
                    Points = p.Points,
                    OrderIndex = p.OrderIndex
                }).ToList();
                
                var viewModel = new AddProblemDialogViewModel(
                    _assignmentId ?? string.Empty,
                    _problemService,
                    _assignmentService,
                    existingProblems);
                
                // Load problems in dialog
                await viewModel.LoadProblemsAsync();
                
                var dialog = new Views.AddProblemDialog(viewModel)
                {
                    Owner = Application.Current.MainWindow
                };
                
                if (dialog.ShowDialog() == true)
                {
                    // Get selected problems from dialog
                    var selectedProblems = viewModel.GetSelectedProblems();
                    
                    if (selectedProblems != null && selectedProblems.Count > 0)
                    {
                        // Get existing order indices
                        var existingIndices = new HashSet<int>(SelectedProblems.Select(p => p.OrderIndex));
                        
                        // Add new problems to the list
                        foreach (var problem in selectedProblems)
                        {
                            // Check if problem already exists
                            if (!SelectedProblems.Any(p => p.ProblemId == problem.ProblemId))
                            {
                                // Find the smallest available order index starting from 1
                                int newOrderIndex = 1;
                                while (existingIndices.Contains(newOrderIndex))
                                {
                                    newOrderIndex++;
                                }
                                existingIndices.Add(newOrderIndex);
                                
                                SelectedProblems.Add(new SelectedProblemItem
                                {
                                    ProblemId = problem.ProblemId,
                                    Code = problem.Code,
                                    Title = problem.Title,
                                    Difficulty = problem.Difficulty.ToString(),
                                    Points = problem.Points,
                                    OrderIndex = newOrderIndex
                                });
                            }
                        }
                        
                        OnPropertyChanged(nameof(HasSelectedProblems));
                        await GetMetroWindow()?.ShowMessageAsync("Thành công", $"Đã thêm {selectedProblems.Count} bài vào danh sách!");
                    }
                }
            }
            catch (Exception ex)
            {
                await GetMetroWindow()?.ShowMessageAsync(
                    "Lỗi",
                    $"Không thể mở cửa sổ chọn bài: {ex.Message}");
            }
        }

        private void ExecuteRemoveProblem(string problemId)
        {
            if (string.IsNullOrEmpty(problemId)) return;

            var selectedProblem = SelectedProblems.FirstOrDefault(p => p.ProblemId == problemId);
            if (selectedProblem != null)
            {
                SelectedProblems.Remove(selectedProblem);

                // Don't update OrderIndex - keep original numbers

                var availableProblem = AvailableProblems.FirstOrDefault(p => p.ProblemId == problemId);
                if (availableProblem != null)
                {
                    availableProblem.IsSelected = false;
                }
            }
        }

        private void ExecuteMoveProblemUp(string problemId)
        {
            if (string.IsNullOrEmpty(problemId)) return;

            var index = SelectedProblems.ToList().FindIndex(p => p.ProblemId == problemId);
            if (index > 0)
            {
                var item = SelectedProblems[index];
                SelectedProblems.RemoveAt(index);
                SelectedProblems.Insert(index - 1, item);

                // Don't update OrderIndex - keep original numbers
            }
        }

        private void ExecuteMoveProblemDown(string problemId)
        {
            if (string.IsNullOrEmpty(problemId)) return;

            var index = SelectedProblems.ToList().FindIndex(p => p.ProblemId == problemId);
            if (index < SelectedProblems.Count - 1)
            {
                var item = SelectedProblems[index];
                SelectedProblems.RemoveAt(index);
                SelectedProblems.Insert(index + 1, item);

                // Don't update OrderIndex - keep original numbers
            }
        }
    }
}

