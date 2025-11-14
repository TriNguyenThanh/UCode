# UCode - Online Judge Platform For UTC2 Student

A comprehensive Online Judge platform for programming education, featuring microservices architecture with ASP.NET Core backend, React frontend, and automated code execution using Isolate sandbox.

## Overview

UCode is a complete online learning platform designed for educational institutions, enabling:

- **Students:** Submit code solutions, track progress, practice problems
- **Teachers:** Create problems, manage classes, assign homework, grade submissions
- **Admins:** Manage users, monitor system health, configure platform settings

## Architecture

### Microservices Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                        Client Layer                         │
├──────────────────────┬──────────────────────────────────────┤
│   React Web App      │    Desktop App (WinForms/WPF)        │
│   (Port: 5173)       │                                      │
└──────────────────────┴──────────────────────────────────────┘
                              │
                              ↓
┌─────────────────────────────────────────────────────────────┐
│                     API Gateway (Ocelot)                    │
│                        Port: 5000                           │
└─────────────────────────────────────────────────────────────┘
                              │
        ┌─────────────────────┼─────────────────────┐
        ↓                     ↓                     ↓
┌───────────────┐    ┌───────────────┐    ┌───────────────┐
│ User Service  │    │Assignment Svc │    │  File Service │
│  Port: 5001   │    │  Port: 5002   │    │  Port: 5073   │
│               │    │               │    │               │
│ - Auth/JWT    │    │ - Problems    │    │ - AWS S3      │
│ - Users       │    │ - Assignments │    │ - File Upload │
│ - Classes     │    │ - Submissions │    │ - Categories  │
└───────────────┘    └───────────────┘    └───────────────┘
                              │
                              ↓
                     ┌────────────────┐
                     │ Judge Service  │
                     │  Port: 5003    │
                     │                │
                     │ - RabbitMQ     │
                     │ - Isolate      │
                     │ - Code Exec    │
                     └────────────────┘
```

## Features

### For Students
- ✅ Browse and solve programming problems
- ✅ Submit code in multiple languages (Python, C++, Java, C#)
- ✅ View submission history and results
- ✅ Track progress and rankings
- ✅ Join classes and complete assignments

### For Teachers
- ✅ Create and manage problems with test cases
- ✅ Create assignments with deadlines
- ✅ Manage classes and students
- ✅ View student submissions and grades
- ✅ Configure language-specific settings

### For Admins
- ✅ User management (students, teachers)
- ✅ System monitoring and health checks
- ✅ Global configuration management

## Technology Stack

### Backend
- **Framework:** ASP.NET Core 8.0
- **Architecture:** Clean Architecture + Microservices
- **Database:** SQL Server with Entity Framework Core
- **Authentication:** JWT Bearer Tokens
- **Message Queue:** RabbitMQ
- **Caching:** Redis (planned)
- **File Storage:** AWS S3
- **Code Execution:** Isolate Sandbox (Linux)
- **API Gateway:** Ocelot

### Frontend
- **Framework:** React 18 + TypeScript
- **Router:** React Router v7
- **UI Library:** Material-UI (MUI)
- **Build Tool:** Vite
- **Styling:** Tailwind CSS + MUI

### Desktop
- **Framework:** WPF (.NET 8)
- **UI:** Modern WPF with Material Design

### DevOps
- **Containerization:** Docker + Docker Compose
- **CI/CD:** GitHub Actions (planned)
- **Monitoring:** Health Checks + Logging

## Services

| Service | Port | Description | Database |
|---------|------|-------------|----------|
| `api-gateway` | 5000 | Ocelot API Gateway | - |
| `user-service` | 5001 | Authentication, Users, Classes | SQL Server |
| `assignment-service` | 5002 | Problems, Assignments, Submissions | SQL Server |
| `file-service` | 5073 | File uploads to AWS S3 | - |
| `judge-service` | 5003 | Code execution with Isolate | - |
| `client` | 5173 | React web application | - |

## Quick Start with Docker

### Prerequisites
- Docker Desktop installed
- Docker Compose V2
- 8GB+ RAM recommended
- AWS credentials for file-service (optional)

### 1. Clone Repository
```bash
git clone https://github.com/TriNguyenThanh/UCode.git
cd UCode
```

### 2. Configure Environment Variables

Create `.env` files for services that need configuration:

**SQL Server (already in docker-compose.dev.yml):**
```env
SA_PASSWORD=YourStrong@Passw0rd
ACCEPT_EULA=Y
```

**User Service & Assignment Service:**
- Connection strings are configured in `appsettings.Development.json`
- Adjust if needed for your environment

**File Service (Optional AWS S3):**
```bash
# Edit backend/src/file-service/appsettings.Development.json
# Add your AWS credentials if using S3
```

### 3. Start All Services

**Option 1: Start everything at once**
```bash
docker-compose -f docker-compose.dev.yml up -d
```

**Option 2: Start services step by step**
```bash
# Start infrastructure first (SQL Server, RabbitMQ)
docker-compose -f docker-compose.dev.yml up -d sqlserver rabbitmq

