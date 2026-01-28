using System.Windows.Controls;
using UCode.Desktop.ViewModels.Students;

namespace UCode.Desktop.Pages.Students
{
    public partial class StudentAssignmentsPage : UserControl
    {
        public StudentAssignmentsPage()
        {
            InitializeComponent();
            DataContext = new StudentAssignmentsViewModel();
        }

        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }
    }
}
