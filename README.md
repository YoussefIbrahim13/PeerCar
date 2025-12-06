# 🚗 PeerCar - Peer-to-Peer Car Rental Platform

![Build Status](https://img.shields.io/badge/Build-Passing-brightgreen)
![Platform](https://img.shields.io/badge/Platform-Web-blue)
![License](https://img.shields.io/badge/License-MIT-green)
![.NET](https://img.shields.io/badge/.NET-8.0-purple)

**PeerCar** is a robust peer-to-peer car rental web application built with **ASP.NET Core MVC**. It connects car owners with renters through a secure, feature-rich platform. The system handles the entire rental lifecycle, from car listings and booking management to payments and reviews, ensuring a seamless user experience.

---

## ✨ Key Features

### 👤 For Users (Renters & Owners)
* **Secure Authentication:** User accounts managed via **ASP.NET Identity** with role-based access.
* **Car Catalog:** Browse available cars with advanced filtering and detailed views.
* **Booking System:** Complete reservation workflow with availability checking and status tracking.
* **Car Submission:** Owners can submit their vehicles for listing (requires admin approval).
* **Reviews & Ratings:** Users can leave reviews for cars and other users based on their booking history.
* **Profile Management:** Manage personal info, upload profile pictures, and view booking history.

### 🛡️ For Admins
* **Admin Dashboard:** Comprehensive overview of platform statistics.
* **Content Management:** Approve or reject car submissions and manage listings.
* **User Management:** View all users, handle account restrictions, and soft-delete support.
* **Reservation Oversight:** Monitor and manage all booking activities.

---

## 🛠️ Tech Stack & Architecture

The project follows the **MVC (Model-View-Controller)** architectural pattern with **Code-First** approach.

### Backend
* **Framework:** ASP.NET Core MVC (.NET 8)
* **Language:** C#
* **ORM:** Entity Framework Core (Code-First with Migrations)
* **Database:** SQL Server
* **Authentication:** ASP.NET Core Identity
* **Email Service:** SendGrid (Production) / Custom Fallback (Development)
* **Background Tasks:** Hosted Services for periodic booking status updates.

### Frontend
* **View Engine:** Razor Views (.cshtml)
* **Styling:** Bootstrap 5, Custom CSS
* **Icons:** Bootstrap Icons
* **Scripting:** jQuery

---

## 🗄️ Database Schema

The database relies on a relational model featuring the following key entities:
* **Users:** Extended IdentityUser with profile attributes.
* **Cars:** Vehicle details, ownership linkage, and approval status.
* **Bookings:** Links Users and Cars with reservation timelines and status.
* **Payments:** 1:1 relationship with Bookings for financial tracking.
* **Reviews:** Polymorphic review system targeting Cars or Users.
* **CarSubmissions:** Staging table for vehicle approval workflows.

---

## 🚀 Getting Started

Follow these steps to set up the project locally.

### Prerequisites
* [.NET 8.0 SDK](https://dotnet.microsoft.com/download)
* SQL Server (LocalDB or full instance)
* Visual Studio 2022 or VS Code

### Installation

1.  **Clone the repository**
    ```bash
    git clone [https://github.com/YoussefIbrahim13/PeerCar.git](https://github.com/YoussefIbrahim13/PeerCar.git)
    cd PeerCar
    ```

2.  **Configure Database & API Keys**
    Update `appsettings.json` with your connection string and SendGrid API Key:
    ```json
    "ConnectionStrings": {
      "DefaultConnection": "Server=.;Database=PeerCarDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=true"
    },
    "SendGrid": {
      "ApiKey": "YOUR_SENDGRID_API_KEY"
    }
    ```

3.  **Apply Migrations**
    Create the database and apply the schema:
    ```bash
    dotnet ef database update
    ```

4.  **Run the Application**
    ```bash
    dotnet run
    ```
    Visit `https://localhost:7198` (or the port shown in your terminal) to view the app.

---

## 📸 Screenshots

| Home Page | Car Details |
|:---:|:---:|
| ![Home Page](path/to/image1.png) | ![Car Details](path/to/image2.png) |

| Admin Dashboard | Booking Flow |
|:---:|:---:|
| ![Dashboard](path/to/image3.png) | ![Booking](path/to/image4.png) |

---

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## 📄 License

This project is licensed under the MIT License.
