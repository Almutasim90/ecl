# ── Stage 1: Build ────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Install Node.js for Tailwind CSS compilation
#RUN apt-get update && apt-get install -y --no-install-recommends curl \
#    && curl -fsSL https://deb.nodesource.com/setup_22.x | bash - \
#  && apt-get install -y --no-install-recommends nodejs \
#    && rm -rf /var/lib/apt/lists/*

# Restore .NET deps
COPY ECL.csproj ./
RUN dotnet restore

# Install npm deps and build CSS
#COPY package.json package-lock.json* ./
#RUN npm ci
#COPY tailwind.config.js ./
#COPY wwwroot/css/app.css ./wwwroot/css/app.css
#COPY Views ./Views
#RUN npm run css:build

# Copy rest of source and publish
COPY . ./
RUN dotnet publish ECL.csproj -c Release -o /app/publish

# ── Stage 2: Runtime ──────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish ./

# Runtime (non-secret). Secrets must come from your host (e.g. Coolify → Environment Variables).
ENV ASPNETCORE_URLS=http://+:8081
ENV ASPNETCORE_ENVIRONMENT=Production

# ── Required when deploying from this Dockerfile only (no docker-compose) ──
# Published appsettings use an empty DB connection. Set in Coolify / your orchestrator:
#   DATABASE_URL=postgresql://...  OR  ConnectionStrings__DefaultConnection=Host=...;...
# Optional admin login:
#   AdminCredentials__Username  /  AdminCredentials__Password
# Optional CORS:
#   WEB_ORIGIN=https://your-public-site

EXPOSE 8081

ENTRYPOINT ["dotnet", "ECL.dll"]
