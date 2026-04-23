FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/Shared/TicketBooking.Common/TicketBooking.Common.csproj", "src/Shared/TicketBooking.Common/"]
COPY ["src/Payment/Payment.Domain/Payment.Domain.csproj", "src/Payment/Payment.Domain/"]
COPY ["src/Payment/Payment.Application/Payment.Application.csproj", "src/Payment/Payment.Application/"]
COPY ["src/Payment/Payment.Infrastructure/Payment.Infrastructure.csproj", "src/Payment/Payment.Infrastructure/"]
COPY ["src/Payment/Payment.API/Payment.API.csproj", "src/Payment/Payment.API/"]
RUN dotnet restore "src/Payment/Payment.API/Payment.API.csproj"
COPY . .
WORKDIR "/src/src/Payment/Payment.API"
RUN dotnet build "Payment.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Payment.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Payment.API.dll"]
