# EasyGames: An ASP.NET Core 8 MVC E-commerce Sample

This project is a compact e-commerce application built with **ASP.NET Core 8 MVC**, designed to showcase best practices for building a modern web store for books, games, and toys.

### Key Features

- **ASP.NET Core 8:** Utilizes the latest Long-Term Support (LTS) version of the framework.
- **Entity Framework Core:** Includes committed database migrations, providing a ready-to-use schema out of the box.
- **ASP.NET Core Identity:** Manages user authentication and authorization with distinct **Owner** and **Customer** roles.
- **Session-Based Cart:** A straightforward shopping cart that works for both anonymous and logged-in users.
- **Automated Seeding:** Populates the product catalog from `wwwroot/data/products.json` using an **idempotent upsert** method, ensuring the database is always consistent.
- **Static Content:** Demo images are committed to `wwwroot/images/` so the site is fully functional on any machine without extra setup.
- **Clean UI:** Features a two-row navbar, hero slider, and a well-organized product catalog.

---

## 1) Getting Started

### Prerequisites

- **.NET 8 SDK**
- **SQL Server LocalDB** (VS installer) or **SQL Express**
- Visual Studio 2022 (recommended) or the `dotnet` CLI

### Quick Start

1.  **Configure the Connection String:** In `appsettings.json`, set the `DefaultConnection` to your SQL Server instance.
    ```json
    {
      "ConnectionStrings": {
        "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=EasyGames;Trusted_Connection=True;MultipleActiveResultSets=true",
        "SqlExpressConnection": "Server=.\\SQLEXPRESS;Database=EasyGames;Trusted_Connection=True;MultipleActiveResultSets=true"
      }
    }
    ```
2.  **Run the application:**
    - **Visual Studio:** Open the solution and press **F5**.
    - **CLI:** Navigate to the project directory and run `dotnet run`.

The first run will automatically apply database migrations and seed the catalog.

### Default Owner Account

| Field    | Value            |
| :------- | :--------------- |
| **Email** | `owner@easygames.local` |
| **Password** | `Owner#123`      |

---

## 2) Project Walkthrough

### Public-Facing Features
- **Home:** Displays products grouped by category (Books, Games, Toys).
- **Shop:** A product grid with filtering options.
- **Cart:** Allows users to add, remove, and update product quantities. A simulated checkout process is available for logged-in users.

### Admin Features (Owner)
- **Manage Stock:** The `/Products` page provides a full **CRUD** (Create, Read, Update, Delete) interface for managing the product catalog. The table includes live search and sticky headers for a polished user experience.

### File Structure

```bash
EasyGames/
├── Controllers/
├── Data/
│   ├── ApplicationDbContext.cs
│   └── Migrations/
├── Models/
├── Services/
├── Views/
│   ├── Shared/
│   ├── Home/
│   ├── Shop/
│   ├── Cart/
│   └── Products/
└── wwwroot/
    ├── images/
    │   ├── hero/
    │   └── products/ # Committed demo images
    ├── uploads/products/ # User uploads (ignored by Git)
    └── data/products.json # Seed data source

### 3) Technical Details
Database & Migrations
Migrations are committed to the Data/Migrations/ folder.

Run Update-Database (Package Manager Console) or dotnet ef database update (CLI) to apply them manually.

Money fields use decimal(18,2) to prevent precision errors.

Seed Data & Images
The IdentitySeed.cs class is responsible for creating roles, the default owner, and populating the product catalog from products.json.

Products are upserted by their Name field, making it easy to update the catalog by simply modifying the JSON file.

Image Uploads
The owner can upload new images through the product management page.

Uploaded images are saved to wwwroot/uploads/products/ with GUID filenames.

The system handles various modern image formats like .webp and .avif.

4) Configuration & Troubleshooting
Middleware Order
A correct middleware pipeline is essential for the application to function properly. The order is:

Nginx

UseHttpsRedirection
UseStaticFiles
UseRouting
UseSession
UseAuthentication
UseAuthorization
MapControllerRoute / MapRazorPages
Common Issues
View not found: Ensure view files exist in the correct Views subdirectory.

Images not showing: Verify that paths in products.json point to the committed /images/products/ directory.

5) GitHub & License
GitHub Basics
Recommended .gitignore entries:

Bash

.vs/
bin/
obj/
*.user
*.suo
*.mdf
*.ldf
wwwroot/uploads/
*.log
The repository should include Data/Migrations/, wwwroot/images/**, and wwwroot/data/products.json.

License
This project is intended for educational and coursework purposes. Demo images and product text are placeholders.