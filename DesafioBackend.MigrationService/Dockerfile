FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["DesafioBackend.MigrationService/DesafioBackend.MigrationService.csproj", "DesafioBackend.MigrationService/"]
COPY ["DesafioBackend.Core.Data/DesafioBackend.Core.Data.csproj", "DesafioBackend.Core.Data/"]
COPY ["DesafioBackend.Core/DesafioBackend.Core.csproj", "DesafioBackend.Core/"]
COPY ["DesafioBackend.ServiceDefaults/DesafioBackend.ServiceDefaults.csproj", "DesafioBackend.ServiceDefaults/"]
RUN dotnet restore "DesafioBackend.MigrationService/DesafioBackend.MigrationService.csproj"
COPY . .
WORKDIR "/src/DesafioBackend.MigrationService"
RUN dotnet build "DesafioBackend.MigrationService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "DesafioBackend.MigrationService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DesafioBackend.MigrationService.dll"]
