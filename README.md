# EcoRide Backend API

A modern, scalable backend API for an eco-friendly bike and scooter sharing platform. Built with Clean Architecture principles and designed for high performance and maintainability.

**Developed by Omar Achbani**

---

## Overview

EcoRide is a comprehensive vehicle-sharing platform that enables users to locate, unlock, and rent electric bikes and scooters. The system provides real-time vehicle tracking, seamless payment processing through digital wallets, and comprehensive trip management.

### Key Features

- **User Management**: Secure registration and authentication system
- **Vehicle Discovery**: Real-time geospatial search for available vehicles
- **Trip Management**: Complete trip lifecycle from vehicle unlock to payment
- **Digital Wallet**: Integrated wallet system for seamless payments
- **Trip History**: Comprehensive trip tracking with detailed receipts
- **Admin Dashboard**: Platform monitoring and analytics
- **Rating System**: User feedback and service quality tracking

---

## Architecture

### Design Patterns

The project implements **Clean Architecture** with **Domain-Driven Design (DDD)** principles, ensuring:

- Clear separation of concerns
- High testability
- Independent business logic
- Maintainable and scalable codebase

### Architecture Layers

```
┌─────────────────────────────────────────────────┐
│              API Layer (REST)                   │
│         Controllers, Middleware, DTOs           │
└─────────────────┬───────────────────────────────┘
                  │
┌─────────────────▼───────────────────────────────┐
│          Application Layer (CQRS)               │
│      Commands, Queries, Handlers, DTOs          │
└─────────────────┬───────────────────────────────┘
                  │
┌─────────────────▼───────────────────────────────┐
│            Domain Layer (Core)                  │
│  Entities, Value Objects, Business Logic        │
└─────────────────┬───────────────────────────────┘
                  │
┌─────────────────▼───────────────────────────────┐
│       Infrastructure Layer (Data)               │
│   EF Core, Repositories, Database, External     │
└─────────────────────────────────────────────────┘
```

### Modular Architecture

The system is organized into three bounded contexts (modules):

1. **Security Module**: User authentication, wallet management, payments
2. **Fleet Module**: Vehicle management, location tracking, availability
3. **Trip Module**: Trip lifecycle, pricing, ratings, receipts

Each module maintains its own:
- Domain logic and entities
- Database schema
- Repository implementations
- Use cases (commands/queries)

---

## Technology Stack

### Core Framework
- **.NET 9.0**: Latest .NET version for maximum performance
- **ASP.NET Core**: High-performance web API framework
- **C# 13**: Modern language features and syntax

### Architecture & Patterns
- **Clean Architecture**: Layered architecture for maintainability
- **CQRS**: Command Query Responsibility Segregation with MediatR
- **Repository Pattern**: Data access abstraction
- **Result Pattern**: Functional error handling
- **Value Objects**: Rich domain modeling

### Database & ORM
- **PostgreSQL 17**: Robust relational database
- **Entity Framework Core 9.0**: Modern ORM with migrations
- **Npgsql**: PostgreSQL data provider

### API & Communication
- **RESTful API**: Standard HTTP/JSON endpoints
- **Swagger/OpenAPI**: Interactive API documentation
- **MediatR**: In-process messaging for CQRS

### Testing
- **xUnit**: Modern testing framework
- **Moq**: Mocking library for unit tests
- **FluentAssertions**: Readable test assertions
- **323 Tests**: Comprehensive test coverage
  - 250 Unit Tests
  - 57 Integration Tests
  - 16 End-to-End Tests

### DevOps & Deployment
- **Docker**: Containerization for consistent deployments
- **Docker Compose**: Multi-container orchestration
- **GitHub Actions**: CI/CD automation
- **Code Formatting**: dotnet format for code quality

### Development Tools
- **Visual Studio 2022**: Primary IDE
- **Git**: Version control
- **Postman**: API testing (Swagger alternative)

---

## Project Structure

