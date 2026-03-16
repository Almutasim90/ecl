# Connecting to Supabase from development (Coolify)

---

## ⚠️ CRITICAL SECURITY WARNING — READ FIRST

**Exposing PostgreSQL directly to the internet is extremely risky.** You are opening your database to:

- **Brute-force password attacks** (automated bots scan port 5432 constantly)
- **Data exfiltration or ransomware** if credentials leak
- **Exploits** targeting known PostgreSQL vulnerabilities

**The safer alternative (recommended):**  
Keep port 5432 **closed** and use the **SSH tunnel** method for local development. Your app in Coolify already connects internally via `supabase-db`. No public exposure needed.

**If you still choose to expose PostgreSQL publicly,** follow the hardened steps below precisely. Do not skip security steps.

---

## ✅ Prerequisites

- SSH access to your VPS: `root@62.169.27.210` (or your VPS IP)
- Coolify admin access
- Your Supabase postgres password (or plan to change it)

**Note:** This guide uses **no SSL** for the database connection. Do not add `SSL Mode=Require` to the connection strings unless your server is configured for SSL.

---

# 🔒 Option A: SSH tunnel (recommended — no public exposure)

No firewall changes, no `pg_hba.conf` edits, same codebase for dev and prod.

### 1. Start the tunnel (keep this terminal open while developing)

```bash
ssh -L 5432:supabase-db:5432 root@62.169.27.210
```

If your Supabase DB container has a different name in Docker, use that instead of `supabase-db` (e.g. the service name from your Supabase stack in Coolify).

### 2. Local connection string (Development)

In **appsettings.Development.json** or **.env** (`DATABASE_URL`):

```text
Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=YOUR_PASSWORD
```

### 3. Production (Coolify) — internal network only

In **Coolify → Your ASP.NET App → Environment Variables**:

```text
DATABASE_URL=Host=DB_SERVICE_NAME;Port=5432;Database=postgres;Username=postgres;Password=YOUR_PASSWORD
```

(or use `ConnectionStrings__DefaultConnection` with the same value).

- **Host:** Use the **Docker service name** of the Supabase PostgreSQL container on the same network as your app. It might be `supabase-db`, `db`, or a project-prefixed name — check your Supabase resource in Coolify (stack/compose) for the real service name. If the app cannot resolve the hostname, see **CONNECTION_TROUBLESHOOTING.md** (“Running in Docker / Coolify”).
- Port 5432 is never exposed to the internet.

---

# 🌐 Option B: Expose Supabase PostgreSQL publicly (use only if required)

If you must allow direct connections from the internet (e.g. a third-party service), follow these steps and **restrict access by IP** where possible.

---

## Step B1: Expose port 5432 in Coolify (Docker port mapping)

Supabase runs in Docker via Coolify; you must map the container port to the host.

**Option B1a — Coolify dashboard**  
Coolify → Your Project → Supabase Resource → Database/PostgreSQL service (`supabase-db`) → Ports: add mapping **5432:5432** (host:container). Save and redeploy.

**Option B1b — Docker Compose override**  
If the dashboard does not allow port mapping, in Coolify → Supabase Resource → Configuration / Advanced, add a Docker Compose override so the DB service exposes `5432:5432`. Save and redeploy.

**Option B1c — Manual (fallback)**  
On the VPS, find the Supabase DB container and ensure it is published with `-p 5432:5432`. Manual changes may be lost on Coolify redeploy; prefer B1a or B1b.

---

## Step B2: PostgreSQL listening on all interfaces

Supabase’s PostgreSQL image often sets `listen_addresses = '*'` by default. Verify:

```bash
ssh root@62.169.27.210
docker exec -it $(docker ps -q --filter name=supabase-db) bash
# Inside container:
psql -U postgres -c "SHOW listen_addresses;"
```

- **Expected:** `listen_addresses = *`
- If it is `localhost`, override via environment (e.g. in Coolify → Supabase DB service → Environment: `POSTGRES_HOST=0.0.0.0`) or a custom `postgresql.conf` and redeploy.

---

## Step B3: Allow remote connections in pg_hba.conf

PostgreSQL will reject remote connections without this.

**Find the file (on VPS):**

```bash
docker exec $(docker ps -q --filter name=supabase-db) find / -name pg_hba.conf 2>/dev/null
# Often: /var/lib/postgresql/data/pg_hba.conf
```

**Edit:** Copy the file out, add a line, and mount it back (or use an init script if your setup supports it). Add **at the top** (before other `host` rules):

**Restrict to your IP (recommended):**

```text
host  all  all  YOUR_DEV_IP/32  md5
```

**Allow all IPs (least secure — only if necessary):**

```text
host  all  all  0.0.0.0/0  md5
```

Then reload Postgres (or restart the DB container). If you use a mounted config, redeploy the Supabase resource in Coolify.

---

## Step B4: Open firewall on the VPS

**UFW (Ubuntu/Debian):**

```bash
ssh root@62.169.27.210

# Restrict to your IP (recommended)
sudo ufw allow from YOUR_DEV_IP to any port 5432 proto tcp
sudo ufw reload

# Or allow from anywhere (least secure)
# sudo ufw allow 5432/tcp
# sudo ufw reload
```

**Cloud provider:** In the VPS firewall / security group, add an inbound rule: TCP port **5432**, source: your IP (or 0.0.0.0/0 only if you accept the risk).

---

## Step B5: Connection string when using public access

**From your dev machine** (appsettings.Development.json or `DATABASE_URL`):

```text
Host=supabase.almutasim.site;Port=5432;Database=postgres;Username=postgres;Password=YOUR_PASSWORD
```

**Coolify (production)** can keep using the internal host:

```text
ConnectionStrings__DefaultConnection=Host=supabase-db;Port=5432;Database=postgres;Username=postgres;Password=YOUR_PASSWORD
```

---

## Step B6: Mandatory security hardening (do not skip)

1. **Change the postgres password** to a strong, unique value and update all connection strings.
2. **Create a limited-privilege app user** — do not use the `postgres` superuser for the app:

   ```sql
   CREATE USER ecl_app WITH PASSWORD 'YourStrongPassword';
   GRANT CONNECT ON DATABASE postgres TO ecl_app;
   GRANT USAGE ON SCHEMA public TO ecl_app;
   GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO ecl_app;
   GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO ecl_app;
   ```

   Then use `Username=ecl_app` (and the new password) in the app connection string.

3. **Fail2Ban (brute-force protection):** On the VPS, install and configure fail2ban with a jail for PostgreSQL (port 5432) to block IPs after repeated failed auth.
4. **Monitor logs** regularly for authentication failures and suspicious activity.

---

# 📋 Decision checklist

| If you choose **SSH tunnel** (recommended) | If you choose **public access** |
|--------------------------------------------|----------------------------------|
| Keep port 5432 **closed** on the VPS        | Expose port 5432 in Coolify and firewall |
| No `pg_hba.conf` or listen_address changes  | Set `listen_addresses = '*'`, add `host ... md5` in pg_hba.conf |
| Dev: `Host=localhost` (tunnel)             | Dev: `Host=supabase.almutasim.site` (or VPS IP) |
| Prod: `Host=supabase-db` (internal)          | Prod: can still use `Host=supabase-db` |
| No public exposure                         | Restrict pg_hba + firewall to your IP; use strong password + limited user + fail2ban |

**Recommendation:** Use the SSH tunnel for local development. If you must have public access, restrict by IP and apply all hardening steps above.
