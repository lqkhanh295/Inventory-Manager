# Inventory-Manager

Clean Architecture sample for an enterprise inventory backend with PostgreSQL and a dependency-free frontend console.

## Structure

- `backend/src/Eims.Domain`: entities, enums, audit/concurrency base model.
- `backend/src/Eims.Application`: DTOs and service contracts.
- `backend/src/Eims.Infrastructure`: EF Core PostgreSQL, Dapper reports, password hashing, JWT creation, background low-stock monitor.
- `backend/src/Eims.Api`: ASP.NET Core API, JWT/RBAC, global exception handling.
- `frontend`: React + Vite console with a minimal stylesheet.

## Backend

Set local PostgreSQL/JWT configuration with environment variables before running the API:

```powershell
$env:ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=eims;Username=postgres;Password=<your-local-password>"
$env:Jwt__SigningKey="<a-long-random-local-signing-key>"
$env:SeedAdmin__Password="<your-local-admin-password>"
```

Run:

```powershell
dotnet restore Eims.slnx
dotnet run --project backend/src/Eims.Api/Eims.Api.csproj --urls http://localhost:5000
```

On first run the API creates schema/tables and seeds:

```txt
username: admin
password: value from `SeedAdmin__Password`
```

Useful endpoints:

- `POST /api/auth/login`
- `GET /api/inventory/stock`
- `POST /api/inventory/import`
- `POST /api/inventory/export`
- `POST /api/inventory/transfer`
- `POST /api/inventory/reconcile`
- `GET /api/reports/movement-history`
- `GET /api/reports/stock-valuation`
- `GET|POST /api/master-data/categories`
- `GET|POST /api/master-data/products`
- `GET|POST /api/master-data/warehouses`

## Frontend

Run:

```powershell
cd frontend
npm install --cache .npm-cache
npm run dev
```

Open `http://127.0.0.1:5173` and login with `admin` plus the value from `SeedAdmin__Password`.

Production preview:

```powershell
npm run build
npm run preview
```

Open `http://127.0.0.1:4173`. The React app must be served by Vite; do not open `frontend/index.html` directly through `file://`.

## Implemented SRS Highlights

- JWT login and role-based authorization for Admin, Manager, Staff.
- Master data for category, product, warehouse with parent-child structure.
- Import/export/transfer/reconciliation with EF Core transactions.
- Optimistic locking through `RowVersion` concurrency tokens.
- Automatic audit logging from EF Core change tracking.
- Low-stock background monitor writing notification records.
- Real-time paged inventory API.
- Dapper-based movement history and stock valuation reports.
- MemoryCache for product master data.
