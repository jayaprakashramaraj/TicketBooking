FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["TicketBooking.Common/TicketBooking.Common.csproj", "TicketBooking.Common/"]
COPY ["Notification.API/Notification.API.csproj", "Notification.API/"]
RUN dotnet restore "Notification.API/Notification.API.csproj"
COPY . .
WORKDIR "/src/Notification.API"
RUN dotnet build "Notification.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Notification.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Notification.API.dll"]
