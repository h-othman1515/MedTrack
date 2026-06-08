# MedTrack Jordan — Complete Project Documentation

**Project:** Medication Expiry & Shortage Tracker for Pharmacies  
**Organization:** Orange Internship 2025  
**Stack:** ASP.NET Core 8 MVC · SQLite · ASP.NET Identity · Bootstrap 5 · Chart.js · DataTables · Leaflet · SweetAlert2  
**Local URL:** `http://localhost:5288`  
**Database file:** `medtrack.db` (SQLite, project root)

---

## Table of Contents

1. [Executive Summary](#1-executive-summary)
2. [Demo Login Accounts](#2-demo-login-accounts)
3. [User Roles & Dashboards](#3-user-roles--dashboards)
4. [Everything We Built & Fixed](#4-everything-we-built--fixed)
5. [Sample Seed Data](#5-sample-seed-data)
6. [Project Structure](#6-project-structure)
7. [How to Run](#7-how-to-run)
8. [Troubleshooting](#8-troubleshooting)
9. [Known Limitations vs. Specification](#9-known-limitations-vs-specification)

---

## 1. Executive Summary

MedTrack Jordan is a role-based web platform for Jordan’s pharmaceutical sector. It connects:

- **Community & hospital pharmacies** — inventory, expiry alerts, surplus sharing, transfers  
- **Drug distributors** — restock requests, deliveries, demand analytics  
- **Ministry of Health (MOH)** — national shortage map, analytics, reports  
- **System administrators** — users, pharmacies, drug catalog, broadcast messages, audit logs  

The application was built as an ASP.NET Core 8 MVC project with a unified visual theme, responsive layout, live dashboard data, and demo accounts for testing every role.

---

## 2. Demo Login Accounts

These accounts are **created and repaired automatically** on every app startup by `Data/DemoUserSeeder.cs`. If login fails after a bad seed, restart the app — the seeder fixes missing roles and resets demo passwords.

### Account credentials

| # | Role | Email | Password | Display name | After login you go to |
|---|------|-------|----------|--------------|------------------------|
| 1 | **System Admin** | `admin@medtrack.jo` | `Admin123!` | System Administrator | `/Admin` |
| 2 | **MOH Admin** | `moh@medtrack.jo` | `Moh12345!` | MOH Administrator | `/MOH/Dashboard` |
| 3 | **Pharmacy Manager** | `pharmacy@medtrack.jo` | `Pharm123!` | Al-Shifa Manager | `/Dashboard` |
| 4 | **Distributor** | `distributor@medtrack.jo` | `Dist123!` | Jordan Pharma Distributor | `/Distributor` |

> **Important:** The MOH password is `Moh12345!` (8+ characters). The old password `Moh123!` does **not** work — it was too short for Identity rules.

### Quick copy-paste

```
Admin:      admin@medtrack.jo       / Admin123!
MOH:        moh@medtrack.jo         / Moh12345!
Pharmacy:   pharmacy@medtrack.jo    / Pharm123!
Distributor: distributor@medtrack.jo / Dist123!
```

### How to log in

1. Start the app (see [How to Run](#7-how-to-run))
2. Open **http://localhost:5288/Account/Login**
3. Enter email and password from the table above
4. You are redirected to the dashboard for your role

### Development shortcut (no password)

In **Development** mode only, append `?devRole=...` to any URL:

| Query string | Simulated role |
|--------------|----------------|
| `?devRole=SystemAdmin` | System Admin |
| `?devRole=MOHAdmin` | MOH Admin |
| `?devRole=Distributor` | Distributor |
| `?devRole=PharmacyManager` | Pharmacy Manager |
| `?devRole=PharmacyStaff` | Pharmacy Staff |

Example: `http://localhost:5288/Admin?devRole=SystemAdmin`

---

## 3. User Roles & Dashboards

| Role | Sidebar / nav | Main features |
|------|---------------|---------------|
| **System Admin** | Admin sidebar | Live dashboard stats, manage pharmacies & users, drug catalog, broadcast notifications, audit logs, system settings |
| **MOH Admin** | MOH sidebar | National dashboard, **Leaflet shortage map**, analytics, pharmacy list, reports |
| **Pharmacy Manager / Staff** | Pharmacy sidebar | Inventory, add stock, CSV import, surplus marketplace, alerts, transfers, profile |
| **Distributor** | Distributor sidebar | Restock requests, demand view, deliveries |

### Password policy (Identity)

- Minimum **8 characters**
- Requires at least one **digit** and one **lowercase** letter
- Uppercase and special characters are **not** required

---

## 4. Everything We Built & Fixed

### 4.1 Global UI & visual theme

- Unified **MedTrack home theme** across all pages using `home.css`, `layout.css`, and `app-theme.css`
- Role-based sidebars: Admin, MOH, Distributor, Pharmacy (in `_Layout.cshtml`)
- Shared page header partial: `Views/Shared/_PageHeader.cshtml`
- Main content centered with `max-width: 1280px` on `.mt-page` and app footer
- Account pages (Login, Register, Profile, Settings, Change Password) match the home theme
- All admin CRUD views and sidebar-linked pages styled consistently
- Floating pill-field background on public and app pages
- **Dark / light mode** toggle on home nav, login, public pages, and authenticated topbar
  - Persisted in `localStorage` under key `mt-theme`
  - Global script: `wwwroot/js/theme.js`
  - Shared button partial: `Views/Shared/_ThemeToggle.cshtml`

### 4.2 SweetAlert2 (system messages)

- Local bundle: `wwwroot/lib/sweetalert2/`
- Helpers: `wwwroot/js/swal-helpers.js` — `showToast`, `mtConfirm`, `mtAlert`
- Partial: `Views/Shared/_SweetAlertMessages.cshtml` (TempData → toast notifications)
- Themed popup styles in `app-theme.css`
- Admin pages use Swal confirmations instead of native browser `confirm()` dialogs

### 4.3 Alerts module

- **Mark all read** and **Clear acknowledged** on Alerts index
- `AlertService` extended for bulk operations and restock/transfer alerts
- Alerts grouped by severity with themed cards

### 4.4 Inventory & auto-restock

- `InventoryService` automatically creates **RestockRequest** when quantity falls below minimum stock
- Inventory list, create, edit, and CSV import views connected to services

### 4.5 Dashboards & live MOH data

- **Admin dashboard** (`/Admin`) — live DB counts: users, pharmacies, drugs, low stock, transfers
- **MOH dashboard** and **Shortages** — live data from `DashboardService`
- **Leaflet map** on MOH Shortages page with real shortage hotspot data via `medtrack.js`
- **Distributor dashboard** — restock lists and demand data from `DashboardService`

### 4.6 Admin broadcast (Notifications)

- Restyled compose section: audience tiles, governorate pills, channel pills, template buttons
- Recent broadcast feed with severity-colored items
- CSS in `app-theme.css`: `.mt-audience-tile`, `.mt-gov-pill`, `.mt-channel-pill`, etc.

### 4.7 Sidebar & layout

- **Desktop:** sidebar always visible (fixed, no collapse)
- **Mobile / tablet (≤992px):** hamburger menu opens drawer; tap overlay or **×** to close
- Fixed **z-index** so mobile drawer appears above content (was previously hidden behind main area)
- Sidebar links auto-close drawer on navigation (mobile)

### 4.8 Responsive design (all devices)

- Fixed broken CSS media queries: `@media(max - width:…)` → `@media (max-width: …)` in `home.css` and `layout.css`
- New `wwwroot/css/responsive.css` (loaded last):
  - Mobile sidebar drawer + backdrop overlay
  - Horizontal scroll for tables and DataTables
  - Stacked page headers, card headers, and button groups on phones
  - Home page hero, stats grid, footer reflow
  - Map/chart flexible sizing, SweetAlert width, safe-area padding for notched phones

### 4.9 Background services

- `ExpiryAlertBackgroundService` — scans medication batches nearing expiry on a schedule

### 4.10 Authentication & demo user seeding (fixes)

**Problem:** Only `admin@medtrack.jo` worked; other accounts failed login.

**Causes found:**
1. Role assignment failed during first seed (`FOREIGN KEY` error) — users existed without roles
2. Users without roles were sent to `/Dashboard`, which requires `PharmacyId` → redirect loop to login
3. MOH password `Moh123!` was only 7 characters (below 8-char minimum)

**Fixes applied:**
- New `Data/DemoUserSeeder.cs` — runs every startup, creates missing users, **assigns missing roles**, resets demo passwords, links pharmacy user to Al-Shifa Pharmacy
- `AccountController.Login` — checks for empty roles and shows a clear error message
- MOH password changed to **`Moh12345!`**

### 4.11 Theme toggle fix (home page)

**Problem:** Clicking the theme toggle on the home page appeared to do nothing.

**Cause:** Duplicate JavaScript click handlers toggled light → dark → light in one click.

**Fix:**
- Single handler in `wwwroot/js/theme.js` using event delegation
- Shared `_ThemeToggle.cshtml` partial used on all pages
- Removed duplicate handlers from `Home/Index.cshtml` and inline `_Layout` script

---

## 5. Sample Seed Data

On first run, `Program.cs` also seeds business data:

| Data | Details |
|------|---------|
| **Drugs (8)** | Amoxicillin, Metformin, Paracetamol, Atorvastatin, Omeprazole, Insulin Glargine, Salbutamol, Vitamin D3 |
| **Pharmacies (2)** | Al-Shifa Pharmacy (Amman), Zarqa Medical Pharmacy (Zarqa) |
| **Medication batches** | Sample stock with varied expiry dates and quantities |
| **Surplus post** | 1 available listing |
| **Transfer request** | 1 pending transfer between pharmacies |

---

## 6. Project Structure

```
LastProject/
├── Controllers/           Admin, MOH, Distributor, Dashboard, Inventory, Alerts, …
├── Services/              InventoryService, AlertService, DashboardService, SurplusService
├── Models/                ApplicationUser, Pharmacy, Drug, MedicationBatch, …
├── ViewModels/            Per-page view models
├── Views/                 Razor views by controller + Shared partials
├── Data/
│   ├── ApplicationDbContext.cs
│   ├── DemoUserSeeder.cs  ← demo accounts (active)
│   └── SeedData.cs        ← legacy (not used at runtime)
├── Background/            ExpiryAlertBackgroundService
├── Middleware/            DevelopmentAuthMiddleware
├── wwwroot/
│   ├── css/               home.css, layout.css, app-theme.css, responsive.css
│   └── js/                theme.js, medtrack.js, swal-helpers.js
├── Program.cs             Startup, DI, Identity, data seeding
├── medtrack.db            SQLite database
├── medtrack.md            Original project specification
└── PROJECT_IMPLEMENTATION.md  ← this document
```

### Key shared files

| File | Purpose |
|------|---------|
| `Views/Shared/_Layout.cshtml` | Master layout, sidebars, topbar, scripts |
| `Views/Shared/_ThemeToggle.cshtml` | Dark/light toggle button |
| `Views/Shared/_SweetAlertMessages.cshtml` | TempData → SweetAlert toasts |
| `Views/Shared/_PageHeader.cshtml` | Consistent page titles |
| `wwwroot/css/app-theme.css` | Dashboard cards, tables, broadcast, Swal theme |
| `wwwroot/css/responsive.css` | Mobile/tablet overrides |
| `wwwroot/js/theme.js` | Global theme toggle |
| `wwwroot/js/swal-helpers.js` | Toast and confirm helpers |

---

## 7. How to Run

```powershell
cd c:\Users\User\Desktop\hanan\LastProject
dotnet run --urls "http://localhost:5288"
```

Then open **http://localhost:5288** in your browser.

### Build notes

- If build fails with **MSB3027** (file locked), stop the running app first:
  ```powershell
  Stop-Process -Name "MedTrack" -Force -ErrorAction SilentlyContinue
  ```
- After CSS/JS changes, hard-refresh the browser: **Ctrl+Shift+R**

---

## 8. Troubleshooting

| Issue | Solution |
|-------|----------|
| Only admin login works | Restart app — `DemoUserSeeder` repairs roles and passwords |
| MOH login fails | Use `Moh12345!` (not `Moh123!`) |
| Login succeeds then returns to login page | User had no role or no PharmacyId — restart app to re-seed |
| Theme toggle does nothing | Hard-refresh; ensure button has `data-mt-theme-toggle` (not old `id="themeToggle"` only) |
| Mobile sidebar not visible | Hard-refresh; use ☰ menu; drawer should appear above dimmed overlay |
| Old styles showing | Ctrl+Shift+R or try incognito window |
| Port already in use | Stop existing MedTrack/dotnet process or change port in run command |

---

## 9. Known Limitations vs. Specification

The full business proposal is in `medtrack.md`. Current implementation status:

| Feature | Status |
|---------|--------|
| Inventory, expiry alerts, surplus, transfers | ✅ Implemented |
| Role-based dashboards (Admin, MOH, Pharmacy, Distributor) | ✅ Implemented |
| MOH shortage map (Leaflet) | ✅ Live data |
| Auto restock requests | ✅ Implemented |
| Responsive UI | ✅ Implemented |
| SweetAlert notifications | ✅ Implemented |
| SQLite database | ✅ In use (spec suggests SQL Server for production) |
| SMS / email notifications | ⚠️ UI only; not fully wired |
| JWT / REST API | ❌ Not implemented |
| Automated tests | ❌ Not implemented |

---

*MedTrack Jordan — Orange Internship 2025 · Built with ASP.NET Core 8*
