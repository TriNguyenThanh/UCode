# Code Submission System

A comprehensive Code Submission System featuring:

- **Backend:** ASP.NET Web API
- **Frontend Web:** React
- **Desktop App:** C# UI (WinForms/WPF)

## Features

- User registration and authentication
- Code submission (web & desktop)
- Submission status tracking
- Admin panel for reviewing submissions
- RESTful API for integration

## Project Structure

- **/api** - ASP.NET Web API backend
- **/web** - React frontend
- **/desktop** - C# desktop application

## Project Architechture

- **Entity Framework Core** for database interactions
- **JWT Authentication** for secure user sessions
- **Microservice** for handling code compilation
- **Clean architechture** for maintainability
- **Redis** for caching
- **RabbitMQ** for message queuing

## Microservice

- **UserService:** `http://localhost:5000`
- **ProblemService:** `http://localhost:5001`
- **SubmissionService:** `http://localhost:5002`
- **ExecuteService:** `http://localhost:5003`

# Backend (ASP.NET Web API)

1. Navigate to `/api`
2. Restore NuGet packages
3. Update `appsettings.json` with your DB connection string
4. Run the project

# Frontend Web (React)
1. Navigate to `/web`
2. Run `npm install`
3. Run `npm start`

# Desktop App (C# UI)

1. Open `/desktop` in Visual Studio
2. Restore NuGet packages
3. Build and run the project

# How to Run This Project

1. **Clone the repository:**
   ```sh
   git clone <repository-url>
   cd ucode.io.vn
   ```

2. **Start the Backend API:**
   - Open a terminal and navigate to the `/api` folder.
   - Restore dependencies and run the API:
     ```sh
     dotnet restore
     dotnet run
     ```
   - Ensure your database is set up and the connection string in `appsettings.json` is correct.

3. **Start the Frontend Web App:**
   - Open a new terminal and navigate to the `/web` folder.
   - Install dependencies and start the development server:
     ```sh
     npm install
     npm start
     ```
   - The app will be available at `https://localhost:5005` by default.

4. **Run the Desktop App:**
   - Open the `/desktop` folder in Visual Studio.
   - Restore NuGet packages if prompted.
   - Build and run the project from Visual Studio.

5. **Access the API Documentation:**
   - After starting the backend, open your browser and go to `http://localhost:<api-port>/swagger` to view the Swagger UI.

## Contributing

1. Fork the repository
2. Create a new branch
3. Commit your changes
4. Open a pull request

## License

This project is licensed under the MIT License.