# Wait for SQL Server to be ready (about 30 seconds)
Start-Sleep -Seconds 30

# Start backend services
docker-compose -f docker-compose.dev.yml up -d api-gateway user-service assignment-service file-service judge-service

# Start frontend
docker-compose -f docker-compose.dev.yml up -d client
```

### 4. Verify Services are Running

```bash
# Check all containers
docker-compose -f docker-compose.dev.yml ps

# Check logs
docker-compose -f docker-compose.dev.yml logs -f
```

### 5. Access Services

| Service | URL | Credentials |
|---------|-----|-------------|
| **Client (Web)** | http://localhost:5173 | See default accounts below |
| **API Gateway** | http://localhost:5000 | - |
| **User Service API** | http://localhost:5001/swagger | - |
| **Assignment Service API** | http://localhost:5002/swagger | - |
| **File Service API** | http://localhost:5073/swagger | - |
| **RabbitMQ Management** | http://localhost:15672 | guest/guest |
| **SQL Server** | localhost:1433 | sa/YourStrong@Passw0rd |

### 6. Default Accounts

After database migration and seeding:

```
Admin:    admin@ucode.edu.vn / Admin@123
Teacher:  teacher@ucode.edu.vn / Teacher@123
Student:  student@ucode.edu.vn / Student@123
```

### 7. Stop Services

```bash
# Stop all services
docker-compose -f docker-compose.dev.yml down

# Stop and remove volumes (WARNING: deletes all data)
docker-compose -f docker-compose.dev.yml down -v
```

## Development Setup

### Running Services Locally (Without Docker)

**Prerequisites:**
- .NET 8 SDK
- Node.js 20+
- SQL Server
- RabbitMQ
- Python 3.11+ (for judge-service)

**1. Start SQL Server and RabbitMQ**
```bash
# Using Docker for infrastructure only
docker-compose -f docker-compose.dev.yml up -d sqlserver rabbitmq
```

**2. Backend Services**

```bash
# API Gateway
cd backend/src/api-gateway
dotnet restore
dotnet run

# User Service
cd backend/src/user-service/Api
dotnet restore
dotnet run

# Assignment Service
cd backend/src/assignment-service/Api
dotnet restore
dotnet run

# File Service
cd backend/src/file-service
dotnet restore
dotnet run
```

**3. Judge Service (Python)**
```bash
cd backend/src/judge-service
python -m venv venv
.\venv\Scripts\Activate.ps1  # Windows PowerShell
pip install -r requirements.txt
python app/main.py
```

**4. Frontend**
```bash
cd client
npm install
npm run dev
```

**5. Desktop App**
```bash
# Open in Visual Studio
cd desktop-client
# Open UCode.Desktop.sln
# Build and Run (F5)
```

## Testing

### Using Swagger UI
1. Navigate to service Swagger endpoints
2. Click "Authorize" and enter JWT token
3. Test endpoints interactively

### Get JWT Token
```bash
# Login via User Service
POST http://localhost:5001/api/auth/login
{
  "email": "student@ucode.edu.vn",
  "password": "Student@123"
}
```

### Running Unit Tests
```bash
# User Service tests
cd backend/src/user-service
dotnet test

# Assignment Service tests
cd backend/src/assignment-service
dotnet test
```

### Integration Tests
```bash
# Start services first
docker-compose -f docker-compose.dev.yml up -d

# Run tests
cd backend/test
dotnet test
```

## Database Management

### Migrations

**User Service:**
```bash
cd backend/src/user-service/Infrastructure
dotnet ef migrations add MigrationName --startup-project ../Api
dotnet ef database update --startup-project ../Api
```

**Assignment Service:**
```bash
cd backend/src/assignment-service/Infrastructure
dotnet ef migrations add MigrationName --startup-project ../Api
dotnet ef database update --startup-project ../Api
```

### Seed Data
- Migrations include seed data for default users and roles
- Run on first startup automatically

### Backup & Restore
```bash
# Backup database
docker exec sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourStrong@Passw0rd -Q "BACKUP DATABASE UCodeDB TO DISK='/var/opt/mssql/backup/ucode.bak'"

# Restore database
docker exec sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourStrong@Passw0rd -Q "RESTORE DATABASE UCodeDB FROM DISK='/var/opt/mssql/backup/ucode.bak'"
```

## Security

- **Authentication:** JWT Bearer tokens with configurable expiration
- **Authorization:** Role-based access control (Student, Teacher, Admin)
- **File Upload:** Size limits, MIME type validation, magic bytes verification
- **Code Execution:** Isolated sandbox environment with resource limits
- **CORS:** Configured allowed origins
- **Secrets:** Use environment variables, never commit credentials
- **SQL Injection:** Protected via EF Core parameterized queries
- **XSS Protection:** Input sanitization and output encoding

## Troubleshooting

### SQL Server Connection Issues
```bash
# Check SQL Server is running
docker ps | findstr sqlserver

# Check logs
docker logs sqlserver

