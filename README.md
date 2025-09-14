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
│
├── Controllers/
│   ├── HomeController.cs          # grouped Home sections
│   ├── ShopController.cs          # list/filter + details
│   ├── CartController.cs          # session cart + checkout (sim)
│   └── ProductsController.cs      # [Authorize(Roles="Owner")] CRUD
│
├── Data/
│   ├── ApplicationDbContext.cs
│   ├── IdentitySeed.cs            # roles + owner + products from JSON (upsert)
│   └── Migrations/                # EF migrations (committed)
│
├── Models/
│   ├── ApplicationUser.cs
│   ├── Product.cs
│   ├── Order.cs, OrderItem.cs
│   └── (optional) ViewModels (e.g., HomeCatalogVM.cs)
│
├── Services/
│   ├── ICartService.cs
│   └── CartService.cs             # wraps Session
│
├── Views/
│   ├── Shared/
│   │   ├── _Layout.cshtml
│   │   └── _HeroSlider.cshtml
│   ├── Home/Index.cshtml
│   ├── Shop/Index.cshtml, Details.cshtml
│   ├── Cart/ViewCart.cshtml, Checkout.cshtml
│   └── Products/*.cshtml
│
└── wwwroot/
    ├── images/
    │   ├── hero/hero-1.jpg|hero-2.jpg|hero-3.jpg
    │   └── products/*                 # committed demo images
    ├── uploads/products/              # runtime uploads (gitignored)
    └── data/products.json             # seed source (upsert)
```
### 3) Seed data & images (examiner-proof)

The catalog ships in wwwroot/data/products.json.

Product images for seeding live in wwwroot/images/products/** (committed to the repo).

At startup, seeding upserts by Name:

Not found → Add

Found → Update Category, Price, Stock, ImageUrl, Description.

To change the demo data: edit the JSON, restart the app (no migration needed).

Tip: Keep Name stable (or introduce an SKU if you plan to rename products).

### 4) Database & migrations

Migrations are committed under Data/Migrations/.

On first run, the app applies migrations automatically (if Program.cs enables it) or run manually:

Package Manager Console (VS):
Update-Database

CLI:
```bash
dotnet tool install -g dotnet-ef   # first time
dotnet ef database update
```
When schema changes (e.g., new fields or decimal precision), create a migration:
Add-Migration <Name> → Update-Database

Money fields use decimal(18,2) to prevent truncation:

Product.Price

OrderItem.UnitPrice

Order.Total

### 5)License
This project is intended for educational and coursework purposes. Demo images and product text are placeholders.