```
EcoRide/
├── src/
│   ├── EcoRide.Api/                    # API Layer
│   │   ├── Controllers/                # REST endpoints
│   │   ├── Middleware/                 # HTTP pipeline
│   │   └── Program.cs                  # Application entry
│   │
│   ├── BuildingBlocks/                 # Shared components
│   │   ├── Domain/                     # Base entities, interfaces
│   │   ├── Application/                # CQRS base classes
│   │   └── Infrastructure/             # Common infrastructure
│   │
│   └── Modules/
│       ├── Security/                   # User & Wallet Module
│       │   ├── Domain/                 # Entities, value objects
│       │   ├── Application/            # Use cases (CQRS)
│       │   └── Infrastructure/         # Data access, migrations
│       │
│       ├── Fleet/                      # Vehicle Module
│       │   ├── Domain/
│       │   ├── Application/
│       │   └── Infrastructure/
│       │
│       └── Trip/                       # Trip Module
│           ├── Domain/
│           ├── Application/
│           └── Infrastructure/
│
├── tests/
│   ├── EcoRide.UnitTests/              # Unit tests (250 tests)
│   ├── EcoRide.IntegrationTests/       # Integration tests (57 tests)
│   └── EcoRide.E2ETests/               # End-to-end tests (16 tests)
│
├── Docs/                               # Documentation
│   └── UI_UX_Specifications.md         # Frontend specifications
│
├── database/                           # Database scripts
├── docker-compose.yml                  # Docker configuration
└── EcoRide.sln                         # Solution file
```

---

## API Endpoints

### User Management
```
POST   /api/users/register          Create new user account
POST   /api/users/login             Authenticate user
```

### Vehicle Management
```
GET    /api/vehicles/available      Get available vehicles by location
GET    /api/vehicles/nearby         Get nearby vehicles with distance
POST   /api/vehicles/unlock         Unlock vehicle and start trip
```

### Trip Management
```
GET    /api/trips/active            Get user's active trip
POST   /api/trips/end               End trip with rating and payment
GET    /api/trips/history           Get trip history (paginated)
GET    /api/trips/{tripId}          Get trip details
GET    /api/trips/{tripId}/receipt  Get trip receipt
```

### Wallet Management
```
GET    /api/wallet/balance          Get wallet balance
POST   /api/wallet/add-funds        Add funds to wallet (10-1000 MAD)
GET    /api/wallet/transactions     Get transaction history (paginated)
```

### Admin Panel
```
GET    /api/admin/dashboard         Get platform statistics
```

---

## Business Rules

### Pricing
- **Base Cost**: 5.00 MAD per trip
- **Per-Minute Rate**: 1.50 MAD per minute
- **Formula**: Total = Base Cost + (Duration × Per-Minute Rate)

### Wallet
- **Minimum Top-Up**: 10.00 MAD
- **Maximum Top-Up**: 1000.00 MAD
- **Automatic Payment**: Deducted from wallet on trip completion

### Trip Rules
- One active trip per user at a time
- Vehicle must be available to unlock
- Rating required on trip completion (1-5 stars)

---

## Getting Started

### Prerequisites
- .NET 9.0 SDK
- PostgreSQL 17
- Docker (optional)

### Running with Docker

1. Clone the repository:
```bash
git clone https://github.com/Omar1Ach/EcoRide-Backend.git
cd EcoRide-Backend
```

2. Start the application:
```bash
docker-compose up
```

The API will be available at `http://localhost:5000`

### Running Locally

1. Set up PostgreSQL and update connection string in `appsettings.json`

2. Apply database migrations:
```bash
dotnet ef database update --project src/Modules/Security/EcoRide.Modules.Security
dotnet ef database update --project src/Modules/Fleet/EcoRide.Modules.Fleet
dotnet ef database update --project src/Modules/Trip/EcoRide.Modules.Trip
```

3. Run the application:
```bash
dotnet run --project src/EcoRide.Api
```

4. Access Swagger documentation:
```
http://localhost:5000/swagger
```

---

## Testing

