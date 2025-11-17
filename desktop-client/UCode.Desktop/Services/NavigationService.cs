using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace UCode.Desktop.Services
{
    public class NavigationService
    {
        private readonly Stack<(UserControl page, object? parameter)> _navigationStack = new();
        private ContentControl? _frame;

        public event EventHandler<bool>? CanGoBackChanged;

        public bool CanGoBack => _navigationStack.Count > 1;

        public void SetFrame(ContentControl frame)
        {
            _frame = frame;
        }

        public void NavigateTo(UserControl page, object? parameter = null)
        {
            if (_frame == null)
                throw new InvalidOperationException("Frame not set. Call SetFrame first.");

            _navigationStack.Push((page, parameter));
            _frame.Content = page;

            // Initialize the page if it has an Initialize method
            InitializePage(page, parameter);

            CanGoBackChanged?.Invoke(this, CanGoBack);
        }

        public void GoBack()
        {
            if (_navigationStack.Count <= 1)
                return;

            // Remove current page
            _navigationStack.Pop();

            // Get previous page
            var (previousPage, previousParameter) = _navigationStack.Peek();

            if (_frame != null)
            {
                _frame.Content = previousPage;
                
                // Re-initialize the page if needed (refresh data)
                if (previousPage.DataContext is ViewModels.TeacherHomeViewModel homeViewModel)
                {
                    _ = homeViewModel.LoadDataAsync();
                }
                else if (previousPage.DataContext is ViewModels.TeacherClassViewModel classViewModel)
                {
                    if (previousParameter is string classId)
                    {
                        _ = classViewModel.InitializeAsync(classId);
                    }
                }
            }

            CanGoBackChanged?.Invoke(this, CanGoBack);
        }

        public void ClearNavigationStack()
        {
            _navigationStack.Clear();
            CanGoBackChanged?.Invoke(this, CanGoBack);
        }

        private void InitializePage(UserControl page, object? parameter)
        {
            // Initialize ViewModels that have async initialization
            if (page.DataContext is ViewModels.TeacherClassViewModel classViewModel && parameter is string classId)
            {
                classViewModel.SetNavigationService(this);
                _ = classViewModel.InitializeAsync(classId);
            }
            else if (page.DataContext is ViewModels.TeacherAssignmentViewModel assignmentViewModel && parameter is string assignmentId)
            {
                _ = assignmentViewModel.InitializeAsync(assignmentId);
            }
            else if (page.DataContext is ViewModels.TeacherHomeViewModel homeViewModel)
            {
                _ = homeViewModel.LoadDataAsync();
            }
            else if (page.DataContext is ViewModels.TeacherProblemsViewModel problemsViewModel)
            {
                _ = problemsViewModel.LoadProblemsAsync();
            }
            else if (page.DataContext is ViewModels.TeacherProblemSubmissionsViewModel submissionsViewModel)
            {
                // Handle anonymous type parameter with assignmentId and problemId
                var paramType = parameter?.GetType();
                if (paramType != null)
                {
                    var assignmentIdProp = paramType.GetProperty("assignmentId");
                    var problemIdProp = paramType.GetProperty("problemId");
                    if (assignmentIdProp != null && problemIdProp != null)
                    {
                        var submAssignmentId = assignmentIdProp.GetValue(parameter)?.ToString();
                        var submProblemId = problemIdProp.GetValue(parameter)?.ToString();
                        if (!string.IsNullOrEmpty(submAssignmentId) && !string.IsNullOrEmpty(submProblemId))
                        {
                            _ = submissionsViewModel.InitializeAsync(submAssignmentId, submProblemId);
                        }
                    }
                }
            }
            else if (page.DataContext is ViewModels.SubmissionDetailViewModel detailViewModel)
            {
                // Handle anonymous type parameter with assignmentId, problemId, and submissionId
                var paramType = parameter?.GetType();
                if (paramType != null)
                {
                    var assignmentIdProp = paramType.GetProperty("assignmentId");
                    var problemIdProp = paramType.GetProperty("problemId");
                    var submissionIdProp = paramType.GetProperty("submissionId");
                    if (assignmentIdProp != null && problemIdProp != null && submissionIdProp != null)
                    {
                        var detailAssignmentId = assignmentIdProp.GetValue(parameter)?.ToString();
                        var detailProblemId = problemIdProp.GetValue(parameter)?.ToString();
                        var detailSubmissionId = submissionIdProp.GetValue(parameter)?.ToString();
                        if (!string.IsNullOrEmpty(detailAssignmentId) && !string.IsNullOrEmpty(detailProblemId) && !string.IsNullOrEmpty(detailSubmissionId))
                        {
                            _ = detailViewModel.InitializeAsync(detailAssignmentId, detailProblemId, detailSubmissionId);
                        }
                    }
                }
            }
            
            // Initialize Page types
            if (page.GetType().Name == "ProblemEditPage" && parameter is string problemId)
            {
                var method = page.GetType().GetMethod("InitializeWithProblemId");
                method?.Invoke(page, new object[] { problemId });
            }
        }
    }
}
