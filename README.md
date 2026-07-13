# Employee Management System

A comprehensive Employee Management System built with ASP.NET Core 8, Entity Framework Core, and React. This application enables administrators to manage employee records, projects, and task assignments with role-based access control.

## Features

- **User Management**: Create, update, and manage employee accounts with role-based access (Administrator, Employee)
- **Project Management**: Organize employees into projects and manage project membership
- **Task Management**: Assign tasks to employees, track completion status, and manage due dates
- **Authentication & Authorization**: JWT-based authentication with role-based authorization
- **Password Management**: Users can change their own passwords securely
- **Profile Management**: Employees can update their profile information and upload profile pictures
- **Swagger API Documentation**: Fully documented REST API with interactive Swagger UI
- **Event Publishing**: RabbitMQ integration for event-driven architecture
- **Docker Support**: Full containerization with Docker Compose for easy deployment

## Technology Stack

### Backend
- **Framework**: ASP.NET Core 8
- **Database**: SQL Server 2022
- **ORM**: Entity Framework Core
- **Authentication**: JWT (JSON Web Tokens)
- **Message Queue**: RabbitMQ
- **Logging**: Serilog
- **API Documentation**: Swagger/OpenAPI

### Frontend
- **Framework**: React 18
- **HTTP Client**: Axios
- **Build Tool**: Vite
- **Router**: React Router v6

### Infrastructure
- **Containerization**: Docker & Docker Compose
- **Networking**: Docker Compose networking

## Prerequisites

