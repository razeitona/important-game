#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# Configure environment variables
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Install Node.js for asset minification (Terser)
RUN apt-get update && \
    apt-get install -y curl && \
    curl -fsSL https://deb.nodesource.com/setup_20.x | bash - && \
    apt-get install -y nodejs && \
    npm install -g npm@latest && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/important-game.web/important-game.web.csproj", "src/important-game.web/"]
COPY ["src/important-game.infrastructure/important-game.infrastructure.csproj", "src/important-game.infrastructure/"]
RUN dotnet restore "./src/important-game.web/important-game.web.csproj"
COPY . .
WORKDIR "/src/src/important-game.web"

# Install npm packages for build process
RUN npm install

# Build with minification
RUN dotnet build "./important-game.web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./important-game.web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create data directory for database volume
RUN mkdir -p /app/data && \
    chmod -R 777 /app/data

# Set environment variable for database connection string
ENV ConnectionStrings__DefaultConnection=Data Source=/app/data/matchwatch.db

EXPOSE 80
ENTRYPOINT ["dotnet", "important-game.web.dll"]