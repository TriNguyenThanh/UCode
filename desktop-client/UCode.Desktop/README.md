# UCode Desktop Client

WPF Desktop Application for UCode Online Judge Platform

## 🎯 Features

- ✅ **Modern UI** with Material Design
- ✅ **MVVM Architecture** with Dependency Injection
- ✅ **JWT Authentication** with Backend API
- ✅ **Login System** with beautiful Material Design UI
- ✅ **Main Dashboard** with navigation drawer
- ⏳ **Code Editor** with AvalonEdit (syntax highlighting)
- ⏳ **Assignment Management**
- ⏳ **Problem Solving**
- ⏳ **Submission Tracking**

## 🏗️ Architecture

```
UCode.Desktop/
├── Models/              # Data models (User, Class, Problem, Assignment)
├── ViewModels/          # MVVM ViewModels with INotifyPropertyChanged
├── Views/               # XAML Views (Login, Main, Home, etc.)
├── Services/            # API Service, Auth Service
├── Helpers/             # RelayCommand, Converters
└── Resources/           # Images, Styles, Templates
```

## 📦 Tech Stack

- **.NET 8.0** - Latest framework
- **WPF** - Windows Presentation Foundation
- **Material Design** - Modern Google Material Design UI
- **MVVM Pattern** - Clean separation of concerns
- **Dependency Injection** - Microsoft.Extensions.DependencyInjection
- **AvalonEdit** - Code editor with syntax highlighting
- **HttpClient** - REST API communication
- **Newtonsoft.Json** - JSON serialization

## 🚀 Getting Started

### Prerequisites

- .NET 8.0 SDK or later
- Windows 10/11
- UCode Backend running on `http://localhost:5000`

### Installation

```bash
cd desktop-client/UCode.Desktop
dotnet restore
dotnet build
```

### Run

```bash
dotnet run
```

Or open in Visual Studio and press F5

## 🔐 Authentication

The app uses JWT Bearer token authentication:

1. Login with credentials (student01/123, teacher01/123, admin/123)
2. Receive `accessToken` and `refreshToken` from API
3. Store token and add to `Authorization: Bearer {token}` header
4. Use token for all subsequent API calls

## 📡 API Integration

### Base URL

```
http://localhost:5000
```

### Endpoints Used

- `POST /api/v1/auth/login` - Login
- `GET /api/v1/classes` - Get classes
- `GET /api/v1/assignments` - Get assignments
- `GET /api/v1/problems` - Get problems
- `POST /api/v1/submissions` - Submit code

## 🎨 UI/UX

 Đối với WPF:
 
dùng Thêm các thuộc tính rendering:
UseLayoutRounding="True"
SnapsToDevicePixels="True"
TextOptions.TextFormattingMode="Display"
TextOptions.TextRenderingMode="ClearType"

để chữ và giao diện không bị mờ như winxp

### Login Window

- Material Design themed
- Rounded corners with shadow
- Username/Email input
- Password input
- Remember me checkbox
- Loading indicator
- Error messages

### Main Window

- Top navigation bar with logo
- User profile menu
- Left navigation drawer
- Content area
- Material icons
- Responsive layout

## 🧪 Testing

Test with these credentials:

| Role    | Username  | Password |
| ------- | --------- | -------- |
| Admin   | admin     | 123      |
| Teacher | teacher01 | 123      |
| Student | student01 | 123      |

## 📝 Next Steps

- [ ] Implement Home Page with classes and assignments
- [ ] Create Classes Page with class details
- [ ] Build Assignment Page with problem list
- [ ] Integrate Code Editor (AvalonEdit) for problem solving
- [ ] Add Submission History page
- [ ] Implement real-time judge feedback
- [ ] Add offline capability
- [ ] Create local database cache (SQLite)

## 🤝 Contributing

This is part of UCode platform with multi-client architecture:

```
Backend API (ASP.NET Core)
    ↓
    ├── Web Client (React + Vite)
    └── Desktop Client (WPF) ← You are here
```

## 📄 License

Part of UCode Online Judge Platform


