using UCode.Desktop.Helpers;
using UCode.Desktop.Services;

namespace UCode.Desktop.ViewModels
{
    public class AddStudentDialogViewModel : ViewModelBase
    {
        private int _selectedTabIndex;
        private bool _isLoading;

        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set => SetProperty(ref _selectedTabIndex, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public VisualSelectTabViewModel VisualSelectTab { get; }
        public ImportExcelTabViewModel ImportExcelTab { get; }

        public AddStudentDialogViewModel(
            string classId,
            ClassService classService)
        {
            VisualSelectTab = new VisualSelectTabViewModel(classService);
            ImportExcelTab = new ImportExcelTabViewModel(classService);

            // Initialize tabs with classId
            VisualSelectTab.Initialize(classId);
            ImportExcelTab.Initialize(classId);
        }

        public void OnSuccess()
        {
            // Called when students are added successfully
            // Close dialog logic will be handled in code-behind
        }
    }
}

