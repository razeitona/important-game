#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app

# Configure environment variables
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/important-game.web/important-game.web.csproj", "src/important-game.web/"]
COPY ["src/important-game.infrastructure/important-game.infrastructure.csproj", "src/important-game.infrastructure/"]
RUN dotnet restore "./src/important-game.web/important-game.web.csproj"
COPY . .
WORKDIR "/src/src/important-game.web"
RUN dotnet build "./important-game.web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./important-game.web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

USER root
RUN chmod 777 /app/data.json
USER app


EXPOSE 80
ENTRYPOINT ["dotnet", "important-game.web.dll"]