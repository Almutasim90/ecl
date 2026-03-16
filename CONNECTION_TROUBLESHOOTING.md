# Supabase / PostgreSQL connection troubleshooting

This app connects to a **PostgreSQL** database (self-hosted Supabase, e.g. `supabase.almutasim.site` or VPS IP).

**To make the DB publicly accessible** (so you can connect from Development or anywhere): see **[docs/SUPABASE_PUBLIC_ACCESS.md](docs/SUPABASE_PUBLIC_ACCESS.md)** for the full checklist (firewall, listen_addresses, pg_hba.conf).

Connection string is in `appsettings.json`, `appsettings.Development.json`, or environment variable `DATABASE_URL` (`ConnectionStrings:DefaultConnection`).

**If connection still fails after trying different hostnames:** the ECL container may not be joining the Supabase Docker network. Run the [diagnostics below](#verify-ecl-is-on-the-supabase-network-run-on-vps) on the VPS and fix network membership in Coolify first.

---

## Running in Docker / Coolify: "Resource temporarily unavailable" or cannot resolve host

If the app runs **inside a container** (e.g. Coolify) and you see:

- **`SocketException (11): Resource temporarily unavailable`** at `Dns.GetHostEntryOrAddressesCore`
- **Database 'postgres' on server 'tcp://supabase-db:5432'** (or similar)

then the container **cannot resolve the database hostname** (`supabase-db` or whatever you used). Common causes:

1. **Wrong hostname** — The `Host=` in your connection string must be the **Docker service name** of the Postgres container **on the same network** as your app. Coolify/Supabase may use a different name (e.g. `db`, `supabase_db`, or a project-prefixed name like `msnhqastnwdl9epbmggflxxz-db`).
2. **ECL container not on the Supabase network (most likely)** — If the ECL app container is **not** actually attached to the Supabase Docker network (`msnhqastnwdl9epbmggflxxz`), no hostname will resolve and connection will always fail. **Verify network membership first** before changing hostnames or connection strings again.

### Verify ECL is on the Supabase network (run on VPS)

SSH to the VPS (e.g. `ssh root@62.169.27.210`) and run:

```bash
# 1. List containers on the Supabase network — ECL app must appear here
docker network inspect msnhqastnwdl9epbmggflxxz --format '{{range .Containers}}{{.Name}} {{end}}'

# 2. Find the ECL app container name
docker ps --format '{{.Names}}' | grep -i ecl

# 3. Check which networks the ECL container is on
docker inspect <ECL_CONTAINER_NAME> --format '{{range $k, $v := .NetworkSettings.Networks}}{{$k}} {{end}}'
```

- If **msnhqastnwdl9epbmggflxxz** does **not** appear in step 3, the ECL container is not on the Supabase network. Fix this in Coolify (see below); no code or hostname change will work until the app is on that network.
- If it does appear, use step 1 to see the **exact container/service names** on that network. For this Supabase stack the Postgres container is **`supabase-db-msnhqastnwdl9epbmggflxxz`** — set `DATABASE_HOST_OVERRIDE=supabase-db-msnhqastnwdl9epbmggflxxz` (or `Host=` in `DATABASE_URL`) to that value.

**Coolify: ensure ECL joins the Supabase network**

- In Coolify, the ECL app must be deployed with a compose file that declares the **external** network with the **exact** name Coolify uses for the Supabase project (e.g. `msnhqastnwdl9epbmggflxxz`). In this repo, `docker-compose.yaml` has `supabase_network` with `name: msnhqastnwdl9epbmggflxxz`. Coolify must create/use that same network and attach the ECL app to it when deploying. If Coolify deploys the app without that network (e.g. only attaches `coolify`), add the Supabase network to the ECL app’s deployment configuration so the app container joins it.

**What to do:**

1. **Use the correct DB hostname for this stack**  
   On this Supabase/Coolify setup the Postgres container is named **`supabase-db-msnhqastnwdl9epbmggflxxz`**. In Coolify → your ECL application → **Environment variables**, set:
   ```text
   DATABASE_HOST_OVERRIDE=supabase-db-msnhqastnwdl9epbmggflxxz
   ```
   Keep your existing `DATABASE_URL`. Redeploy. **The ECL container must also be on the same Docker network** (`msnhqastnwdl9epbmggflxxz`); if it is not in the network list (step 1 above), connection will still fail until you fix that in Coolify.

2. **Find the correct DB hostname**  
   On the server (or in Coolify): open the Supabase resource and check the **service name** of the PostgreSQL container (in the stack/compose it might be `db` or `supabase-db`). Or run:
   ```bash
   docker network inspect msnhqastnwdl9epbmggflxxz
   ```
   and look at which containers are attached and their names (use the **service name**, not the container ID). Set `DATABASE_HOST_OVERRIDE` to that name, or put it in `DATABASE_URL` as `Host=...`.

3. **Set the connection string in Coolify**  
   If you prefer not to use the override, set:
   ```text
   DATABASE_URL=Host=REAL_SERVICE_NAME;Port=5432;Database=postgres;Username=postgres;Password=YOUR_PASSWORD
   ```
   Replace `REAL_SERVICE_NAME` with the actual Postgres service name (e.g. `db`).

4. **Confirm the app is on the Supabase network**  
   Your ECL app must be on the same Docker network as Supabase (e.g. `msnhqastnwdl9epbmggflxxz`). In your docker-compose the app uses `supabase_network`; ensure Coolify attaches that network to the app.

After fixing the hostname and network, redeploy the app.

---

## Observed error (and what it means)

If you see:

- **`PostgreSQL Error: Exception while reading from stream`**  
  **`Inner: An existing connection was forcibly closed by the remote host`**

then the TCP connection to port 5432 is established, but **the server is closing the connection** during or right after the PostgreSQL handshake. Common causes:

1. **pg_hba.conf** does not allow your client IP (or only allows `localhost`).
2. **SSL mismatch**: server requires SSL but the app does not use it (or the opposite). This setup uses **no SSL** — do not add `SSL Mode=Require` to the connection string.
3. **Auth or protocol**: server rejects the connection before sending an error (e.g. strict auth).

Port **5432 is open** (confirmed with `Test-NetConnection`). So the fix is on the **server**, not the firewall.

## Fix on the Supabase/PostgreSQL server

Do this on the machine where PostgreSQL (or Supabase) runs (**62.169.27.210**):

1. **Allow external connections in `pg_hba.conf`**  
   Add a line so your client IP (or all IPs for testing) can connect:
   ```text
   host  all  all  0.0.0.0/0  md5
   ```
   Or restrict to your IP: `host  all  all  YOUR_CLIENT_IP/32  md5`  
   Then reload PostgreSQL: `pg_ctl reload` or `sudo systemctl reload postgresql` (or restart the Postgres container).

2. **Ensure Postgres is listening on all interfaces**  
   In `postgresql.conf`:
   ```text
   listen_addresses = '*'
   ```
   Restart PostgreSQL after changing this.

3. **SSL**  
   This setup uses **no SSL**. Keep the connection string without any `SSL Mode=...` (or use `SSL Mode=Disable` if needed). If your server later enables SSL, add `SSL Mode=Require;Trust Server Certificate=true`.

4. **Supabase Docker**  
   If Postgres runs in Docker, ensure the **host** (not only the container) has `pg_hba.conf` and `postgresql.conf` updated if you connect from outside Docker, or that the port mapping (e.g. `5432:5432`) and the container’s config allow external clients.

## If connection still fails

1. **Check the exact error**  
   On startup the app runs diagnostics. Look for:
   - `PostgreSQL Error: ...` and `Inner: ...`
   - `Connection refused` → port closed or wrong host
   - `timeout` → firewall or network blocking

2. **Try Supabase pooler port**  
   If your setup exposes the pooler, use port **54322** in the connection string (and ensure it’s open):
   ```text
   Host=62.169.27.210;Port=54322;Database=postgres;Username=postgres;Password=...;SearchPath=public
   ```

3. **Override via environment**  
   Set `ConnectionStrings__DefaultConnection` to your full connection string (e.g. in Coolify or your host).

## App behavior when the DB is unreachable

The app **still starts** if the database connection fails at startup. You will see the error in the console, but the web server will listen. Once the server’s `pg_hba.conf` and `listen_addresses` are fixed, restart the app (or rely on retries) and the connection should succeed.

## Testing from your machine

```powershell
Test-NetConnection -ComputerName 62.169.27.210 -Port 5432
```

If `TcpTestSucceeded` is `True`, the port is open; then fix `pg_hba.conf` and `listen_addresses` on the server as above.
