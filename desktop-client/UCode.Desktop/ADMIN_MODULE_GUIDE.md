# Admin Module Implementation Guide

## 📁 Cấu trúc Files đã tạo

### Models (`Models/Admin/`)
- ✅ `AdminDashboardStats.cs` - Dashboard statistics model
- ✅ `UserManagement.cs` - User management models & requests
- ✅ `ClassManagement.cs` - Class management models & requests
- ✅ `ProblemManagement.cs` - Problem management models & requests
- ✅ `SystemSettings.cs` - System settings configuration model

### Services (`Services/`)
- ✅ `AdminService.cs` - Main admin service với full CRUD operations

### ViewModels (`ViewModels/Admin/`)
- ✅ `AdminHomeViewModel.cs` - Dashboard ViewModel
- ✅ `AdminUsersViewModel.cs` - User management ViewModel
- ✅ `AdminClassesViewModel.cs` - Class management ViewModel
- ✅ `AdminProblemsViewModel.cs` - Problem management ViewModel
- ✅ `AdminSettingsViewModel.cs` - Settings ViewModel

### Pages (`Pages/Admin/`)
- ✅ `AdminHomePage.xaml/.cs` - Dashboard page với statistics cards
- ✅ `AdminUsersPage.xaml/.cs` - User management page với DataGrid
- ✅ `AdminClassesPage.xaml/.cs` - Class management page
- ✅ `AdminProblemsPage.xaml/.cs` - Problem management page
- ✅ `AdminSettingsPage.xaml/.cs` - Settings page (UI only)

### Windows (`Views/Windows/`)
- ✅ `AdminHomeWindow.xaml/.cs` - Main admin window với sidebar navigation

---

## 🚀 Cách sử dụng

### 1. Khởi chạy Admin Window

```csharp
// Trong App.xaml.cs hoặc sau khi login thành công
var apiService = new ApiService(new HttpClient());
var adminService = new AdminService(apiService);

var adminWindow = new AdminHomeWindow(adminService);
adminWindow.Show();
```

### 2. Dependency Injection Setup (Recommended)

```csharp
// Trong App.xaml.cs
public partial class App : Application
{
    private ServiceProvider _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();
        
        // Register services
        services.AddHttpClient<ApiService>();
        services.AddSingleton<AdminService>();
        services.AddTransient<AdminHomeWindow>();
        
        _serviceProvider = services.BuildServiceProvider();
        
        // Show admin window
        var adminWindow = _serviceProvider.GetService<AdminHomeWindow>();
        adminWindow.Show();
    }
}
```

### 3. Backend API Endpoints cần implement

Admin Module cần các API endpoints sau từ backend:

#### Dashboard
```
GET /api/v1/admin/dashboard/stats
```

#### User Management
```
GET    /api/v1/admin/users?page=1&pageSize=20&searchTerm=&role=&isActive=
GET    /api/v1/admin/users/{userId}
POST   /api/v1/admin/users
PUT    /api/v1/admin/users/{userId}
DELETE /api/v1/admin/users/{userId}
POST   /api/v1/admin/users/{userId}/reset-password
POST   /api/v1/admin/users/bulk-action
GET    /api/v1/admin/users/export/excel
```

#### Class Management
```
GET    /api/v1/admin/classes?page=1&pageSize=20&searchTerm=&teacherId=&isActive=
GET    /api/v1/admin/classes/{classId}
GET    /api/v1/admin/classes/{classId}/statistics
POST   /api/v1/admin/classes
PUT    /api/v1/admin/classes/{classId}
DELETE /api/v1/admin/classes/{classId}
PUT    /api/v1/admin/classes/{classId}/teacher
GET    /api/v1/admin/classes/export/excel
```

#### Problem Management
```
GET    /api/v1/admin/problems?page=1&pageSize=20&searchTerm=&difficulty=&status=&visibility=
POST   /api/v1/admin/problems/approve
POST   /api/v1/admin/problems/bulk-action
DELETE /api/v1/admin/problems/{problemId}
GET    /api/v1/admin/problems/export/excel
```

#### Settings
```
GET /api/v1/admin/settings
PUT /api/v1/admin/settings
```

---

## 🎨 UI Features

### Dashboard
- 4 statistics cards (Users, Classes, Problems, Submissions)
- Growth indicators (+N this week)
- User activity metrics
- Quick action buttons
- Clickable cards for navigation

