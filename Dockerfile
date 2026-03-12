# ── Stage 1: Build Frontend (Node.js) ──────────────────────────────
FROM node:20-alpine AS node-build
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
RUN npm run build  # Assuming you have a build script in package.json

# ── Stage 2: Build Backend (.NET) ──────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS dotnet-build
WORKDIR /src
COPY ["ECL.csproj", "./"]
RUN dotnet restore
COPY . .
# Copy frontend assets from the node-build stage
COPY --from=node-build /app/wwwroot ./wwwroot
RUN dotnet publish -c Release -o /app/publish

# ── Stage 3: Runtime ───────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=dotnet-build /app/publish .

# Persist DB and Data
RUN mkdir -p /app/App_Data
VOLUME ["/app/App_Data"]

# Connection via Environment Variable
ENV ConnectionStrings__DefaultConnection="Data Source=/app/App_Data/ecl.db"
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "ECL.dll"]
