# ── Stage 1: Build (.NET + Node for Tailwind CSS) ─────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Install Node.js (required for tailwindcss css:build during dotnet publish)
RUN apt-get update && apt-get install -y --no-install-recommends curl \
    && curl -fsSL https://deb.nodesource.com/setup_22.x | bash - \
    && apt-get install -y --no-install-recommends nodejs \
    && rm -rf /var/lib/apt/lists/*

# Restore .NET dependencies first (layer cache)
COPY ECL.csproj ./
RUN dotnet restore

# Install npm dependencies (for Tailwind)
COPY package.json package-lock.json* ./
RUN npm ci --omit=dev

# Copy remaining source and publish (triggers BuildTailwind target)
COPY . ./
RUN dotnet publish -c Release -o /app/publish

# ── Stage 2: Runtime ──────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish ./

# Run as non-root user
RUN adduser --disabled-password --no-create-home appuser \
    && chown -R appuser /app
USER appuser

# ASP.NET Core listens on port 8080 inside the container
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Connection string is supplied at runtime via environment variable:
#   -e ConnectionStrings__DefaultConnection="Host=...;Database=...;Username=...;Password=..."
# or via Docker secrets / compose env_file. Do NOT hardcode credentials here.

ENTRYPOINT ["dotnet", "ECL.dll"]
