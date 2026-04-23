FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/PaymentSimulator/PaymentSimulator.API/PaymentSimulator.API.csproj", "src/PaymentSimulator/PaymentSimulator.API/"]
COPY ["src/Shared/TicketBooking.Common/TicketBooking.Common.csproj", "src/Shared/TicketBooking.Common/"]
RUN dotnet restore "src/PaymentSimulator/PaymentSimulator.API/PaymentSimulator.API.csproj"
COPY . .
WORKDIR "/src/src/PaymentSimulator/PaymentSimulator.API"
RUN dotnet build "PaymentSimulator.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./PaymentSimulator.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PaymentSimulator.API.dll"]
