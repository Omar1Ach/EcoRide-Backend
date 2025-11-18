using EcoRide.BuildingBlocks.Application;
<<<<<<< Updated upstream
=======
using EcoRide.Modules.Security.Infrastructure;
>>>>>>> Stashed changes
using EcoRide.Modules.Fleet.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add controllers
builder.Services.AddControllers();

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS for development
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add Application layer (MediatR, FluentValidation, Behaviors)
builder.Services.AddApplication(
    typeof(Program).Assembly,
<<<<<<< Updated upstream
    typeof(EcoRide.Modules.Fleet.Application.Queries.GetNearbyVehicles.GetNearbyVehiclesQuery).Assembly);

// Add Infrastructure layer (DbContext, Repositories)
=======
    typeof(EcoRide.Modules.Security.Application.Commands.RegisterUser.RegisterUserCommand).Assembly,
    typeof(EcoRide.Modules.Fleet.Application.Queries.GetNearbyVehicles.GetNearbyVehiclesQuery).Assembly);

// Add Infrastructure layer (DbContext, Repositories, Services)
builder.Services.AddSecurityInfrastructure(builder.Configuration);
>>>>>>> Stashed changes
builder.Services.AddFleetInfrastructure(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "EcoRide API v1");
        options.RoutePrefix = string.Empty; // Swagger at root
    });
}

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthorization();

app.MapControllers();

app.Run();
