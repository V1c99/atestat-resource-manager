FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

COPY src/ResourceManager.Domain/*.csproj src/ResourceManager.Domain/
COPY src/ResourceManager.Infrastructure/*.csproj src/ResourceManager.Infrastructure/
COPY src/ResourceManager.Api/*.csproj src/ResourceManager.Api/
RUN dotnet restore src/ResourceManager.Api/ResourceManager.Api.csproj

COPY src/ src/
RUN dotnet publish src/ResourceManager.Api/ResourceManager.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "ResourceManager.Api.dll"]