### Run All Tests
```bash
dotnet test
```

### Run Specific Test Suite
```bash
# Unit tests only
dotnet test tests/EcoRide.UnitTests

# Integration tests only
dotnet test tests/EcoRide.IntegrationTests

# E2E tests only
dotnet test tests/EcoRide.E2ETests
```

### Test Coverage
- **Total Tests**: 323
- **Pass Rate**: 100%
- **Coverage**: All business logic covered

---

## Configuration

### Environment Variables

```bash
ConnectionStrings__SecurityDb=Host=localhost;Database=ecoride_security;...
ConnectionStrings__FleetDb=Host=localhost;Database=ecoride_fleet;...
ConnectionStrings__TripDb=Host=localhost;Database=ecoride_trip;...
```

### Database Schemas

The application uses a modular database approach with separate schemas:
- `security`: User accounts, wallets, transactions
- `fleet`: Vehicles, locations, availability
- `trip`: Trips, ratings, receipts

---

## Performance & Scalability

### Current Capacity
- **Designed for**: Up to 100K users
- **Architecture**: Modular monolith (easy to extract to microservices)
- **Database**: PostgreSQL with optimized indexes
- **Response Time**: < 100ms for most endpoints

### Scalability Path
1. **Phase 1 (MVP)**: Modular monolith with Clean Architecture
2. **Phase 2**: Add domain events and message bus
3. **Phase 3**: Extract modules to microservices
4. **Phase 4**: Event sourcing and CQRS read models

---

## Code Quality

### Standards
- **Code Style**: Microsoft C# coding conventions
- **Formatting**: Enforced via dotnet format
- **CI/CD**: GitHub Actions with automated checks
- **Warnings**: Treated as errors in production builds

### Quality Metrics
- Clean Architecture compliance
- SOLID principles
- High test coverage
- No critical code smells

---

## API Documentation

### Swagger UI
Interactive API documentation is available at `/swagger` when running the application.

### Frontend Integration
Comprehensive UI/UX specifications for frontend developers are available in `Docs/UI_UX_Specifications.md`.

---

## Security Considerations

### Current Implementation
- Password validation (minimum complexity requirements)
- User isolation (users can only access their own data)
- Input validation on all endpoints
- SQL injection prevention (parameterized queries)

### Production Recommendations
- Implement JWT authentication
- Add HTTPS enforcement
- Rate limiting per user
- API key management for admin endpoints
- Encrypt sensitive data at rest

---

## Monitoring & Logging

### Logging
- Structured logging with correlation IDs
- Different log levels (Debug, Info, Warning, Error)
- Request/response logging in development

### Health Checks
- Database connectivity checks
- API health endpoint
- Liveness and readiness probes for Kubernetes

---

## Contributing

This is a private project developed by Omar Achbani.

---

## License

© 2025 Omar Achbani. All rights reserved.

This project is proprietary and confidential.

---

## Contact

**Developer**: Omar Achbani

For questions or support, please contact the development team.

---

## Version History

### v1.0.0 (Current)
- ✅ User registration and authentication
- ✅ Vehicle discovery with geospatial search
- ✅ Trip management (start, end, rating)
- ✅ Digital wallet with transaction history
- ✅ Admin dashboard
- ✅ Complete test suite (323 tests)
- ✅ Docker deployment support
- ✅ CI/CD with GitHub Actions

---

## Technical Highlights

### Clean Architecture Benefits
- **Testability**: 323 automated tests with 100% pass rate
- **Maintainability**: Clear separation of concerns
- **Flexibility**: Easy to swap infrastructure components
- **Scalability**: Ready for microservices extraction

### CQRS Pattern
- Optimized read and write operations
- Scalable query handling
- Clear use case boundaries
- Easy to add caching layers

### Domain-Driven Design
- Rich domain models with business logic
- Value objects for type safety
- Repository pattern for data access
- Modular bounded contexts

---

**Built with passion for clean code and sustainable mobility** 🚴‍♂️⚡

