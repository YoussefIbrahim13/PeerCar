# PeerCar

PeerCar is an ASP.NET Core MVC application for peer-to-peer car rentals. It provides user registration and authentication (ASP.NET Identity), car listings, booking management, admin tools, and image uploads for user profiles and car listings.

## Key Features
- User registration, login, and profile management (including profile picture uploads)
- Role-based access control: `Admin`, `Owner`, `Renter`
- Car listings with availability, pricing, and location
- Booking lifecycle: create, cancel, and manage bookings
- Administrative pages for managing users, cars and bookings
- Background service to update booking statuses
- Email notifications via SendGrid (production) or a local email sender (development)

## Tech stack
- .NET 8 (ASP.NET Core MVC)
- Entity Framework Core (SQL Server supported; migrations included)
- ASP.NET Core Identity for authentication
- SendGrid for production email delivery
- Bootstrap 5 for UI

## Prerequisites
- .NET 8 SDK
- SQL Server (or change connection string to another supported provider)
- (Optional) SendGrid account and API key for production email sending

## Quick start

1. Clone the repo

```powershell
git clone <repo-url>
cd PeerCar
```

2. Configure the connection string

Edit `appsettings.json` (or `appsettings.Development.json`) and set the `DefaultConnection` to point at your SQL Server instance.

3. Create database (apply migrations)

```powershell
dotnet restore
dotnet ef database update --project PeerCar
```

4. Run the application

```powershell
dotnet run --project PeerCar
```

The app will be available at `https://localhost:5001` (or as logged in the console).

## Important notes
- The startup code will automatically apply migrations and seed an admin user (email `YOUSSEF@GMAIL.COM`, password `Admin@123`) if missing. Change or remove default credentials for production use.
- The app creates a small `wwwroot/images/profiles/default.png` placeholder at startup if missing to avoid 404s for default profile images.
- If you see an error about missing columns (e.g., `ProfileImageUrl`), ensure migrations are applied. There are some legacy/empty migrations in the repo; if your database schema is out of sync, you may need to run a corrective migration.

## Development tips
- To disable caching while debugging static files, open DevTools and enable "Disable cache" (Network tab).
- Profile images are stored under `wwwroot/images/profiles/` and the app stores the filename (not full path) in the `AspNetUsers.ProfileImageUrl` column.
- Use `asp-append-version="true"` or a cache-busting query string to ensure clients fetch updated image files after uploads.

## Contributing
- Fork the repository and create a branch for your feature or fix. Open a pull request with a clear description and testing steps.

## License
- Add license information here if applicable.

## Contact
- For questions or help, open an issue in the repository.
