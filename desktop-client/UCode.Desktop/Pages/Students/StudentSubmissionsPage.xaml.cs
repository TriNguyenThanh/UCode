using System.Windows.Controls;
using UCode.Desktop.ViewModels.Students;

namespace UCode.Desktop.Pages.Students
{
    public partial class StudentSubmissionsPage : UserControl
    {
        public StudentSubmissionsPage()
        {
            InitializeComponent();
            DataContext = new StudentSubmissionsViewModel();
        }

        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            if (DataContext is StudentSubmissionsViewModel viewModel)
            {
                e.Row.Header = ((viewModel.PageNumber - 1) * viewModel.PageSize + e.Row.GetIndex() + 1).ToString();
            }
            else
            {
                e.Row.Header = (e.Row.GetIndex() + 1).ToString();
            }
        }
    }
}
