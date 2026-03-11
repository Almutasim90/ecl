# Supabase / PostgreSQL connection troubleshooting

This app connects to a **PostgreSQL** database (e.g. self-hosted Supabase at `62.169.27.210`).

## Current configuration (testing)

- **Host:** `62.169.27.210`
- **Port:** `5432`
- **Database:** `postgres`
- **User:** `postgres`
- **SSL:** Disable (for testing)

Connection string is in `appsettings.json` and `appsettings.Development.json` under `ConnectionStrings:DefaultConnection`.

## Observed error (and what it means)

If you see:

- **`PostgreSQL Error: Exception while reading from stream`**  
  **`Inner: An existing connection was forcibly closed by the remote host`**

then the TCP connection to port 5432 is established, but **the server is closing the connection** during or right after the PostgreSQL handshake. Common causes:

1. **pg_hba.conf** does not allow your client IP (or only allows `localhost`).
2. **SSL mismatch**: server requires SSL but the app uses `SSL Mode=Disable`, or the opposite.
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

3. **If the server requires SSL**  
   In the app connection string, set:
   - `SSL Mode=Require` and `Trust Server Certificate=true`  
   (Already tried in diagnostics; if it still fails, the server may be closing for another reason.)

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
   Host=62.169.27.210;Port=54322;Database=postgres;Username=postgres;Password=...;SearchPath=public;SSL Mode=Require;Trust Server Certificate=true
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
