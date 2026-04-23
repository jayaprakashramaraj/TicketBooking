FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/Shared/TicketBooking.Common/TicketBooking.Common.csproj", "src/Shared/TicketBooking.Common/"]
COPY ["src/Booking/Booking.Domain/Booking.Domain.csproj", "src/Booking/Booking.Domain/"]
COPY ["src/Booking/Booking.Application/Booking.Application.csproj", "src/Booking/Booking.Application/"]
COPY ["src/Booking/Booking.Infrastructure/Booking.Infrastructure.csproj", "src/Booking/Booking.Infrastructure/"]
COPY ["src/Booking/Booking.API/Booking.API.csproj", "src/Booking/Booking.API/"]
RUN dotnet restore "src/Booking/Booking.API/Booking.API.csproj"
COPY . .
WORKDIR "/src/src/Booking/Booking.API"
RUN dotnet build "Booking.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Booking.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Booking.API.dll"]
