# Inventory-Manager (EIMS) — Enterprise Inventory & Warehouse ERP

> A production-grade Enterprise Inventory Management System (EIMS) architected with **Clean Architecture**, a high-performance **EF Core + Dapper hybrid data layer**, **PostgreSQL**, and a responsive React management console.

[![.NET 9](https://img.shields.io/badge/.NET-9.0%20%2F%208.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-Web_API-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/apps/aspnet)
[![Clean Architecture](https://img.shields.io/badge/Architecture-Clean%20Architecture-blueviolet?style=for-the-badge)](#system-architecture)
[![Hybrid ORM](https://img.shields.io/badge/Data_Layer-EF_Core%20%2B%20Dapper-00599C?style=for-the-badge)](https://docs.microsoft.com/ef/core/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16.0+-316192?style=for-the-badge&logo=postgresql&logoColor=white)](https://www.postgresql.org/)
[![React](https://img.shields.io/badge/React-18.0+-20232A?style=for-the-badge&logo=react&logoColor=61DAFB)](https://react.dev/)
[![License](https://img.shields.io/badge/License-MIT-blue?style=for-the-badge)](#license)

---

## 📖 1. Overview

**Inventory-Manager (EIMS)** is an enterprise equipment and warehouse inventory platform engineered to handle complex supply chain operations: goods receipt (import), outbound shipments (export), inter-warehouse transfers, and periodic physical count reconciliations.

The system addresses the dual demands of enterprise inventory software:
1. **Strict Data Integrity**: Guaranteed ACID transactional safety and optimistic concurrency protection during stock movement.
2. **Sub-Millisecond Reporting**: High-speed analytical queries across millions of historical movement logs without degrading transactional throughput.

---

## ✨ 2. Architectural Highlights & Engineering Innovations

### ⚡ 1. Hybrid ORM Data Access Strategy (EF Core + Dapper)
- **EF Core for Transactional Writes**: All state mutations (`Import`, `Export`, `Transfer`, `Reconciliation`) leverage Entity Framework Core transactions to guarantee atomicity and change-tracking consistency.
- **Dapper for High-Throughput Reporting**: Complex aggregation queries (e.g., Stock Valuation, Movement History, Replenishment Analysis) bypass EF Core change tracking and execute via raw, parameterized SQL using **Dapper**, achieving near-zero object mapping overhead.

### 🛡️ 2. Optimistic Concurrency & Lost Update Prevention
- Employs PostgreSQL `RowVersion` concurrency tokens across inventory records. If two warehouse operators attempt to adjust the same SKU simultaneously, the second write is rejected with a concurrency conflict, preventing phantom stock calculations.

### 📝 3. Automated Audit Trail & Change Interception
- Integrated database interceptors capture every inventory operation: User ID, Timestamp, Action Type, Previous Balance, and New Balance, ensuring total compliance with financial and auditing standards.

### 🔔 4. Proactive Low-Stock Sentinel Service
- Runs an asynchronous ASP.NET Core `BackgroundService` that periodically checks warehouse stock against predefined reorder thresholds (`SafetyStockLevel`) and automatically dispatches alert records to warehouse managers.

### 🏢 5. Multi-Tier Warehouse & Product Hierarchy
- Supports hierarchical category trees (parent-child classifications) and multi-warehouse physical bin tracking.
- Memory-cached master data layer (`IMemoryCache`) minimizes redundant database roundtrips for stable reference datasets.

---

## 🏗️ 3. System Architecture

```text
Inventory-Manager/
├── Eims.slnx                           # Modern XML .NET Solution File
├── backend/
│   └── src/
│       ├── Eims.Domain/                # Pure business logic, entities, enums, concurrency base
│       │   ├── Common/                 # BaseEntity, AuditableEntity, IAggregateRoot
│       │   ├── Entities/               # Product, Category, Warehouse, StockItem, StockMovement
│       │   └── Enums/                  # MovementType (Import, Export, Transfer, Reconcile), UserRole
│       ├── Eims.Application/           # DTOs, service interfaces, validation contracts
│       │   ├── DTOs/                   # Request/Response data transfer objects
│       │   └── Services/               # IAuthService, IInventoryService, IReportService
│       ├── Eims.Infrastructure/        # EF Core DbContext, Dapper queries, JWT, Background workers
│       │   ├── Data/                   # EimsDbContext, EntityTypeConfigurations, Migrations
│       │   ├── Reports/                # Dapper-optimized analytical query implementations
│       │   └── Workers/                # LowStockMonitoringBackgroundService
│       └── Eims.Api/                   # ASP.NET Core presentation layer
│           ├── Controllers/            # AuthController, InventoryController, ReportsController
│           └── Middleware/             # GlobalExceptionMiddleware, JwtAuthorizationMiddleware
└── frontend/                           # React + TypeScript + Vite management console
    ├── src/
    │   ├── components/                 # Data tables, stock adjustment forms, navigation
    │   ├── pages/                      # Dashboard, Inventory List, Stock Movements, Reports
    │   └── services/                   # Axios HTTP API client
    ├── package.json
    └── vite.config.ts
```

---

## 🔐 4. Role-Based Access Control (RBAC) Matrix

| Operation | Staff | Manager | Admin |
|---|:---:|:---:|:---:|
| View Stock Levels & Catalog | ✅ | ✅ | ✅ |
| Execute Import / Export Operations | ✅ | ✅ | ✅ |
| Initiate Inter-Warehouse Transfers | ❌ | ✅ | ✅ |
| Perform Stock Reconciliation (Audit Adjustments) | ❌ | ✅ | ✅ |
| Generate Valuation & Movement Reports | ❌ | ✅ | ✅ |
| Manage Master Data (Categories, Warehouses) | ❌ | ❌ | ✅ |
| User & System Administration | ❌ | ❌ | ✅ |

---

## 📡 5. REST API Reference

### Authentication
- `POST /api/auth/login` — Authenticate and receive JWT bearer token

### Inventory Operations
- `GET /api/inventory/stock` — Paginated list of current stock items across all warehouses
- `POST /api/inventory/import` — Process inbound goods receipt into designated warehouse
- `POST /api/inventory/export` — Process outbound shipment with stock deduction
- `POST /api/inventory/transfer` — Execute atomic transfer between source and target warehouses
- `POST /api/inventory/reconcile` — Record physical count discrepancy adjustments

### Reports & Analytics (Dapper-Powered)
- `GET /api/reports/movement-history` — Filterable audit trail of all historical movements
- `GET /api/reports/stock-valuation` — Real-time monetary valuation by warehouse and category

### Master Data Management
- `GET|POST /api/master-data/categories` — Category taxonomy management
- `GET|POST /api/master-data/products` — Product SKU catalog management
- `GET|POST /api/master-data/warehouses` — Physical warehouse facility management

---

## 🚀 6. Getting Started & Local Setup

### Prerequisites
- [.NET 9 or .NET 8 SDK](https://dotnet.microsoft.com/download)
- [PostgreSQL 15+](https://www.postgresql.org/) database
- [Node.js (v18+)](https://nodejs.org/)

### 1. Backend Setup

Set your local database connection and signing key in environment variables (or update `appsettings.Development.json`):

```powershell
$env:ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=eims_db;Username=postgres;Password=your_password"
$env:Jwt__SigningKey="YourSuperSecretJWTKeyWithAtLeast32CharactersLength!"
$env:SeedAdmin__Password="SecureAdminPassword123!"
```

Restore dependencies and start the API server:

```powershell
dotnet restore Eims.slnx
dotnet run --project backend/src/Eims.Api/Eims.Api.csproj --urls http://localhost:5000
```

> **Automatic Schema Migration & Seed:** On first startup, the API automatically verifies database connectivity, applies EF Core migrations, and seeds an initial `admin` account with the password configured in `SeedAdmin__Password`.

### 2. Frontend Setup

Open a new terminal session and navigate to the frontend directory:

```powershell
cd frontend
npm install
npm run dev
```

Open `http://localhost:5173` in your browser and sign in using your credentials.

---

## 📄 License

This project is licensed under the [MIT License](LICENSE).
Author: [Le Quoc Khanh](https://github.com/lqkhanh295).
