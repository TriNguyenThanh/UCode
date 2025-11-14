using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UCode.Desktop.ViewModels;

namespace UCode.Desktop.Controls
{
    public partial class VisualSelectTabControl : UserControl
    {
        public VisualSelectTabControl()
        {
            InitializeComponent();
        }

        private void DataGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var dataGrid = sender as DataGrid;
            if (dataGrid == null) return;

            // Get the clicked element
            var dep = (DependencyObject)e.OriginalSource;

            // Walk up the visual tree to find if we clicked on a cell
            while (dep != null && dep != dataGrid)
            {
                // Don't toggle if clicking on checkbox directly
                if (dep is CheckBox)
                    return;

                if (dep is DataGridCell cell)
                {
                    // Get the row
                    var row = DataGridRow.GetRowContainingElement(cell);
                    if (row != null && row.Item is StudentItemViewModel student)
                    {
                        // Toggle selection
                        student.IsSelected = !student.IsSelected;
                    }
                    return;
                }

                dep = System.Windows.Media.VisualTreeHelper.GetParent(dep);
            }
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            // Prevent event bubbling to avoid double toggle
            e.Handled = true;
        }
    }
}

