FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/Catalog/Catalog.Domain/Catalog.Domain.csproj", "src/Catalog/Catalog.Domain/"]
COPY ["src/Catalog/Catalog.Application/Catalog.Application.csproj", "src/Catalog/Catalog.Application/"]
COPY ["src/Catalog/Catalog.Infrastructure/Catalog.Infrastructure.csproj", "src/Catalog/Catalog.Infrastructure/"]
COPY ["src/Catalog/Catalog.API/Catalog.API.csproj", "src/Catalog/Catalog.API/"]
RUN dotnet restore "src/Catalog/Catalog.API/Catalog.API.csproj"
COPY . .
WORKDIR "/src/src/Catalog/Catalog.API"
RUN dotnet build "Catalog.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Catalog.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Catalog.API.dll"]
