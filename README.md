# AutoGarage Backend

A garage management REST API built with ASP.NET Core Web API (.NET 10).

## Tech Stack

- ASP.NET Core Web API (.NET 10)
- Entity Framework Core
- SQL Server
- ASP.NET Core Identity
- JWT Authentication + Refresh Tokens
- Serilog Logging

## Prerequisites

Make sure you have these installed:

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [SQL Server Express](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (free)
- [SQL Server Management Studio - SSMS](https://learn.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms) (optional but recommended)

## Getting Started

### Step 1: Clone the repo

git clone https://github.com/srinivasulu956/auto-garage-backend.git

### Step 2: Create appsettings.Development.json

Inside the `Auto-Garage` folder, create a file called
`appsettings.Development.json` and paste the content
shared by the project owner privately.

It should look like this:
{
"ConnectionStrings": {
"AutoGarageDbConnection": "Server=.;Database=AutoGarage;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;",
"AutoGarageAuthDbConnection": "Server=.;Database=AutoGarageAuth;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;"
},
"Jwt": {
"Key": "GET THIS FROM PROJECT OWNER",
"Issuer": "http://localhost:7224/",
"Audience": "http://localhost:7224/"
}
}

### Step 3: Run Migrations

Open terminal inside the Auto-Garage folder and run:
dotnet ef database update

This will automatically create the databases and
all tables on your local SQL Server.

### Step 4: Run the project

dotnet run

API runs at: https://localhost:7224
Swagger UI: https://localhost:7224/swagger

## Project Roles

| Role     | Access                            |
| -------- | --------------------------------- |
| Admin    | Manage users, bookings, mechanics |
| Customer | Book services, manage vehicles    |
| Mechanic | View and update assigned jobs     |

## Branching Strategy

- `main` — stable production code
- `develop` — active development branch
- `feature/*` — individual feature branches

Always raise a PR to `develop`. Never push directly to
`main` or `develop`.

## Setup - Default Admin

After running migrations, execute the seed script:

1. Open SSMS
2. Connect to your SQL Server
3. Open `seed-admin.sql`
4. Click Execute

Default credentials:

- Email: admin@gmail.com
- Password: Admin956956@ag

# Setting Default admin login to access the application by logging

-- ================================================================
-- AutoGarage - Default Admin Seed Script
-- Run this on AutoGarageAuth database after running migrations
-- ================================================================
-- Default Admin Credentials:
-- Email : admin@gmail.com
-- Password: Admin956956@ag
-- ================================================================

USE AutoGarageAuth;

-- Step 1: Insert Roles (skip if already exists)
IF NOT EXISTS (SELECT 1 FROM [dbo].[AspNetRoles] WHERE [Name] = 'Admin')
BEGIN
INSERT INTO [dbo].[AspNetRoles] (Id, Name, NormalizedName, ConcurrencyStamp)
VALUES (
'd6a029f6-f39e-434c-a72b-171ef7d2560d',
'Admin',
'ADMIN',
NEWID()
)
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[AspNetRoles] WHERE [Name] = 'Customer')
BEGIN
INSERT INTO [dbo].[AspNetRoles] (Id, Name, NormalizedName, ConcurrencyStamp)
VALUES (
'c07538fb-40fc-4ba7-ade6-1e4450f78129',
'Customer',
'CUSTOMER',
NEWID()
)
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[AspNetRoles] WHERE [Name] = 'Mechanic')
BEGIN
INSERT INTO [dbo].[AspNetRoles] (Id, Name, NormalizedName, ConcurrencyStamp)
VALUES (
'09cd9546-8864-49f4-8771-51385d68edba',
'Mechanic',
'MECHANIC',
NEWID()
)
END

-- Step 2: Insert Default Admin User (skip if already exists)
IF NOT EXISTS (SELECT 1 FROM [dbo].[AspNetUsers] WHERE [Email] = 'admin@gmail.com')
BEGIN
DECLARE @UserId NVARCHAR(450) = NEWID()

    INSERT INTO [dbo].[AspNetUsers] (
        [Id], [FirstName], [LastName], [IsActive], [IsDeleted],
        [CreatedAt], [ModifiedAt], [UserName], [NormalizedUserName],
        [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash],
        [SecurityStamp], [ConcurrencyStamp], [PhoneNumber],
        [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd],
        [LockoutEnabled], [AccessFailedCount], [ThemePreference]
    )
    VALUES (
        @UserId,
        'Default',
        'Admin',
        1,
        0,
        GETUTCDATE(),
        GETUTCDATE(),
        'admin@gmail.com',
        'ADMIN@GMAIL.COM',
        'admin@gmail.com',
        'ADMIN@GMAIL.COM',
        1,
        'AQAAAAIAAYagAAAAECro16mHMvRLigl5CaW6EyUcG9FiXUEccRLqC0X1PT5QeAWAwP1MLebnNK24tREg3g==',
        'ZEEUSL2M7F2R3YK32ZLST26H4EJ6OTZ7',
        NEWID(),
        NULL,
        0,
        0,
        NULL,
        1,
        0,
        'light'
    )

    -- Step 3: Assign Admin Role to the user
    INSERT INTO [dbo].[AspNetUserRoles] (UserId, RoleId)
    VALUES (
        @UserId,
        'd6a029f6-f39e-434c-a72b-171ef7d2560d'
    )

    PRINT 'Default admin user created successfully!'

END
ELSE
BEGIN
PRINT 'Admin user already exists. Skipping.'
END
