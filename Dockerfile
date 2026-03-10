# ── Build stage ────────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project file and restore dependencies first (layer-cache friendly)
COPY ECL.csproj ./
RUN dotnet restore

# Copy everything else and publish
COPY . ./
RUN dotnet publish ECL.csproj -c Release -o /app/publish --no-restore

# ── Runtime stage ───────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

# ASP.NET Core listens on 8081 inside the container; Coolify will map it externally
ENV ASPNETCORE_URLS=http://0.0.0.0:8081
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8081

ENTRYPOINT ["dotnet", "ECL.dll"]
