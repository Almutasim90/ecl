# ── Stage 1: Build ────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Install Node.js for Tailwind CSS compilation
RUN apt-get update && apt-get install -y --no-install-recommends curl \
    && curl -fsSL https://deb.nodesource.com/setup_22.x | bash - \
    && apt-get install -y --no-install-recommends nodejs \
    && rm -rf /var/lib/apt/lists/*

# Restore .NET deps
COPY ECL.csproj ./
RUN dotnet restore

# Install npm deps and build CSS
COPY package.json package-lock.json* ./
RUN npm ci
COPY tailwind.config.js ./
COPY wwwroot/css/app.css ./wwwroot/css/app.css
COPY Views ./Views
RUN npm run css:build

# Copy rest of source and publish
COPY . ./
RUN dotnet publish -c Release -o /app/publish

# ── Stage 2: Runtime ──────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish ./

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

ENTRYPOINT ["dotnet", "ECL.dll"]
