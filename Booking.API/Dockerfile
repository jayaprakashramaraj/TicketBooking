FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["TicketBooking.Common/TicketBooking.Common.csproj", "TicketBooking.Common/"]
COPY ["Booking.API/Booking.API.csproj", "Booking.API/"]
RUN dotnet restore "Booking.API/Booking.API.csproj"
COPY . .
WORKDIR "/src/Booking.API"
RUN dotnet build "Booking.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Booking.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Booking.API.dll"]
