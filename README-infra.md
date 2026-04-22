# Infrastructure Services

This project uses a separate Docker Compose file for infrastructure services (SQL Server, Redis, RabbitMQ, MongoDB). This allows you to keep the infrastructure running while you restart or debug individual microservices locally.

## How to use

### 1. Start Infrastructure Only
Run this command to start all databases and message brokers:

```bash
docker-compose -f docker-compose.infra.yml up -d
```

### 2. Local Development (VS Code / CLI)
When running services locally (e.g., via `dotnet run` or VS Code F5), they will connect to these containers using `localhost` and the standard ports:

- **SQL Server:** `127.0.0.1,1434`
- **Redis:** `127.0.0.1:6380`
- **RabbitMQ:** `127.0.0.1:5673` (Management: `http://localhost:15673`)
- **MongoDB:** `127.0.0.1:27017`

### 3. Start Everything (Full Stack)
If you want to run the entire stack (Infrastructure + APIs + Frontend) in Docker:

```bash
docker-compose up -d
```

Note: `docker-compose.yml` extends `docker-compose.infra.yml`, so it will automatically include the infrastructure services.
