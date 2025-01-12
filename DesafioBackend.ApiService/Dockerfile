FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["DesafioBackend.ApiService/DesafioBackend.ApiService.csproj", "DesafioBackend.ApiService/"]
COPY ["DesafioBackend.ServiceDefaults/DesafioBackend.ServiceDefaults.csproj", "DesafioBackend.ServiceDefaults/"]
COPY ["DesafioBackend.Core.Data/DesafioBackend.Core.Data.csproj", "DesafioBackend.Core.Data/"]
COPY ["DesafioBackend.Core/DesafioBackend.Core.csproj", "DesafioBackend.Core/"]
COPY ["DesafioBackend.Application/DesafioBackend.Application.csproj", "DesafioBackend.Application/"]
RUN dotnet restore "DesafioBackend.ApiService/DesafioBackend.ApiService.csproj"
COPY . .
WORKDIR "/src/DesafioBackend.ApiService"
RUN dotnet build "DesafioBackend.ApiService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "DesafioBackend.ApiService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DesafioBackend.ApiService.dll"]
