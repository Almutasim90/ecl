# Test database setup for local development

When running locally (`dotnet run` or F5), the app reads the connection string from:

1. **Environment variable** `DATABASE_URL` (recommended for secrets), or  
2. **appsettings.Development.json** → `ConnectionStrings:DefaultConnection`

If the database is unreachable (e.g. your VPS is not reachable from your machine), the app still **starts in Development** and logs a warning so you can work on code; DB-dependent features will fail until the connection works.

---

## Option A: Use a dedicated test Supabase (recommended)

Create a **separate** Supabase project used only for local testing:

1. **Supabase Cloud (free tier)**  
   - Go to [supabase.com](https://supabase.com) → New project.  
   - After it’s created: **Settings → Database** → copy the **Connection string** (URI or “Connection string” with Host/Port/Database/User/Password).  
   - Convert to Npgsql format, e.g.:  
     `Host=db.xxxxx.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=YOUR_PASSWORD`

2. **Self‑hosted Supabase**  
   - Create a second instance (or a separate DB) for testing and get its connection details.  
   - Use the same Npgsql format as above.

3. **Where to put the connection string (do not commit secrets)**  
   - **Preferred:** Create a `.env` in the project root (it’s in `.gitignore`) and set:  
     `DATABASE_URL=Host=...;Port=5432;Database=postgres;Username=postgres;Password=...`  
   - **Alternative:** Use [User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets):  
     `dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=...;Port=5432;..."`  
   - **Optional:** `appsettings.Development.json` — only if you don’t commit it or your repo is private and you accept storing the test DB password there.

4. **Run the app**  
   - `dotnet run` (or run from IDE).  
   - On first run, migrations apply to the test DB.  
   - Share the same `DATABASE_URL` (or connection string) with anyone who needs to run the app locally against this test DB.

---

## Option B: Use your existing VPS Supabase from your PC

If you want to use `supabase.almutasim.site` from your local machine:

1. Ensure the VPS allows **inbound connections to PostgreSQL** (port 5432) from your IP (or from anywhere for testing).  
2. Ensure **pg_hba.conf** allows your client IP and **listen_addresses** is correct (see `CONNECTION_TROUBLESHOOTING.md`).  
3. Put the connection string in `.env` as `DATABASE_URL` or in `appsettings.Development.json` as `ConnectionStrings:DefaultConnection`.  
4. If the host is only reachable via VPN, connect to the VPN before running the app.

---

## Summary

| Goal                         | Action |
|-----------------------------|--------|
| Run app locally, no DB yet  | Use Development: app starts and logs a warning; set `DATABASE_URL` or DefaultConnection when ready. |
| Local testing with a DB     | Create a test Supabase (cloud or self‑hosted), get connection string, set `DATABASE_URL` in `.env` or User Secrets. |
| Use production VPS from PC  | Fix network/firewall/pg_hba so 5432 is reachable, then set connection string in `.env` or appsettings.Development.json. |
