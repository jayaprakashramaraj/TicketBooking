FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/Identity/Identity.Domain/Identity.Domain.csproj", "src/Identity/Identity.Domain/"]
COPY ["src/Identity/Identity.Application/Identity.Application.csproj", "src/Identity/Identity.Application/"]
COPY ["src/Identity/Identity.Infrastructure/Identity.Infrastructure.csproj", "src/Identity/Identity.Infrastructure/"]
COPY ["src/Identity/Identity.API/Identity.API.csproj", "src/Identity/Identity.API/"]
RUN dotnet restore "src/Identity/Identity.API/Identity.API.csproj"
COPY . .
WORKDIR "/src/src/Identity/Identity.API"
RUN dotnet build "Identity.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Identity.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Identity.API.dll"]
