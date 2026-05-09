# Database Connection — Self-Hosted Supabase on VPS

This document describes **all required information** to connect the ECL application to a PostgreSQL database (self-hosted Supabase) on your VPS.

**Local development:** Prefer the **SSH tunnel** (no public DB exposure). **If you must expose the DB:** follow **[SUPABASE_PUBLIC_ACCESS.md](SUPABASE_PUBLIC_ACCESS.md)** for security warnings and hardened steps (firewall, `listen_addresses`, `pg_hba.conf`, IP restriction).

---

## 1. PostgreSQL (Database) — Required for app runtime

The app uses **Entity Framework Core** with **Npgsql** to connect to PostgreSQL. This is the primary database for listening/reading questions and migrations.

| Parameter   | Description                    | Example / Notes |
|------------|--------------------------------|-----------------|
| **Host**   | VPS hostname or IP             | `supabase.almutasim.site` or `62.169.27.210` |
| **Port**   | PostgreSQL port                | `5432` (direct) or `6543`/`54322` if using Supabase pooler |
| **Database** | Database name                | Usually `postgres` for Supabase |
| **Username** | PostgreSQL user              | e.g. `postgres` |
| **Password** | PostgreSQL password          | From your Supabase/Postgres setup |

**Connection string format (Npgsql) — no SSL (default for this setup):**

```text
Host=YOUR_HOST;Port=5432;Database=postgres;Username=postgres;Password=YOUR_PASSWORD
```

**Optional (only if your server uses SSL):** append `;SSL Mode=Require;Trust Server Certificate=true`

**Where to configure:**

- **Option A — Environment (recommended for production):**  
  Set `ConnectionStrings__DefaultConnection` to the full string, or set `DATABASE_URL` (the app uses `DATABASE_URL` if set).
- **Option B — Config files:**  
  `appsettings.json` or `appsettings.Development.json` under `ConnectionStrings:DefaultConnection`.
- **Option C — Local-only dev config (recommended for your machine):**  
  Create `appsettings.Development.local.json` (this repo ignores `appsettings.*.local.json`). Put secrets there.
- **Never** commit real passwords to git; use `.env` (from `.env.example`) and exclude `.env` from version control.

---

## 2. Storage (public URL) — For audio/assets

The app builds public storage URLs for assets (e.g. listening audio). Currently the base URL is in code.

| Parameter | Description | Example |
|-----------|-------------|---------|
| **Storage base URL** | Public base for Supabase Storage | `https://supabase.almutasim.site/storage/v1/object/public` |

If you use a different Supabase URL, this should be configurable (e.g. via `SUPABASE_URL` or a dedicated `STORAGE_PUBLIC_URL`). See `Models/ListeningQuestion.cs` for the current `AudioPath` logic.

---

## 3. Server-side checklist (on the VPS)

For the app to connect to PostgreSQL on your VPS, ensure:

1. **pg_hba.conf** allows your app’s IP (or `0.0.0.0/0` for testing):  
   `host  all  all  0.0.0.0/0  md5`
2. **postgresql.conf**: `listen_addresses = '*'`
3. **Firewall:** Port `5432` (or your chosen port) open for the app server.
4. **Reload/restart** PostgreSQL after config changes.

See `CONNECTION_TROUBLESHOOTING.md` for details and pooler options. This setup uses **no SSL**; omit SSL parameters in the connection string.

---

## Summary: minimum required for “application connected to database”

| # | What | Required? | Configured via |
|---|------|-----------|-----------------|
| 1 | PostgreSQL Host, Port, Database, Username, Password | **Yes** | `ConnectionStrings:DefaultConnection` or `DATABASE_URL` |
| 2 | Storage base URL | If you serve audio/assets from Supabase | Code or config (e.g. `SUPABASE_URL`) |

The app supports **optional student login** (to save progress) and **admin login** (to manage content). The **minimum** to run is still a valid **PostgreSQL connection string** (Host, Port, Database, Username, Password) via `ConnectionStrings:DefaultConnection` or `DATABASE_URL`.