### For Local Development
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) - Required
- [SQL Server 2022 Express](https://www.microsoft.com/en-us/sql-server/sql-server-2022) or [LocalDB](https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb) - Required
- [Node.js 18+](https://nodejs.org/) (includes npm) - Required
- [Git](https://git-scm.com/) - Required
- A text editor or IDE (Visual Studio, VS Code, etc.)

### For Docker Deployment
- [Docker Desktop](https://www.docker.com/products/docker-desktop) - Includes Docker and Docker Compose
- OR manually installed [Docker](https://docs.docker.com/get-docker/) + [Docker Compose](https://docs.docker.com/compose/install/)

**Verified with:**
- .NET 8 SDK
- Node.js 18+
- Docker 29.6.1
- SQL Server 2022 Express or LocalDB

## Getting Started - Local Development

### Step 1: Verify Prerequisites

Check that all required tools are installed:

```bash
dotnet --version          # Should be 8.0 or higher
node --version            # Should be v18 or higher
npm --version             # Should be v9 or higher
```

### Step 2: Setup SQL Server

You have two options:

**Option A: Using SQL Server Express (Recommended for beginners)**
- Download and install [SQL Server 2022 Express](https://www.microsoft.com/en-us/sql-server/sql-server-2022)
- Note your server name (usually `.\SQLEXPRESS` or `localhost\SQLEXPRESS`)

**Option B: Using LocalDB (Lightweight, no installation required)**
- LocalDB is included with Visual Studio or available separately
- Server name: `(localdb)\mssqllocaldb`

### Step 3: Update Database Connection String

Edit `src/EmployeeManagement.Api/appsettings.json` and set the `DefaultConnection`:

For **SQL Server Express**:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=EmployeeManagementDb;Trusted_Connection=true;TrustServerCertificate=true;"
}
```

For **LocalDB**:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=EmployeeManagementDb;Trusted_Connection=true;TrustServerCertificate=true;"
}
```

For **Windows Authentication** (no password needed if using same Windows account):
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=EmployeeManagementDb;Trusted_Connection=true;TrustServerCertificate=true;"
}
```

### Step 4: Apply Database Migrations

Open a PowerShell/Command Prompt in the project root and run:

```bash
cd src/EmployeeManagement.Api
dotnet ef database update
```

This will:
- Create the database if it doesn't exist
- Create all required tables
- Create default roles (Administrator, Employee)
- Create default admin user: `admin@company.com` / `Admin@12345`

**Troubleshooting:**
- If you get "No database provider" error: Run `dotnet add package Microsoft.EntityFrameworkCore.SqlServer`
- If connection fails: Verify SQL Server is running and connection string is correct
- If database already exists with old schema: Run `dotnet ef database drop` then `dotnet ef database update`

### Step 5: Start the API

In the same `src/EmployeeManagement.Api` directory, run:

```bash
dotnet run
```

You should see output like:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
```

The API will be available at:
- **API Base URL**: http://localhost:5000/api
- **Swagger UI**: http://localhost:5000/swagger

Keep this terminal open (or use `dotnet run` in the background).

### Step 6: Install Frontend Dependencies

Open a **new** PowerShell/Command Prompt window and run:

```bash
cd client-app
npm install
```

This installs React, Vite, Axios, and all other frontend dependencies.

**Troubleshooting:**
- If npm is slow: Try `npm install --prefer-offline`
- If you get permission errors on Mac/Linux: Use `sudo npm install`
- If you get conflicts: Try `npm install --legacy-peer-deps`

### Step 7: Start the Frontend Dev Server

In the same `client-app` directory, run:

```bash
npm run dev
```

You should see output like:
```
VITE v5.4.21  ready in 150 ms

➜  Local:   http://localhost:3000/
```

Open your browser and navigate to **http://localhost:3000**

### Step 8: Login

You should now see the login page. Use the default admin credentials:

- **Email**: admin@company.com
- **Password**: Admin@12345

After successful login, you'll see the Projects page.

### Testing the Application

1. **Create a new employee**: Go to **Admin** menu → **Manage Users** → **+ New User**
   - Email: `employee@company.com`
   - Password: `Employee@123`
   - Role: Employee

2. **Create a project**: On the Projects page, click **+ New Project**
   - Name: "Q1 Development"
   - Description: "Q1 2025 development tasks"
   - Due Date: Pick a future date

3. **Add employee to project**: Click project name → click **+ Add Member** → Select the employee

4. **Create a task**: On project page → click **+ New Task**
   - Title: "Setup API"
   - Description: "Configure development environment"
   - Assign to: Select the employee
   - Due Date: Pick a future date

5. **Login as employee**: Log out → Login with `employee@company.com` / `Employee@123`
   - Go to **My Tasks** → You should see the assigned task
   - Click checkbox to mark as complete

6. **Admin view completed tasks**: Log back in as admin to see updated status

## Getting Started - Docker Deployment

### Prerequisites
- Docker Desktop running
- Port 3000, 5000, 8080, 1433, 5672 available (not in use)

### Quick Start (Recommended)

In the project root directory:

```bash
# Build and start all services (API, Frontend, Database, RabbitMQ)
docker-compose -f docker/docker-compose.yml up --build

# (It will take 2-3 minutes for first run)
```

Wait for output showing:
```
employee-mgmt-api        | Now listening on: http://0.0.0.0:8080
employee-mgmt-client     | listen 80
```

Then visit: **http://localhost:3000**

### Access Points

- **Frontend**: http://localhost:3000
- **API**: http://localhost:8080/api
- **Swagger UI**: http://localhost:8080/swagger
- **RabbitMQ Management**: http://localhost:15672 (guest/guest)

### Stopping Services

```bash
# Stop all containers
docker-compose -f docker/docker-compose.yml down

# Stop and remove all data
docker-compose -f docker/docker-compose.yml down -v
```

### Troubleshooting Docker

**Services keep restarting:**
```bash
docker-compose -f docker/docker-compose.yml logs -f
```
This shows detailed logs for debugging.

**Port already in use:**
```bash
# Find what's using port 3000
lsof -i :3000  # On Mac/Linux

# Or change ports in docker-compose.yml
# ports: ["8000:80"]  # Use 8000 instead of 3000
```

**Database errors in logs:**
```bash
# Wait for SQL Server to be fully ready
docker-compose -f docker/docker-compose.yml ps

# All should show "healthy" before API connects
```

## API Endpoints

### Authentication
- `POST /api/auth/login` - Login with email and password

### Users (Admin Only)
- `GET /api/users` - List all users
- `GET /api/users/{id}` - Get user details
- `POST /api/users` - Create new user
- `PUT /api/users/{id}` - Update user
- `DELETE /api/users/{id}` - Delete user

### User Profile (Self-Service)
- `GET /api/users/me` - Get current user profile
- `PUT /api/users/me` - Update own profile
- `POST /api/users/me/change-password` - Change password
- `POST /api/users/me/profile-picture` - Upload profile picture

### Projects
- `GET /api/projects` - List projects
- `GET /api/projects/{id}` - Get project details
- `POST /api/projects` - Create project
- `PUT /api/projects/{id}` - Update project
- `DELETE /api/projects/{id}` - Delete project (only if no open tasks)
- `POST /api/projects/{id}/members` - Add member to project
- `DELETE /api/projects/{id}/members/{employeeId}` - Remove member from project

### Tasks
- `GET /api/tasks` - Get all tasks
- `GET /api/tasks/my` - Get tasks assigned to current user
- `GET /api/tasks/{id}` - Get task details
- `POST /api/tasks` - Create task
- `PUT /api/tasks/{id}` - Update task
- `POST /api/tasks/{id}/toggle-completion` - Toggle task completion
- `POST /api/tasks/{id}/complete` - Mark task as completed
- `DELETE /api/tasks/{id}` - Delete task

## Business Rules

### User Management
- Administrators cannot delete their own account
- Email addresses must be unique
- Password minimum length is 6 characters
- Roles: Administrator (full access), Employee (limited access)

### Project Management
- Projects must have a unique name
- Projects can have multiple employees
- Employees can see projects they're assigned to (unless Admin)
- Admins can see all projects

### Task Management
- Due dates cannot be in the past (today or future only)
- Tasks can have Open or Completed status
- Only assigned employee or Admin can update task
- Only assigned employee or Admin can mark as completed
- Tasks can be deleted only if no dependencies exist

## Project Structure

```
EmployeeManagement/
├── src/
│   ├── EmployeeManagement.Api/          # ASP.NET Core API backend
│   │   ├── Controllers/                 # API endpoints
│   │   ├── Services/                    # Business logic
│   │   ├── Data/                        # Database context & migrations
│   │   ├── Models/                      # DTOs and request/response models
│   │   ├── Middleware/                  # Custom middleware
│   │   ├── Properties/launchSettings.json
│   │   ├── appsettings.json
│   │   └── Program.cs                   # API configuration
│   └── EmployeeManagement.Domain/       # Domain entities
│       └── Entities/                    # Core business entities
├── client-app/                          # React frontend
│   ├── src/
│   │   ├── pages/                       # Page components
│   │   ├── components/                  # Reusable components
│   │   ├── context/                     # React Context (Auth)
│   │   ├── api/                         # API client
│   │   ├── styles.css                   # Global styles
│   │   └── main.jsx                     # React entry point
│   ├── package.json
│   └── vite.config.js                   # Vite configuration
├── docker/
│   ├── Dockerfile.api                   # API container build
│   ├── Dockerfile.client                # Frontend container build
│   ├── docker-compose.yml               # Docker Compose orchestration
│   └── nginx.conf                       # Nginx configuration
├── tests/
│   └── EmployeeManagement.Tests/        # Unit tests
└── README.md                            # This file
```

## Common Issues & Solutions

### "localhost doesn't appear" / "Cannot connect to API"

**Problem**: Frontend loads but no data appears, console shows 404 errors

**Solution**:
1. Check API is running: Open http://localhost:5000/swagger in browser
2. Check frontend proxy: Look at browser's Network tab
   - API calls should go to `/api/...`
   - The proxy should forward to `http://localhost:5000/api/`
3. Check CORS: API logs should show "CORS request"
   - If missing, update `appsettings.json` CORS origins

### "Build failed" / Compilation errors

**Solution**:
```bash
# Clear and rebuild
rm -r bin obj              # On Mac/Linux
rmdir /s /q bin obj        # On Windows Command Prompt

# Restore packages
dotnet restore

# Try building again
dotnet build
```

### "npm ERR!" or frontend won't start

**Solution**:
```bash
# Clear node_modules and reinstall
rm -r node_modules package-lock.json    # Mac/Linux
rmdir /s /q node_modules package-lock.json  # Windows

npm install
npm run dev
```

### Database connection fails

**Solution**:
1. Verify SQL Server is running
2. Check connection string has correct server name
3. Try with Windows Authentication first
4. Check firewall isn't blocking port 1433

### "Password does not meet complexity requirements"

**Solution**: The admin password in the code (`Admin@12345`) meets requirements. If creating users fails:
- Use passwords with uppercase, lowercase, number, and special character
- Minimum 6 characters

## Performance Notes

- First `dotnet run` takes 30-60 seconds (dependencies load)
- First `npm run dev` takes 20-40 seconds
- First API request takes 2-3 seconds (cold start)
- Docker first build takes 3-5 minutes

## Security Notes

- **NEVER** commit real database passwords to git
- Change default admin password after first login
- Update JWT secret key in `appsettings.json` for production
- HTTPS is recommended for production
- CORS is restricted to localhost for development

## Testing the REST API

You can use tools like Postman, Insomnia, or curl to test the API:

```bash
# Login (get JWT token)
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@company.com","password":"Admin@12345"}'

# Response will include "token": "eyJhbGciOi..."

# Use token in subsequent requests
curl -X GET http://localhost:5000/api/users \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

## Additional Resources

- [ASP.NET Core Documentation](https://learn.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
- [React Documentation](https://react.dev/)
- [Vite Documentation](https://vitejs.dev/)
- [Docker Documentation](https://docs.docker.com/)
- [Swagger/OpenAPI](https://swagger.io/)
- [SQL Server Documentation](https://learn.microsoft.com/en-us/sql/sql-server/)

## Support

For issues or questions:
1. Check the **Common Issues & Solutions** section above
2. Check the logs:
   - API: Console output from `dotnet run`
   - Frontend: Browser console (F12) and Network tab
   - Docker: `docker-compose logs -f`
3. Verify all prerequisites are installed with correct versions

## Version History

### v1.0.0
- Initial release with all core features
- Tested on .NET 8, Node 18+, Docker 29.6.1
- Working local development setup
- Working Docker Compose setup
- Fixed port configuration (5000 instead of 5080)
- Improved UI with description boxes
- Comprehensive documentation