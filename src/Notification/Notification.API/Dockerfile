FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/Shared/TicketBooking.Common/TicketBooking.Common.csproj", "src/Shared/TicketBooking.Common/"]
COPY ["src/Notification/Notification.API/Notification.API.csproj", "src/Notification/Notification.API/"]
RUN dotnet restore "src/Notification/Notification.API/Notification.API.csproj"
COPY . .
WORKDIR "/src/src/Notification/Notification.API"
RUN dotnet build "Notification.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Notification.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Notification.API.dll"]
