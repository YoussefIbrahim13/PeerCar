# 🚗 PeerCar

### A Peer-to-Peer Car Rental Platform

*Connecting car owners and renters through a secure, verified, and real-time rental experience.*

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat-square&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![ASP.NET Core MVC](https://img.shields.io/badge/ASP.NET%20Core-MVC-512BD4?style=flat-square&logo=dotnet&logoColor=white)](https://learn.microsoft.com/aspnet/core/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-2019%2B-CC2927?style=flat-square&logo=microsoftsqlserver&logoColor=white)](https://www.microsoft.com/sql-server)
[![Entity Framework Core](https://img.shields.io/badge/EF%20Core-8.0-512BD4?style=flat-square&logo=dotnet&logoColor=white)](https://learn.microsoft.com/ef/core/)
[![SignalR](https://img.shields.io/badge/SignalR-Real--time-00B7C3?style=flat-square&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/apps/aspnet/signalr)
[![Bootstrap](https://img.shields.io/badge/Bootstrap-5-7952B3?style=flat-square&logo=bootstrap&logoColor=white)](https://getbootstrap.com/)
[![OAuth 2.0](https://img.shields.io/badge/OAuth%202.0-Google-4285F4?style=flat-square&logo=google&logoColor=white)](https://developers.google.com/identity/protocols/oauth2)
[![License](https://img.shields.io/badge/License-Unspecified-lightgrey?style=flat-square)](#-license)

</div>

---

## 📖 Overview

**PeerCar** is an enterprise-grade peer-to-peer car rental platform built on **ASP.NET Core 8 MVC**, engineered around a layered **Domain → Application → Infrastructure → Presentation** architecture. It models the full lifecycle of a real-world rental marketplace — vehicle onboarding and admin approval, identity/document verification, date-based booking with payment and refund tracking, real-time owner–renter communication, and an AI-assisted support channel — all behind role-based authentication with Google OAuth and email-based account workflows.

---

## ✨ Features

- 🚙 **Car Listings** — owners submit cars with photos and ownership documents for admin review
- 📅 **Booking Workflow** — date-based bookings with full lifecycle tracking (`Pending → Confirmed → Completed/Cancelled`), key handover, and return confirmation
- ✅ **Admin Approval System** — cars and identity/license documents pass through a `Pending → Approved/Rejected` moderation pipeline before going live
- 🪪 **Identity Verification** — renters/owners upload National ID and driver's license (front & back); admins verify before granting full marketplace access
- 💳 **Payments** — booking payments with granular status tracking (Pending, Paid, Succeeded, Failed, Refunded) and refund support
- ⭐ **Reviews & Ratings** — users can review both cars and counterparties (owners/renters) after a completed booking
- 💬 **Real-Time Chat** — in-app owner–renter messaging powered by **SignalR** (`ChatHub`)
- 🤖 **AI Chatbot Assistant** — an in-app assistant (`/chatbot`) that proxies requests to an external AI microservice
- 🔐 **Authentication** — ASP.NET Core Identity (email/password) plus **Google OAuth** sign-in
- 📧 **Email Notifications** — password reset and email confirmation flows via SMTP
- ⚙️ **Background Services** — automatic booking expiration and booking-status workers running as hosted services
- 🛠️ **Admin Dashboard** — centralized management of users, car submissions, bookings, and an activity archive

---

## 📸 Screenshots & Demo

<div align="center">

### 🏠 Landing Page
<img src="https://github.com/user-attachments/assets/6bc1ddc8-3217-4ae4-9556-625fce04f97f" alt="PeerCar Landing Page" width="800"/>

<br/><br/><br/>

### 🛠️ Admin Dashboard
<img src="https://github.com/user-attachments/assets/7889d954-34c0-4b57-8837-e55edabc7d9a" alt="Admin Dashboard" width="800"/>

<br/><br/><br/>

### 💬 Real-Time Chat
<img src="https://github.com/user-attachments/assets/977adeff-9161-40b3-88b1-297dd20bf053" alt="Real-Time Chat Demo" width="800"/>

<br/><br/><br/>

### 🚗 Car Listings & Booking Flow
<img src="https://github.com/user-attachments/assets/77bdb100-adef-4024-b8cb-6d4c94aa8a62" alt="Car Listings" width="800"/>

</div>
> 💡 **Note:** Replace the placeholder paths above with your own image/GIF files (e.g. inside a `docs/screenshots/` folder in the repo root) once available. Recommended width: `800px` for consistent rendering across GitHub's light and dark themes.

---

## 🏗️ Architecture

The solution follows a **Clean / Layered Architecture** inside a single ASP.NET Core MVC project:

```
Peer_Car/
├── Domain/              # Entities, enums, and core interfaces (no external dependencies)
│   ├── Entities/         # Car, Booking, Payment, Review, User, CarSubmission...
│   ├── Enums/            # BookingStatus, UserRole, PaymentStatus, DocumentStatus...
│   └── Interfaces/       # IGenericRepository
├── Application/         # Business logic
│   ├── Interfaces/       # Service contracts (ICarService, IBookingService, ...)
│   ├── Services/         # Service implementations
│   └── ViewModels/       # DTOs / view models used by the MVC layer
├── Infrastructure/       # EF Core data access, repositories, external services
│   ├── Data/              # ApplicationDbContext, DbInitializer
│   ├── Repositories/       # GenericRepository<T>
│   └── Services/           # EmailSender, BookingExpirationWorker, file storage...
├── Presentation/         # MVC layer
│   ├── Controllers/        # Account, Admin, Bookings, Cars, Reviews, Chatbot, Users...
│   ├── Hubs/                # ChatHub (SignalR)
│   └── Views/                # Razor views per controller
├── Migrations/           # EF Core migrations
└── Program.cs            # App startup & DI configuration
```

### 🧰 Tech Stack

| Layer | Technology |
|---|---|
| **Framework** | ASP.NET Core MVC (.NET 8) |
| **Database** | SQL Server (via EF Core 8) |
| **Auth** | ASP.NET Core Identity + Google OAuth 2.0 |
| **Real-time** | SignalR |
| **Frontend** | Razor Views + Bootstrap 5 |
| **AI Integration** | External chat microservice (configurable URL) |
| **Email** | SMTP (Gmail by default) |

---

## 🚀 Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (LocalDB, Express, or full SQL Server)
- A Google OAuth Client ID/Secret *(optional — only needed for "Sign in with Google")*
- An SMTP account, e.g. a Gmail account with an App Password *(optional — only needed for email confirmation / password reset)*

### 1️⃣ Clone the repository

```bash
git clone https://github.com/YoussefIbrahim13/PeerCar.git
cd PeerCar
```

### 2️⃣ Configure secrets

> ⚠️ **Security Notice:** Never commit real credentials to `appsettings.json`. Use the .NET [Secret Manager](https://learn.microsoft.com/aspnet/core/security/app-secrets) for local development instead:

```bash
cd Peer_Car
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=.;Database=PeerCarDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
dotnet user-secrets set "EmailSettings:SmtpServer" "smtp.gmail.com"
dotnet user-secrets set "EmailSettings:Port" "587"
dotnet user-secrets set "EmailSettings:SenderEmail" "your-email@gmail.com"
dotnet user-secrets set "EmailSettings:SenderName" "PeerCar Support"
dotnet user-secrets set "EmailSettings:Password" "your-app-password"
dotnet user-secrets set "Authentication:Google:ClientId" "your-google-client-id"
dotnet user-secrets set "Authentication:Google:ClientSecret" "your-google-client-secret"
dotnet user-secrets set "AiServiceUrl" "http://localhost:8000/ai/v1/chat"
```

| Key | Description |
|---|---|
| `ConnectionStrings:DefaultConnection` | SQL Server connection string |
| `EmailSettings:*` | SMTP settings used for transactional emails |
| `Authentication:Google:ClientId/ClientSecret` | Google OAuth credentials for "Sign in with Google" |
| `AiServiceUrl` | Base URL of the AI chat microservice powering `/chatbot` |

### 3️⃣ Apply database migrations

```bash
cd Peer_Car
dotnet ef database update
```

### 4️⃣ Run the application

```bash
dotnet run
```

The app will be available at `https://localhost:5001` (or the port shown in the console). The database is seeded on first run via `DbInitializer`.

---

## 👤 User Roles

| Role | Description |
|---|---|
| 🧍 `Renter` | Browses and books available cars |
| 🚘 `Owner` | Lists/submits cars for rent, manages their bookings |
| 🛡️ `Admin` | Approves car submissions and document verification, manages users and the platform |

---

## 🔒 Security Note

> ⚠️ **Important:** This repository's `appsettings.json` previously contained **live SMTP credentials** committed directly to source control.
>
> If you've forked or cloned this repo with that file present:
> 1. **Rotate the email account's app password immediately.**
> 2. **Purge the secret from git history** using [`git filter-repo`](https://github.com/newren/git-filter-repo) or the [BFG Repo-Cleaner](https://rtyley.github.io/bfg-repo-cleaner/).
> 3. **Migrate all future configuration** to user secrets, environment variables, or a managed secret store (e.g. Azure Key Vault).

---

## 🗺️ Roadmap

- [ ] Containerize with Docker / docker-compose (app + SQL Server)
- [ ] CI/CD pipeline (build, test, lint, deploy)
- [ ] Migrate sensitive config to environment variables / secret manager for production
- [ ] Add automated test coverage for `Application/Services`
- [ ] Add JWT-secured API endpoints for a future mobile client

---

## 🤝 Contributing

Contributions, issues, and feature requests are welcome! Feel free to open an issue or submit a pull request.

---

## 📄 License

No license file is currently included in this repository. Add a `LICENSE` file (e.g. MIT) if you intend others to use or contribute to this project.

---

<div align="center">

Built with ❤️ using ASP.NET Core

</div>
