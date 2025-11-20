# EcoRide Integration Tests

Professional integration tests using **Testcontainers** with PostgreSQL+PostGIS for realistic database testing.

## Architecture

### Test Infrastructure
- **WebApplicationFactory**: Custom factory that hosts the entire API in-memory
- **Testcontainers**: Spins up isolated PostgreSQL containers with PostGIS extension
- **PostgreSQL 16 + PostGIS 3.4**: Full database with geospatial support
- **Clean Architecture**: Tests against real API endpoints and database

### Why Testcontainers?
✅ **Real Database Behavior**: Uses actual PostgreSQL instead of in-memory mocks
✅ **PostGIS Support**: Full geospatial functionality for Fleet module
✅ **Isolation**: Each test run gets a fresh database container
✅ **Reproducibility**: Consistent environment across all machines
✅ **CI/CD Ready**: Works in GitHub Actions, Azure DevOps, etc.

## Prerequisites

### Required
- **.NET 9.0 SDK**
- **Docker Desktop** (running)
  - Windows: Docker Desktop for Windows
  - macOS: Docker Desktop for Mac
  - Linux: Docker Engine

### Verify Docker
```bash
docker --version
docker ps
```

## Running Tests

### All Integration Tests
```bash
dotnet test tests/EcoRide.IntegrationTests/EcoRide.IntegrationTests.csproj
```

### Specific Test Class
```bash
dotnet test tests/EcoRide.IntegrationTests/EcoRide.IntegrationTests.csproj --filter "FullyQualifiedName~AuthControllerTests"
```

### Single Test
```bash
dotnet test tests/EcoRide.IntegrationTests/EcoRide.IntegrationTests.csproj --filter "FullyQualifiedName~RegisterUser_WithValidData_ShouldReturn200"
```

### With Detailed Output
```bash
dotnet test tests/EcoRide.IntegrationTests/EcoRide.IntegrationTests.csproj --verbosity detailed
```

## Test Coverage

### Authentication Tests (`AuthControllerTests.cs`)
- ✅ User registration with validation
- ✅ Email/phone number validation
- ✅ Password strength requirements
- ✅ Duplicate email handling
- ✅ OTP verification flow
- ✅ Login with valid/invalid credentials
- ✅ Forgot password flow
- ✅ Password reset with codes
- ✅ JWT refresh token flow

**Total: 14 tests**

### Vehicle Tests (`VehiclesControllerTests.cs`)
- ✅ Get nearby vehicles with geospatial queries
- ✅ Coordinate validation (latitude/longitude)
- ✅ Radius filtering
- ✅ Vehicle type filtering (Scooter, Bike)
- ✅ Battery level filtering
- ✅ Status filtering (Available, Reserved, InUse, etc.)
- ✅ Pagination (page number, page size)
- ✅ Multiple filter combinations

**Total: 20 tests**

### Trip Tests (`TripsControllerTests.cs`)
- ✅ Start trip with reservation validation
- ✅ QR code validation
- ✅ Coordinate validation for trip locations
- ✅ Active trip stats retrieval
- ✅ Trip history with pagination
- ✅ Emergency contacts retrieval
- ✅ End trip validation

**Total: 20 tests**

## Test Execution Flow

1. **Container Startup**: TestContainers pulls `postgis/postgis:16-3.4` image and starts container
2. **Database Creation**: EF Core creates schemas (`security`, `fleet`, `trip`) and tables
3. **API Hosting**: WebApplicationFactory hosts the API in-memory
4. **Test Execution**: HTTP requests hit real endpoints with real database
5. **Cleanup**: Container is automatically stopped and removed

## Performance

- **First Run**: ~30-60 seconds (downloads Docker image)
- **Subsequent Runs**: ~10-15 seconds (image cached)
- **Per Test**: ~100-500ms average

## CI/CD Integration

### GitHub Actions
```yaml
- name: Start Docker
  run: |
    sudo systemctl start docker

- name: Run Integration Tests
  run: dotnet test tests/EcoRide.IntegrationTests/EcoRide.IntegrationTests.csproj
```

### Azure DevOps
```yaml
- script: dotnet test tests/EcoRide.IntegrationTests/EcoRide.IntegrationTests.csproj
  displayName: 'Run Integration Tests'
```

## Troubleshooting

### "Docker is not running"
**Solution**: Start Docker Desktop

### "Image pull timeout"
**Solution**: Check internet connection or configure Docker registry mirror

### "Port already in use"
**Solution**: TestContainers uses random ports - conflict unlikely, but restart Docker if it happens

### Tests are slow
**Solution**:
- Ensure Docker has sufficient resources (CPU: 2+, RAM: 4GB+)
- Use SSD for Docker storage
- Tests run in parallel by default - this is expected behavior

## File Structure

```
EcoRide.IntegrationTests/
├── Api/
│   ├── AuthControllerTests.cs      # Authentication endpoint tests
│   ├── VehiclesControllerTests.cs  # Vehicle endpoint tests
│   └── TripsControllerTests.cs     # Trip endpoint tests
├── Infrastructure/
│   └── IntegrationTestWebAppFactory.cs  # TestContainers setup
├── EcoRide.IntegrationTests.csproj
└── README.md                        # This file
```

## Key Dependencies

```xml
<PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="9.0.0" />
<PackageReference Include="Testcontainers.PostgreSql" Version="3.10.0" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="9.0.2" />
<PackageReference Include="xunit" Version="2.9.2" />
```

## Best Practices

✅ **Isolation**: Each test class gets its own database instance via `IClassFixture`
✅ **Cleanup**: TestContainers automatically removes containers after tests
✅ **Realistic**: Uses same database type as production (PostgreSQL+PostGIS)
✅ **Fast**: Parallel execution with xUnit
✅ **Deterministic**: Fresh database for each run ensures no test interdependencies

## Next Steps

- [ ] Add more edge case tests
- [ ] Test concurrent user scenarios
- [ ] Add performance benchmarks
- [ ] Test database migrations
- [ ] Add integration tests for payment flows

## Support

For issues or questions:
1. Check Docker is running: `docker ps`
2. Verify .NET SDK: `dotnet --version`
3. Check test output for specific errors
4. Review logs in test output

---

**Professional Integration Testing** ✨
Built with Testcontainers for maximum reliability and maintainability.
