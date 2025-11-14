# Custom DataGrid Usage Guide

## Overview
Dự án đã có một bộ style DataGrid đẹp và hiện đại có thể tái sử dụng cho tất cả các trang.

## Styles Available

### 1. ModernDataGrid
Style chính cho DataGrid với thiết kế hiện đại, sạch sẽ.

```xml
<DataGrid Style="{StaticResource ModernDataGrid}"
          ItemsSource="{Binding YourCollection}">
    <!-- Columns here -->
</DataGrid>
```

### 2. ModernDataGridRow
Style cho các hàng với hiệu ứng hover và selection đẹp mắt.

```xml
<DataGrid RowStyle="{StaticResource ModernDataGridRow}">
    <!-- ... -->
</DataGrid>
```

### 3. ModernDataGridCell
Style cho các ô với padding và alignment tốt.

```xml
<DataGrid CellStyle="{StaticResource ModernDataGridCell}">
    <!-- ... -->
</DataGrid>
```

### 4. ModernDataGridColumnHeader
Style cho header với màu nền và typography rõ ràng.

```xml
<DataGrid ColumnHeaderStyle="{StaticResource ModernDataGridColumnHeader}">
    <!-- ... -->
</DataGrid>
```

### 5. SelectionCheckBox
Checkbox đẹp cho việc chọn hàng.

```xml
<CheckBox Style="{StaticResource SelectionCheckBox}"
          IsChecked="{Binding IsSelected}"/>
```

### 6. StatusBadge
Badge đẹp cho hiển thị trạng thái.

```xml
<Border Style="{StaticResource StatusBadge}"
        Background="#E8F5E9">
    <TextBlock Text="Active" Foreground="#4CAF50"/>
</Border>
```

## Complete Example

```xml
<DataGrid ItemsSource="{Binding Items}"
          Style="{StaticResource ModernDataGrid}"
          RowStyle="{StaticResource ModernDataGridRow}"
          CellStyle="{StaticResource ModernDataGridCell}"
          ColumnHeaderStyle="{StaticResource ModernDataGridColumnHeader}">
    <DataGrid.Columns>
        <!-- Checkbox Column -->
        <DataGridTemplateColumn Width="60">
            <DataGridTemplateColumn.Header>
                <CheckBox IsChecked="{Binding SelectAll}"
                          Style="{StaticResource SelectionCheckBox}"/>
            </DataGridTemplateColumn.Header>
            <DataGridTemplateColumn.CellTemplate>
                <DataTemplate>
                    <CheckBox IsChecked="{Binding IsSelected}"
                              Style="{StaticResource SelectionCheckBox}"/>
                </DataTemplate>
            </DataGridTemplateColumn.CellTemplate>
        </DataGridTemplateColumn>
        
        <!-- Text Columns -->
        <DataGridTextColumn Header="Name" Binding="{Binding Name}" Width="*"/>
        <DataGridTextColumn Header="Code" Binding="{Binding Code}" Width="120"/>
        
        <!-- Status Column -->
        <DataGridTemplateColumn Header="Status" Width="110">
            <DataGridTemplateColumn.CellTemplate>
                <DataTemplate>
                    <Border Style="{StaticResource StatusBadge}"
                            Background="#E8F5E9">
                        <TextBlock Text="Active" 
                                   Foreground="#4CAF50"
                                   FontWeight="SemiBold"
                                   FontSize="11"/>
                    </Border>
                </DataTemplate>
            </DataGridTemplateColumn.CellTemplate>
        </DataGridTemplateColumn>
    </DataGrid.Columns>
</DataGrid>
```

## Single-Click Row Selection

Để enable chọn hàng bằng 1 click (không cần click vào checkbox), thêm event handler:

### XAML
```xml
<DataGrid MouseLeftButtonUp="DataGrid_MouseLeftButtonUp">
    <!-- ... -->
</DataGrid>
```

### Code-behind
```csharp
private void DataGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
{
    var dataGrid = sender as DataGrid;
    if (dataGrid == null) return;

    var dep = (DependencyObject)e.OriginalSource;

    while (dep != null && dep != dataGrid)
    {
        // Don't toggle if clicking on checkbox directly
        if (dep is CheckBox)
            return;

        if (dep is DataGridCell cell)
        {
            var row = DataGridRow.GetRowContainingElement(cell);
            if (row != null && row.Item is YourItemViewModel item)
            {
                // Toggle selection
                item.IsSelected = !item.IsSelected;
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
```

## Color Palette

### Status Colors
- **Active/Success**: `#E8F5E9` (background), `#4CAF50` (text)
- **Inactive/Disabled**: `#F5F5F5` (background), `#9E9E9E` (text)
- **Warning**: `#FFF3E0` (background), `#FF9800` (text)
- **Error**: `#FFEBEE` (background), `#F44336` (text)
- **Info**: `#E3F2FD` (background), `#2196F3` (text)

### Row Colors
- **Normal**: `White`
- **Alternate**: `#FAFAFA`
- **Hover**: `#F5F5F5`
- **Selected**: `#E3F2FD`
- **Selected + Hover**: `#BBDEFB`

### Border Colors
- **Light**: `#F0F0F0`
- **Medium**: `#E0E0E0`
- **Dark**: `#BDBDBD`

## Best Practices

1. **Always use all 4 styles together** cho consistency:
   - ModernDataGrid
   - ModernDataGridRow
   - ModernDataGridCell
   - ModernDataGridColumnHeader

2. **Row Height**: Default là 52px, có thể customize nếu cần

3. **Column Width**: 
   - Checkbox column: 60px
   - Status column: 110px
   - Code/ID columns: 100-120px
   - Text columns: Use `*` for flexible width

4. **Font Sizes**:
   - Header: 12px
   - Body: 13px
   - Status badge: 11px

5. **Spacing**:
   - Cell padding: 12px horizontal
   - Badge padding: 10px horizontal, 4px vertical

## Migration Guide

Để migrate DataGrid cũ sang style mới:

### Before
```xml
<DataGrid ItemsSource="{Binding Items}"
          AutoGenerateColumns="False"
          CanUserAddRows="False"
          GridLinesVisibility="Horizontal"
          RowBackground="White"
          AlternatingRowBackground="#F9F9F9">
    <!-- ... -->
</DataGrid>
```

### After
```xml
<DataGrid ItemsSource="{Binding Items}"
          Style="{StaticResource ModernDataGrid}"
          RowStyle="{StaticResource ModernDataGridRow}"
          CellStyle="{StaticResource ModernDataGridCell}"
          ColumnHeaderStyle="{StaticResource ModernDataGridColumnHeader}">
    <!-- ... -->
</DataGrid>
```

## Examples in Project

Xem `Controls/VisualSelectTabControl.xaml` để có ví dụ đầy đủ về cách sử dụng custom DataGrid.
