# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/important-game.app/important-game.app.csproj", "src/important-game.app/"]
COPY ["src/important-game.infrastructure/important-game.infrastructure.csproj", "src/important-game.infrastructure/"]
RUN dotnet restore "./src/important-game.app/important-game.app.csproj"
COPY . .
WORKDIR "/src/src/important-game.app"
RUN dotnet build "./important-game.app.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./important-game.app.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create data directory for database volume
RUN mkdir -p /app/data && \
    chmod -R 777 /app/data

# Set environment variable for database connection string
ENV ConnectionStrings__DefaultConnection=Data Source=/app/data/matchwatch.db

ENTRYPOINT ["dotnet", "important-game.app.dll"]