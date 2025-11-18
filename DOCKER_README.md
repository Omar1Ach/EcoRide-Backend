# EcoRide Docker Infrastructure Setup

## Quick Start

### 1. Start All Services
```bash
docker-compose up -d
```

### 2. View Logs
```bash
docker-compose logs -f
```

### 3. Stop All Services
```bash
docker-compose down
```

### 4. Stop and Remove All Data (Fresh Start)
```bash
docker-compose down -v
```

## Service URLs

| Service | URL | Credentials |
|---------|-----|-------------|
| **PostgreSQL** | `localhost:5432` | User: `ecoride_user`<br>Password: `EcoRide2025!Strong`<br>Database: `EcoRide` |
| **PgAdmin** | http://localhost:5050 | Email: `admin@ecoride.ma`<br>Password: `EcoRide2025!PgAdmin` |
| **Redis** | `localhost:6379` | Password: `EcoRide2025!Redis` |
| **RabbitMQ Management** | http://localhost:15672 | User: `ecoride`<br>Password: `EcoRide2025!Rabbit` |
| **Seq Logging** | http://localhost:5341 | Auto-login (development) |

## Verify Services

### Check PostgreSQL
```bash
docker exec -it ecoride-postgres psql -U ecoride_user -d EcoRide -c "SELECT version();"
```

### Check PostGIS Extension
```bash
docker exec -it ecoride-postgres psql -U ecoride_user -d EcoRide -c "SELECT PostGIS_version();"
```

### Check Redis
```bash
docker exec -it ecoride-redis redis-cli -a "EcoRide2025!Redis" ping
```

### Check RabbitMQ
```bash
docker exec -it ecoride-rabbitmq rabbitmqctl status
```

## Connect to PostgreSQL from .NET

**Connection String:**
```
Host=localhost;Port=5432;Database=EcoRide;Username=ecoride_user;Password=EcoRide2025!Strong;Include Error Detail=true
```

## Troubleshooting

### Reset PostgreSQL Database
```bash
docker-compose down
docker volume rm ecoridproject_postgres_data
docker-compose up -d postgres
```

### View Service Health
```bash
docker-compose ps
```

### Access PostgreSQL Shell
```bash
docker exec -it ecoride-postgres psql -U ecoride_user -d EcoRide
```

## Production Notes

⚠️ **IMPORTANT:** Change all default passwords before deploying to production!

Update in:
- `docker-compose.yml`
- `.env` file
- `appsettings.Production.json`
