# Todo API + Frontend

How to Run Project
🔹 OPTION 1 — Local Run (Manual)
Step 1 — Clone repo
git clone https://github.com/zzeroootwooo/DSSFinalProjectLeontiiLeonov

Step 2 — Setup database

Make sure PostgreSQL is running.

Edit appsettings.json:

"ConnectionStrings": {
  "DefaultConnection": "Host=127.0.0.1;Port=5432;Database=todo_db;Username=postgres;Password=postgres"
}
Step 3 — Setup JWT
"Jwt": {
  "Key": "ThisIsASecretKeyForJwtTokenGeneration",
  "Issuer": "TodoApi"
}
Step 4 — Run backend
dotnet restore
dotnet run

Backend will start on:

http://localhost:3087

Swagger:

http://localhost:3087/swagger
Step 5 — Run frontend

Go to frontend folder:

cd ./frontend
npm install
npm run dev
Step 6 — Run tests
npm run test:e2e

or

npm run cy:open

🔹 OPTION 2 — Run with Docker (Recommended)
Step 1 — Build backend image

Go to root folder:

docker compose up --build

## Notes about Docker

- `docker compose build` only builds images but does NOT run containers
- `docker compose up` starts containers
- `docker compose up --build` builds and runs in one command (recommended)

Step 2 — Run full system

Go to frontend folder:

cd ./frontend
For Windows (PowerShell):
$env:BACKEND_IMAGE="dss_final_project-backend:latest"
docker compose -f docker-compose.e2e.yml up --build --exit-code-from cypress
For Linux / Mac:
export BACKEND_IMAGE="dss_final_project-backend:latest"
docker compose -f docker-compose.e2e.yml up --build --exit-code-from cypress

This will start:

PostgreSQL
Backend
Frontend
Cypress tests
Swagger Usage

Open:
http://localhost:3087/swagger
Register user
Login
Copy token
Click Authorize
Paste token
Test protected endpoints

## Overview

This project is a full-stack To-Do application built as part of the DSS final assignment.

The system includes:

- ASP.NET Core Web API backend
- PostgreSQL database
- JWT authentication
- Frontend client
- Docker / Docker Compose setup
- Cypress end-to-end tests

Users can register, log in, create their own todos, update them, mark them as completed, delete them, and optionally make them public. Public todos can be viewed without authentication.

---

## Main Features

### Authentication
- User registration
- User login
- JWT Bearer authentication
- Protected endpoints for private user data

### Todo Management
- Create a todo
- View own todos
- View todo by ID
- Update todo
- Mark todo as completed / not completed
- Delete todo

### Public Todos
- Public todo list endpoint available without login
- Public items are visible to everyone

### Filtering, Sorting and Pagination
- Pagination with page and pageSize
- Filtering by completion status
- Filtering by priority
- Filtering by due date range
- Search by title/details
- Sorting by created date, due date, priority, and title

### Testing
- Cypress E2E tests for authentication, public todos, CRUD, pagination, filtering, sorting, and search

---

## Tech Stack

### Backend
- ASP.NET Core 8
- Entity Framework Core
- PostgreSQL
- JWT Authentication
- BCrypt password hashing


### Testing
- Cypress

### DevOps
- Docker
- Docker Compose

---

## Project Structure

```text
TodoApi/
│
├── Controllers/         # API controllers
├── Data/                # AppDbContext and database configuration
├── DTOs/                # Request/response DTO classes
├── Models/              # Entity models
├── Migrations/          # EF Core migrations
├── Properties/
├── Program.cs           # Application configuration and startup
├── appsettings.json     # Local configuration
├── Dockerfile
├── docker-compose.yml
│
└── frontend/
    ├── src/             # Frontend source code
    ├── cypress/         # End-to-end tests
    ├── Dockerfile
    ├── cypress.Dockerfile
    └── docker-compose.e2e.yml


    How the Backend Works
Program.cs

Program.cs is responsible for:

registering services
configuring the database connection
enabling CORS
configuring JWT authentication
enabling Swagger
applying EF Core migrations on startup
Controllers

The backend uses controllers to expose REST API endpoints.

AuthController

Responsible for authentication:

POST /api/auth/register — creates a new user
POST /api/auth/login — validates credentials and returns JWT token
TodosController

Responsible for todo operations:

GET /api/todos/public — returns public todos
GET /api/todos — returns authenticated user's todos
POST /api/todos — creates a todo
GET /api/todos/{id} — returns one todo by id
PUT /api/todos/{id} — updates a todo
PATCH /api/todos/{id}/completion — updates completion status
DELETE /api/todos/{id} — deletes a todo

DTOs

DTOs are used to separate API contracts from database entities.
This improves clarity, validation, and API safety.

Models

Models represent database entities such as:

User
TodoItem
AppDbContext

AppDbContext defines the database tables and relationships used by Entity Framework Core.

Database

The project uses PostgreSQL.

Main tables:

Users
Todos

Relationships:

one user can own many todos
each todo belongs to one user
Authentication

Authentication is implemented using JWT Bearer tokens.

Flow:

User registers with email and password
Password is hashed with BCrypt and saved in the database
User logs in
Backend validates credentials
Backend generates JWT token
Client sends token in the Authorization header for protected endpoints

Example header:

Authorization: Bearer YOUR_TOKEN_HERE

## Database Migrations

Migrations are applied automatically on application startup using:

db.Database.Migrate();

This means:
- no manual migration commands are required
- database schema is created automatically

Manual command (optional):

dotnet ef database update

