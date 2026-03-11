# ── Build stage ───────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

RUN apt-get update \
	&& apt-get install -y --no-install-recommends nodejs npm \
	&& rm -rf /var/lib/apt/lists/*

COPY ECL.csproj ./
RUN dotnet restore

COPY . ./
RUN if [ -f package-lock.json ]; then npm ci; else npm install; fi
RUN dotnet publish -c Release -o /app/publish

# ── Runtime stage ─────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Audio files are mounted at runtime via Docker volume — not baked into image
COPY --from=build /app/publish ./

# Persist SQLite DB with server files in container storage/volume
RUN mkdir -p /app/App_Data
VOLUME ["/app/App_Data"]

# Prefer local SQLite file unless DATABASE_URL is explicitly provided at runtime
ENV ConnectionStrings__DefaultConnection="Data Source=/app/App_Data/ecl.db"

# ASP.NET Core listens on port 8080 inside the container
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "ECL.dll"]
