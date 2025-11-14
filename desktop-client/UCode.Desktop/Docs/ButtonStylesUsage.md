# Button Styles Usage Guide

## Overview
Dự án có một bộ button styles đẹp và hiện đại, dễ dàng tái sử dụng cho toàn bộ ứng dụng.

## Available Styles

### 1. PrimaryButton (Blue)
Button chính cho các action quan trọng.

```xml
<Button Content="Lưu" 
        Command="{Binding SaveCommand}"
        Style="{StaticResource PrimaryButton}"/>
```

**Colors:**
- Normal: `#2196F3` (Blue)
- Hover: `#1976D2` (Dark Blue)
- Disabled: `#E0E0E0` (Gray)

### 2. SuccessButton (Green)
Button cho các action thành công hoặc xác nhận.

```xml
<Button Content="Xác nhận" 
        Command="{Binding ConfirmCommand}"
        Style="{StaticResource SuccessButton}"/>
```

**Colors:**
- Normal: `#4CAF50` (Green)
- Hover: `#388E3C` (Dark Green)

### 3. WarningButton (Orange)
Button cho các action cần cảnh báo.

```xml
<Button Content="Cảnh báo" 
        Command="{Binding WarnCommand}"
        Style="{StaticResource WarningButton}"/>
```

**Colors:**
- Normal: `#FF9800` (Orange)
- Hover: `#F57C00` (Dark Orange)

### 4. DangerButton (Red)
Button cho các action nguy hiểm (xóa, hủy).

```xml
<Button Content="Xóa" 
        Command="{Binding DeleteCommand}"
        Style="{StaticResource DangerButton}"/>
```

**Colors:**
- Normal: `#F44336` (Red)
- Hover: `#D32F2F` (Dark Red)

### 5. SecondaryButton (Outlined)
Button phụ với border.

```xml
<Button Content="Hủy" 
        Command="{Binding CancelCommand}"
        Style="{StaticResource SecondaryButton}"/>
```

**Colors:**
- Normal: White background, Blue border
- Hover: `#E3F2FD` (Light Blue)

### 6. GhostButton (Transparent)
Button trong suốt, minimal.

```xml
<Button Content="Bỏ qua" 
        Command="{Binding SkipCommand}"
        Style="{StaticResource GhostButton}"/>
```

**Colors:**
- Normal: Transparent
- Hover: `#F5F5F5` (Light Gray)

### 7. IconButton (Square)
Button nhỏ cho icons.

```xml
<Button Content="✕" 
        Command="{Binding CloseCommand}"
        Style="{StaticResource IconButton}"/>
```

**Size:** 36x36px

### 8. PaginationButton
Button cho pagination (kế thừa từ IconButton).

```xml
<Button Content="❮" 
        Command="{Binding PreviousPageCommand}"
        Style="{StaticResource PaginationButton}"/>
```

### 9. LinkButton (Text only)
Button dạng link, không có background.

```xml
<Button Content="Xem thêm" 
        Command="{Binding ViewMoreCommand}"
        Style="{StaticResource LinkButton}"/>
```

**Features:**
- Underline on hover
- Blue color
- No background

## Customization

### Override Properties
Bạn có thể override các properties:

```xml
<Button Content="Custom" 
        Style="{StaticResource PrimaryButton}"
        Width="200"
        Height="50"
        FontSize="16"
        Padding="30,15"/>
```

### Custom Colors
Để tạo button với màu khác:

```xml
<Button Content="Custom Color" 
        Style="{StaticResource ModernButton}"
        Background="#9C27B0"
        Foreground="White"/>
```

## Common Patterns

### Button Group
```xml
<StackPanel Orientation="Horizontal" HorizontalAlignment="Right">
    <Button Content="Hủy" 
            Style="{StaticResource SecondaryButton}"
            Margin="0,0,10,0"/>
    <Button Content="Lưu" 
            Style="{StaticResource PrimaryButton}"/>
</StackPanel>
```

### Icon + Text Button
```xml
<Button Style="{StaticResource PrimaryButton}">
    <StackPanel Orientation="Horizontal">
        <TextBlock Text="✓" FontSize="16" Margin="0,0,8,0"/>
        <TextBlock Text="Xác nhận"/>
    </StackPanel>
</Button>
```

### Loading State
```xml
<Button Content="{Binding ButtonText}" 
        Style="{StaticResource PrimaryButton}"
        IsEnabled="{Binding IsNotLoading}"/>

<!-- In ViewModel -->
public string ButtonText => IsLoading ? "Đang xử lý..." : "Lưu";
```

### Pagination Example
```xml
<StackPanel Orientation="Horizontal">
    <Button Content="❮" 
            Command="{Binding PreviousCommand}"
            Style="{StaticResource PaginationButton}"
            Margin="0,0,5,0"/>
    <TextBlock Text="1-10 trong 100" 
               VerticalAlignment="Center"
               Margin="10,0"/>
    <Button Content="❯" 
            Command="{Binding NextCommand}"
            Style="{StaticResource PaginationButton}"
            Margin="5,0,0,0"/>
</StackPanel>
```

## Best Practices

1. **Use semantic styles**: 
   - Primary cho action chính
   - Danger cho xóa/hủy
   - Success cho xác nhận
   - Secondary cho action phụ

2. **Consistent sizing**:
   - Default height: 40px
   - Icon buttons: 36x36px
   - Padding: 20px horizontal, 10px vertical

3. **Button placement**:
   - Primary button ở bên phải
   - Cancel/Secondary button ở bên trái
   - Spacing: 10-15px giữa các buttons

4. **Disabled state**:
   - Luôn bind `IsEnabled` với ViewModel
   - Không cần custom disabled style

5. **Loading state**:
   - Disable button khi đang xử lý
   - Thay đổi text để hiển thị trạng thái

## Color Palette

### Primary Colors
- **Blue**: `#2196F3` (Primary)
- **Green**: `#4CAF50` (Success)
- **Orange**: `#FF9800` (Warning)
- **Red**: `#F44336` (Danger)

### Hover Colors
- **Blue**: `#1976D2`
- **Green**: `#388E3C`
- **Orange**: `#F57C00`
- **Red**: `#D32F2F`

### Disabled
- **Background**: `#E0E0E0`
- **Foreground**: `#9E9E9E`

## Migration Guide

### Before
```xml
<Button Content="Lưu"
        Background="#2196F3"
        Foreground="White"
        BorderThickness="0"
        FontSize="13"
        FontWeight="SemiBold"
        Padding="20,10"
        Height="40"
        Cursor="Hand">
    <Button.Template>
        <!-- Long template code -->
    </Button.Template>
</Button>
```

### After
```xml
<Button Content="Lưu"
        Style="{StaticResource PrimaryButton}"/>
```

**Result**: 90% less code! 🎉

## Examples in Project

- `Controls/VisualSelectTabControl.xaml` - Pagination và Primary buttons
- Xem các examples khác trong project

## Tips

1. **Reuse, don't recreate**: Luôn dùng styles có sẵn
2. **Consistent**: Dùng cùng style cho cùng loại action
3. **Accessible**: Tất cả buttons đều có focus state
4. **Responsive**: Buttons tự động adapt với content
