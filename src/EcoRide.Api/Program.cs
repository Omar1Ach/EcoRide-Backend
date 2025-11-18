using EcoRide.BuildingBlocks.Application;
using EcoRide.Modules.Security.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add controllers
builder.Services.AddControllers();

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "EcoRide API",
        Version = "v1",
        Description = "EcoRide - Tourism-focused bike/scooter sharing platform for Morocco"
    });
});

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
    typeof(EcoRide.Modules.Security.Application.Commands.RegisterUser.RegisterUserCommand).Assembly);

// Add Infrastructure layer (DbContext, Repositories, Services)
builder.Services.AddSecurityInfrastructure(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "EcoRide API v1");
        options.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
    });
}

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthorization();

app.MapControllers();

app.Run();
