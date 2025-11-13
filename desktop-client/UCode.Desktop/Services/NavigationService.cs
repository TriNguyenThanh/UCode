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
            
            // Initialize Page types
            if (page.GetType().Name == "ProblemEditPage" && parameter is string problemId)
            {
                var method = page.GetType().GetMethod("InitializeWithProblemId");
                method?.Invoke(page, new object[] { problemId });
            }
        }
    }
}
