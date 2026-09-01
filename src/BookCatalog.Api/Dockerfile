FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/BookCatalog.Api/BookCatalog.Api.csproj", "src/BookCatalog.Api/"]
COPY ["src/BookCatalog.Infrastructure/BookCatalog.Infrastructure.csproj", "src/BookCatalog.Infrastructure/"]
COPY ["src/BookCatalog.Domain/BookCatalog.Domain.csproj", "src/BookCatalog.Domain/"]
COPY ["src/BookCatalog.Application/BookCatalog.Application.csproj", "src/BookCatalog.Application/"]
RUN dotnet restore "src/BookCatalog.Api/BookCatalog.Api.csproj"
COPY . .
WORKDIR "/src/src/BookCatalog.Api"
RUN dotnet build "./BookCatalog.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./BookCatalog.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BookCatalog.Api.dll"]
