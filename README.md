<div align="center">

<img src="https://img.shields.io/badge/PeerCar-Drive%20Your%20Way-0d6efd?style=for-the-badge&logo=car&logoColor=white" alt="PeerCar" height="60"/>

<br/>
<br/>

**A full-stack, peer-to-peer car rental platform built on Clean Architecture**  
*Enabling car owners and renters to connect seamlessly — with real-time AI assistance, admin oversight, and a robust booking lifecycle engine.*

<br/>

[![.NET](https://img.shields.io/badge/.NET_8.0-512BD4?style=flat-square&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![ASP.NET Core MVC](https://img.shields.io/badge/ASP.NET_Core_MVC-512BD4?style=flat-square&logo=dotnet&logoColor=white)](https://learn.microsoft.com/en-us/aspnet/core/mvc/)
[![Entity Framework Core](https://img.shields.io/badge/EF_Core_8-512BD4?style=flat-square&logo=dotnet&logoColor=white)](https://learn.microsoft.com/en-us/ef/core/)
[![SQL Server](https://img.shields.io/badge/SQL_Server-CC2927?style=flat-square&logo=microsoftsqlserver&logoColor=white)](https://www.microsoft.com/en-us/sql-server)
[![SignalR](https://img.shields.io/badge/SignalR-Real--time-512BD4?style=flat-square&logo=dotnet&logoColor=white)](https://learn.microsoft.com/en-us/aspnet/core/signalr/)
[![Bootstrap 5](https://img.shields.io/badge/Bootstrap_5-7952B3?style=flat-square&logo=bootstrap&logoColor=white)](https://getbootstrap.com/)
[![jQuery](https://img.shields.io/badge/jQuery-0769AD?style=flat-square&logo=jquery&logoColor=white)](https://jquery.com/)
[![Google Auth](https://img.shields.io/badge/Google_OAuth-4285F4?style=flat-square&logo=google&logoColor=white)](https://developers.google.com/identity)
[![SMTP](https://img.shields.io/badge/SMTP_Email-EA4335?style=flat-square&logo=gmail&logoColor=white)](https://support.google.com/mail/answer/7126229)

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg?style=flat-square)](LICENSE)
[![Build](https://img.shields.io/badge/Build-Passing-brightgreen?style=flat-square&logo=github-actions&logoColor=white)]()
[![PRs Welcome](https://img.shields.io/badge/PRs-Welcome-brightgreen?style=flat-square)](https://github.com/YoussefIbrahim13/PeerCar/pulls)
[![EF Migrations](https://img.shields.io/badge/DB_Migrations-6_Applied-blue?style=flat-square)]()

</div>

---

## 📸 Live Preview

> A responsive, modern UI designed for car owners, renters, and platform administrators — all in one cohesive experience.

<div align="center">

| 🏠 Homepage | 🚗 Browse Cars | 📊 Admin Dashboard |
|:-----------:|:--------------:|:-----------------:|
| Hero banner with Browse & Join CTAs | Filterable car listings with availability status | Real-time platform statistics |

| 📅 Booking Flow | 💬 AI Chatbot | 🛡️ KYC Verification |
|:---------------:|:-------------:|:--------------------:|
| Date selection with auto price calculation | Streaming AI assistant via SignalR | National ID & License review queue |

> 🔗 Deploy easily on **[Azure App Service](https://azure.microsoft.com/en-us/products/app-service)**, **[Railway](https://railway.app/)**, or any server running .NET 8.

</div>

---

## 🏛️ Architecture Overview

PeerCar is structured around **Clean Architecture** (Onion Architecture), enforcing a strict dependency rule: outer layers depend on inner ones, never the reverse. Business rules are framework-agnostic and fully testable in isolation.

```
PeerCar/
├── Domain/                     ← Core — no external dependencies
│   ├── Entities/               ← Car, User, Booking, Payment, Review, CarSubmission
│   ├── Enums/                  ← BookingStatus, PaymentStatus, UserRole, DocumentStatus…
│   ├── Interfaces/             ← IGenericRepository<T>
│   └── Common/                 ← BaseEntity (Guid Id + CreatedAt, auto-assigned)
│
├── Application/                ← Use-case orchestration
│   ├── Interfaces/             ← ICarService, IBookingService, IPaymentService…
│   ├── Services/               ← All business logic implementations
│   └── ViewModels/             ← DTOs bridging Presentation ↔ Application
│
├── Infrastructure/             ← External concerns (DB, email, workers)
│   ├── Data/                   ← ApplicationDbContext (EF Core + ASP.NET Identity)
│   ├── Repositories/           ← GenericRepository<T> implementation
│   ├── Seeders/                ← DbInitializer — seeds roles and admin on startup
│   └── Services/               ← EmailSender, BookingStatusBackgroundService, BookingExpirationWorker
│
└── Presentation/               ← ASP.NET Core MVC delivery mechanism
    ├── Controllers/            ← Account, Admin, Bookings, Cars, Chatbot, Payments, Reviews, Users
    ├── Views/                  ← Razor (.cshtml) views per feature area
    ├── Hubs/                   ← ChatHub — real-time AI response streaming via SignalR
    ├── ViewComponents/         ← RecentCarsViewComponent
    └── wwwroot/                ← Bootstrap 5, jQuery, Bootstrap Icons, custom CSS/JS
```

---

## ✨ Feature Highlights

### 🚗 Car Listing & Multi-Stage Submission Approval
Owners submit cars through a controlled approval workflow. Each submission enters a `Pending` state, where admins review uploaded car images and documents before approving or rejecting. Rejection reasons are captured in the model and communicated back to the owner automatically by email.

### 📅 Booking Lifecycle Engine
The booking system models the full real-world rental experience as a state machine:

```
Pending ──(payment)──► Confirmed ──(key handover)──► Car Received ──(return)──► Completed
                           │
                           └──(no key handover within 24h)──► Cancelled (auto)
```

Two `IHostedService` background workers keep state consistent without any manual intervention:

- **`BookingStatusBackgroundService`** — periodically scans for confirmed bookings past their end date and marks them `Completed`.
- **`BookingExpirationWorker`** — cancels confirmed bookings where the key was never handed over within the allowed window.

### 💳 Payment Processing
`PaymentService` calculates total rental cost (`PricePerDay × rental days`, with a minimum of one day), generates a unique transaction ID, and transitions the booking to `Confirmed` atomically. The `Booking` entity tracks `RefundAmount` and `IsRefunded` for post-return financials.

### 💬 AI-Powered Chatbot (Real-time Streaming)
An integrated AI assistant powered by **ASP.NET Core SignalR** streams responses word-by-word from a configurable external Python AI microservice. The `ChatHub` sends the user's message via HTTP, reads the response as a raw stream, and pushes character chunks back to the browser in real time — producing a natural typewriter effect.

### 🔐 Authentication & Identity
- **ASP.NET Core Identity** with role-based access control: `Admin`, `Owner`, `Renter`
- **Google OAuth 2.0** with profile picture claim mapping
- **Email confirmation** using 30-minute expiring tokens
- **Password reset** via SMTP with HTML email templates
- **Soft delete** on `User` and `Car` — data is never hard-deleted; global EF Core query filters transparently exclude soft-deleted records from all queries

### 🛡️ Admin Control Panel
A dedicated admin area provides full platform governance:

- Dashboard with live aggregate stats (total users, cars, bookings, pending submissions)
- User management: create, activate/suspend, soft-delete, view per-user reservations
- **KYC / Identity Verification**: review queue for National ID (front & back) and Driver's License uploads
- Car submission approval/rejection with admin notes and automated email notification
- Complete visibility into all bookings, payments, and reviews

### ⭐ Dual-Target Review System
Reviews can target either a **Car** or a **User** (Owner or Renter), and are optionally linked to a specific `Booking` for verified-rental context. This enables trustworthy, contextual reputation-building for both vehicles and people on the platform.

### 📧 Transactional Email (SMTP / Gmail)
HTML emails are sent for: registration confirmation, email change requests, password resets, account status changes, and car submission outcomes.

---

## 🗄️ Data Model at a Glance

```
User ─────────────┬──< OwnedCars ──< CarSubmission (approval audit trail)
                  │         └──< Bookings ──< Payment
                  │                    └──< Reviews (targets Car OR User)
                  └──< Reviews (as reviewer)
```

**Key design decisions:**
- **All PKs are `Guid`** — collision-safe and distribution-ready, no identity column dependency.
- **Global soft-delete query filters** on `User`, `Car`, `Booking`, `Review` — deleted records are excluded transparently across the entire application.
- **`decimal(18,2)` enforced globally** via `OnModelCreating` reflection — no precision loss on any monetary field.
- **`DeleteBehavior.Restrict`** on all critical FK relationships — prevents accidental cascading data loss.

---

## 🛠️ Tech Stack

| Concern | Technology |
|---|---|
| **Web Framework** | ASP.NET Core MVC on .NET 8 |
| **ORM & Migrations** | Entity Framework Core 8 + SQL Server provider |
| **Authentication** | ASP.NET Core Identity + Google OAuth 2.0 |
| **Real-time Communication** | ASP.NET Core SignalR |
| **Background Processing** | `IHostedService` / `BackgroundService` |
| **Email** | SMTP via `System.Net.Mail` (Gmail App Password) |
| **Frontend** | Razor Views, Bootstrap 5, Bootstrap Icons, jQuery |
| **AI Service** | External Python microservice (streaming HTTP) |
| **Database** | Microsoft SQL Server |
| **Architecture Pattern** | Clean Architecture, Generic Repository, Dependency Injection |

---

## 🚀 Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (LocalDB is sufficient for development)
- A Gmail account with an [App Password](https://myaccount.google.com/apppasswords)
- *(Optional)* Google Cloud Console credentials for OAuth

### 1. Clone the Repository

```bash
git clone https://github.com/YoussefIbrahim13/PeerCar.git
cd PeerCar
```

### 2. Configure Application Settings

Edit `Peer_Car/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=PeerCarDb;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "Port": 587,
    "SenderEmail": "your-email@gmail.com",
    "SenderName": "PeerCar Support",
    "Password": "your-gmail-app-password"
  },
  "AiServiceUrl": "http://localhost:8000/ai/v1/chat"
}
```

For Google OAuth, use [User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) (recommended) or `appsettings.Development.json`:

```json
{
  "Authentication": {
    "Google": {
      "ClientId": "YOUR_GOOGLE_CLIENT_ID",
      "ClientSecret": "YOUR_GOOGLE_CLIENT_SECRET"
    }
  }
}
```

### 3. Apply Database Migrations

```bash
cd Peer_Car
dotnet ef database update
```

This creates the `PeerCarDb` database and applies all 6 migrations, including Identity schema and KYC document fields.

### 4. Run the Application

```bash
dotnet run
```

On first launch, `DbInitializer` automatically seeds the database with roles and a default admin account. Navigate to `https://localhost:{port}` to explore the platform.

---

## 🔌 AI Chatbot Integration

The chatbot connects to an external Python AI service at the configured `AiServiceUrl`. The `ChatHub` expects:

**Request**
```http
POST /ai/v1/chat
Content-Type: application/json

{ "message": "What cars are available near Cairo?" }
```

**Response:** A streaming HTTP response — text chunks emitted progressively. The hub reads them in 10-character buffers and pushes each chunk to the connected client via SignalR, producing the typewriter streaming effect in the browser.

---

## 👥 User Roles & Permissions

| Role | Capabilities |
|------|-------------|
| **Admin** | Full platform control: user management, KYC, car submissions, all bookings & payments |
| **Owner** | List cars, manage approval status, view car bookings, coordinate key handover |
| **Renter** | Browse cars, create bookings, pay, confirm car receipt & return, leave reviews |
| **User** | Default role on registration — promoted to `Owner` on first car listing submission |

---

## 🤝 Contributing

Contributions, issues, and feature requests are welcome!

1. Fork the repository
2. Create your feature branch: `git checkout -b feature/amazing-feature`
3. Commit your changes: `git commit -m 'feat: add amazing feature'`
4. Push to the branch: `git push origin feature/amazing-feature`
5. Open a Pull Request

---

## 📄 License

This project is licensed under the **MIT License** — see the [LICENSE](LICENSE) file for details.

---

<div align="center">

Built with ❤️ by [Youssef Ibrahim](https://github.com/YoussefIbrahim13)

⭐ **Star this repo** if you find it useful!

</div>
