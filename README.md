# CRMPro — Complete CRM Management System

A portfolio-ready CRM built with **React + Vite**, **ASP.NET Core 8**, **EF Core**, **SQLite**, and **JWT**.

## Features

- JWT authentication and role-based authorization
- PBKDF2 password hashing with legacy demo-password upgrade
- Admin / Sales roles
- Dashboard with live KPIs
- Customers CRUD, search, filters, pagination
- Customer details with related leads, deals and activities
- Leads CRUD, search and filters
- Deals CRUD and visual drag-and-drop pipeline
- Activities and notes with CRUD management
- Notifications with unread counter and mark-all-read
- Reports with pipeline metrics, win rate and CSV export
- Admin user management (create, edit, disable and delete)
- Audit logs
- Profile editing and password change
- Personal settings
- Swagger API documentation
- SQLite database with demo seed data
- Responsive UI
- Explicit CORS handling
- Stable SQLite decimal aggregation handling

## Run

### Backend

Double-click:

`START-BACKEND.bat`

or:

```powershell
cd Backend
dotnet restore
dotnet run --project .\CRMPro.Api.csproj --urls "http://localhost:5002"
```

Swagger:

`http://localhost:5002/swagger`

### Frontend

```powershell
cd Frontend
npm install
npm run dev
```

Open:

`http://localhost:5173`

## Demo accounts

Admin:

`admin@crmpro.com` / `123456`

Sales:

`sales@crmpro.com` / `123456`

Manager/Sales:

`manager@crmpro.com` / `123456`

## Portfolio positioning

CRMPro demonstrates authentication, authorization, CRUD, REST APIs, database integration, dashboards, sales pipelines, reporting, notifications, audit trails and responsive business UI in one full-stack application.