### User Management
- Search by name/email
- Filter by role (Admin/Teacher/Student)
- Filter by status (Active/Inactive)
- Pagination support
- Actions: Edit, Reset Password, Delete
- Export to Excel

### Class Management
- Search by class name
- Filter by teacher
- Filter by status
- View class statistics
- Reassign teacher
- Export to Excel

### Problem Management
- Search by title
- Filter by difficulty (Easy/Medium/Hard)
- Filter by status (Draft/Pending/Approved/Rejected)
- Filter by visibility (Public/Private/ClassOnly)
- Actions: Approve, Reject, Change Visibility, Delete
- Export to Excel

### Settings (UI Only - No Functionality Yet)
- Email Configuration
- Security Settings
- Judge Configuration
- System Configuration
- Assignment Settings

---

## ⚠️ TODO / Chưa implement

### Dialogs cần tạo thêm:
1. **UserCreateDialog** - Tạo user mới
2. **UserEditDialog** - Chỉnh sửa user
3. **PasswordResetDialog** - Reset password
4. **ClassCreateDialog** - Tạo class mới
5. **ClassEditDialog** - Chỉnh sửa class
6. **TeacherSelectionDialog** - Chọn teacher để reassign
7. **ProblemReviewDialog** - Review & reject problem với notes

### Features cần hoàn thiện:
1. **Bulk Actions**
   - Bulk activate/deactivate users
   - Bulk delete
   - Bulk change problem visibility

2. **Export Functions**
   - Save file dialog
   - Excel/CSV generation
   - Download handling

3. **Settings Functionality**
   - Save settings to backend
   - Test email configuration
   - Validation

4. **Real-time Updates** (Optional)
   - SignalR integration
   - Live notifications
   - Auto-refresh statistics

5. **Charts & Visualizations** (Optional)
   - User growth chart (Line)
   - Submission rate chart (Bar)
   - Active users chart (Pie)
   - Requires: LiveCharts.Wpf package

---

## 🔧 Troubleshooting

### Issue: "Type not found" errors
**Solution:** Rebuild project (`Ctrl+Shift+B`)

### Issue: Navigation not working
**Solution:** Check NavigationService is properly initialized in AdminHomeWindow

### Issue: API calls failing
**Solution:** 
1. Ensure backend is running on `http://localhost:5000`
2. Check JWT token is set: `apiService.SetAccessToken(token)`
3. Verify API endpoints exist in backend

### Issue: Material Design icons not showing
**Solution:** Add to App.xaml:
```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <materialDesign:BundledTheme BaseTheme="Light" 
                                        PrimaryColor="Blue" 
                                        SecondaryColor="Amber"/>
            <ResourceDictionary Source="pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Defaults.xaml"/>
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

---

## 📦 Required NuGet Packages

Tất cả packages đã có trong project:
- ✅ MahApps.Metro (2.4.10)
- ✅ MaterialDesignThemes (5.3.0)
- ✅ MaterialDesignColors (5.3.0)
- ✅ Newtonsoft.Json (13.0.4)
- ✅ ClosedXML (0.102.1) - for Excel export

Optional (nếu cần charts):
- ❌ LiveChartsCore.SkiaSharpView.WPF (2.0.0-rc2)

---

## 🎯 Next Steps

1. **Test UI** - Run application và kiểm tra navigation
2. **Backend Integration** - Implement các API endpoints cần thiết
3. **Create Dialogs** - Tạo các dialog forms cho CRUD operations
4. **Implement Export** - Hoàn thiện Excel export functionality
5. **Add Validation** - Input validation cho forms
6. **Error Handling** - Improve error messages
7. **Add Charts** (Optional) - Dashboard visualizations
8. **Testing** - Unit tests cho ViewModels

---

## 📝 Notes

- Settings page CHỈ có UI, chưa có functionality (theo yêu cầu)
- System Monitoring và Audit Logs KHÔNG implement (theo yêu cầu)
- Tất cả CRUD operations đã sẵn sàng, chỉ cần backend APIs
- UI đã responsive và có loading indicators
- Pagination đã được implement
- Material Design theme nhất quán với Teacher module

---

**Created by:** GitHub Copilot
**Date:** November 16, 2025
**Status:** ✅ Ready for testing