# Test connection
docker exec sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourStrong@Passw0rd -Q "SELECT @@VERSION"
```

### RabbitMQ Connection Failed
```bash
# Check RabbitMQ is running
docker ps | findstr rabbitmq

# Check logs
docker logs rabbitmq

# Access management UI
# http://localhost:15672 (guest/guest)
```

### Judge Service - Isolate Not Found
```bash
# Rebuild judge service image
docker-compose -f docker-compose.dev.yml build judge-service

# Verify Isolate installation
docker exec judge-service isolate --version
```

### Port Already in Use
```bash
# Find process using port (e.g., 5173)
netstat -ano | findstr :5173

# Kill process by PID
taskkill /PID <PID> /F
```

### File Service - AWS Access Denied
- Verify AWS credentials in `appsettings.json`
- Check IAM permissions for S3 access
- See `backend/src/file-service/SETUP.md`

### Frontend Not Loading
```bash
# Clear npm cache
cd client
npm cache clean --force
Remove-Item -Recurse -Force node_modules
npm install

# Rebuild
npm run build
```

## Configuration

### Environment Variables

**User Service:**
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost,1433;Database=UCodeUserDB;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True"
},
"Jwt": {
  "Key": "your-secret-key-min-32-characters",
  "Issuer": "UCode",
  "Audience": "UCodeUsers",
  "ExpiryInMinutes": 60
}
```

**Assignment Service:**
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost,1433;Database=UCodeAssignmentDB;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True"
},
"RabbitMQ": {
  "HostName": "localhost",
  "Port": 5672,
  "UserName": "guest",
  "Password": "guest"
}
```

**Judge Service:**
```python
RABBITMQ_HOST = "localhost"
DEFAULT_TIME_LIMIT = 1  # seconds
DEFAULT_MEMORY_LIMIT = 256  # MB
MAX_CONCURRENT_SUBMISSIONS = 5
```

**File Service:**
```json
"AWS": {
  "Region": "ap-southeast-1",
  "BucketName": "ucode-files",
  "AccessKey": "your-access-key",
  "SecretKey": "your-secret-key"
}
```

## Production Deployment

### Docker Compose Production
```bash
# Build production images
docker-compose -f docker-compose.prod.yml build

# Start production services
docker-compose -f docker-compose.prod.yml up -d

# Check health
docker-compose -f docker-compose.prod.yml ps
```

### Production Checklist
- [ ] Use strong passwords and secrets
- [ ] Configure HTTPS/TLS certificates
- [ ] Set up reverse proxy (nginx/traefik)
- [ ] Enable rate limiting
- [ ] Configure CORS properly
- [ ] Use managed databases (Azure SQL, AWS RDS)
- [ ] Set up monitoring and alerting
- [ ] Configure auto-scaling
- [ ] Implement backup strategy
- [ ] Use managed message queue
- [ ] Enable CDN for static assets

### Kubernetes (Planned)
- Helm charts for service deployment
- Auto-scaling configurations
- Persistent volumes for databases
- Ingress configuration

## Documentation

- [User Service API](backend/src/user-service/README.md)
- [Assignment Service API](backend/src/assignment-service/README.md)
- [File Service Setup](backend/src/file-service/SETUP.md)
- [File Types Support](backend/src/file-service/DOCUMENT_TYPES.md)
- [Judge Service Architecture](backend/src/judge-service/README.md)
- [System Design](docs/system-design/)
- [API Documentation](docs/Api/)

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

### Coding Standards
- Follow Clean Architecture principles
- Use DTOs for API contracts
- Write unit tests for business logic
- Document complex algorithms
- Use meaningful variable names
- Follow C# naming conventions
- Use async/await for I/O operations

### Branch Strategy
- `main` - Production ready code
- `develop` - Development branch
- `feature/*` - Feature branches
- `bugfix/*` - Bug fix branches
- `hotfix/*` - Production hotfixes

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Team

- **Backend Team:** ASP.NET Core microservices
- **Frontend Team:** React web application
- **DevOps Team:** Docker, CI/CD, Infrastructure
- **Desktop Team:** C# WPF desktop application

## Support

For issues and questions:
- Create an issue on [GitHub](https://github.com/TriNguyenThanh/UCode/issues)
- Email: support@ucode.edu.vn
- Documentation: [docs/](docs/)

## Roadmap

### Phase 1 (Current)
- [x] User authentication and authorization
- [x] Problem and assignment management
- [x] Code submission and execution
- [x] File upload to AWS S3
- [x] Docker development environment

### Phase 2 (In Progress)
- [ ] API Gateway with rate limiting
- [ ] Redis caching layer
- [ ] Real-time notifications (SignalR)
- [ ] Enhanced desktop application

### Phase 3 (Planned)
- [ ] Code review and comments
- [ ] Contest system with leaderboards
- [ ] Advanced analytics and reporting
- [ ] Mobile applications (React Native)
- [ ] CI/CD pipeline automation
- [ ] Kubernetes deployment
- [ ] Multi-language support (i18n)

---

**Built with for educational excellence**