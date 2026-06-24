🚗 PeerCar

A Premium Peer-to-Peer Car Rental Platform

Connecting car owners and renters through a secure, verified, and real-time rental experience.

📖 Overview

PeerCar is an enterprise-grade peer-to-peer car rental platform built on ASP.NET Core 8 MVC, engineered around a layered Domain → Application → Infrastructure → Presentation architecture (Clean Architecture).

It models the full lifecycle of a real-world rental marketplace:

Vehicle onboarding & admin approval workflows

Identity/document verification pipelines

Date-based booking with comprehensive status, payment, and refund tracking

Real-time owner-renter communication via WebSockets

AI-assisted customer support

Secure role-based authentication with Google OAuth and email workflows.

✨ Features

🚙 Car Listings — Owners submit cars with detailed specs, photos, and ownership documents for admin review.

📅 Booking Workflow — Date-based bookings with full lifecycle tracking (Pending → Confirmed → Completed/Cancelled), key handover, and return confirmation.

✅ Admin Approval System — Cars and identity/license documents pass through a Pending → Approved/Rejected moderation pipeline before going live.

🪪 Identity Verification — Renters and owners upload National ID and driver's license (front & back); admins verify details before granting full marketplace access.

💳 Payments — Booking payments with granular status tracking (Pending, Paid, Succeeded, Failed, Refunded) and automated refund support.

⭐ Reviews & Ratings — Users can review both cars and counterparties (owners/renters) after a completed booking.

💬 Real-Time Chat — In-app owner-renter messaging powered by SignalR (ChatHub).

🤖 AI Chatbot Assistant — An in-app assistant (/chatbot) that proxies requests to an external AI microservice.

🔐 Authentication — ASP.NET Core Identity (email/password) plus Google OAuth 2.0 sign-in.

📧 Email Notifications — Password reset and email confirmation flows handled asynchronously via SMTP.

⚙️ Background Services — Automatic booking expiration and booking-status workers running as hosted services.

🛠️ Admin Dashboard — Centralized management of users, car submissions, bookings, and platform activity archives.

📸 Screenshots & Demo

🏠 Landing Page

🛠️ Admin Dashboard

💬 Real-Time Chat

🚗 Car Listings & Booking Flow

💡 Note: Replace the placeholder paths above with your actual image/GIF files (e.g., inside a docs/screenshots/ folder in the repository root). Recommended width is 800px for consistent rendering across GitHub's light and dark themes.

🏗️ Architecture

The solution follows a Clean / Layered Architecture inside a single ASP.NET Core MVC project:

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


🧰 Tech Stack

Layer

Technology / Framework

Description

Framework

ASP.NET Core MVC (.NET 8)

Core web framework

Database

SQL Server (via EF Core 8)

Relational database mapping & migrations

Auth

ASP.NET Core Identity + Google OAuth 2.0

Secure user management and external logins

Real-time

SignalR

WebSockets wrapper for instant messaging

Frontend

Razor Views + Bootstrap 5

Clean, responsive, and dynamic UI rendering

AI Integration

External AI Microservice

Proxied conversational assistant endpoint

Email

SMTP (Gmail Secure App SMTP)

Transactional and verification emails

🚀 Getting Started

Prerequisites

.NET 8 SDK or higher

SQL Server (LocalDB, Express, or full SQL Server instance)

Google OAuth Client ID/Secret (Optional — only needed for "Sign in with Google")

SMTP Account e.g., Gmail with an App Password (Optional — only needed for email confirmations)

1️⃣ Clone the repository

git clone https://github.com/YoussefIbrahim13/PeerCar.git
cd PeerCar


2️⃣ Configure secrets

⚠️ Security Notice: Never commit real credentials or keys to appsettings.json. Use the .NET Secret Manager for local development instead:

cd Peer_Car
dotnet user-secrets init

# Configure database and external services
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=.;Database=PeerCarDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
dotnet user-secrets set "EmailSettings:SmtpServer" "smtp.gmail.com"
dotnet user-secrets set "EmailSettings:Port" "587"
dotnet user-secrets set "EmailSettings:SenderEmail" "your-email@gmail.com"
dotnet user-secrets set "EmailSettings:SenderName" "PeerCar Support"
dotnet user-secrets set "EmailSettings:Password" "your-app-password"
dotnet user-secrets set "Authentication:Google:ClientId" "your-google-client-id"
dotnet user-secrets set "Authentication:Google:ClientSecret" "your-google-client-secret"
dotnet user-secrets set "AiServiceUrl" "http://localhost:8000/ai/v1/chat"


Secret Key

Description

ConnectionStrings:DefaultConnection

Your local or remote SQL Server connection string.

EmailSettings:*

SMTP setup parameters for transactional platform notifications.

Authentication:Google:*

OAuth API credentials configured in Google Cloud Console.

AiServiceUrl

The active HTTP URL endpoint for the AI assistant chatbot.

3️⃣ Apply database migrations

cd Peer_Car
dotnet ef database update


4️⃣ Run the application

dotnet run


The application will boot up and be accessible locally at https://localhost:5001 (or whichever port is assigned by the console output). The database seeds automatically on its first initialization via DbInitializer.

👤 User Roles

Role

Permissions & Responsibilities

🧍 Renter

Search/filter active cars, upload identity documents, and book rentals.

🚘 Owner

List personal cars, manage booking requests, track return handovers.

🛡️ Admin

Moderates car submittals, reviews identity verification, oversees users and platform metrics.

🔒 Security Note

[!WARNING]
Important Historical Warning:
This repository's appsettings.json previously contained live SMTP credentials committed directly to source control. If you have forked or cloned this repository with that file present:

Rotate your email account's app password immediately.

Purge the secret from git history using git filter-repo or the BFG Repo-Cleaner.

Migrate all future configuration secrets securely via User Secrets or environment variables.

🗺️ Roadmap

[ ] Containerize application with Docker & docker-compose (App + SQL Server)

[ ] Implement CI/CD pipeline (build, test, lint, deploy)

[ ] Migrate production configuration to Azure Key Vault or AWS Secrets Manager

[ ] Expand automated unit/integration test coverage for Application/Services

[ ] Add JWT-secured RESTful API endpoints for a future mobile client

🤝 Contributing

Contributions, issues, and feature requests are welcome! Feel free to open an issue or submit a pull request.

📄 License

No license file is currently included in this repository. Add a LICENSE file (e.g., MIT) if you intend others to use, modify, or contribute to this project.

Built with ❤️ using ASP.NET Core
