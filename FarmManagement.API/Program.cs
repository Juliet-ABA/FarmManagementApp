using FarmManagement.API.Data;
using FarmManagement.API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Azure.Cosmos;

var builder = WebApplication.CreateBuilder(args);

// Retrieve allowed origins for CORS from configuration
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

// Configure Cosmos Client as a Singleton
builder.Services.AddSingleton<CosmosClient>(serviceProvider =>
{
    var cosmosDbEndpoint = builder.Configuration["CosmosDb:EndpointUri"];
    var cosmosDbKey = builder.Configuration["CosmosDb:PrimaryKey"];
    return new CosmosClient(cosmosDbEndpoint, cosmosDbKey);
});

// Register the CosmosDbService as the implementation of IFieldsService
builder.Services.AddScoped<IFieldsService, CosmosDbService>();

// Add controllers for API
builder.Services.AddControllers();

// Add Swagger/OpenAPI for documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS support
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "blazorCors",
        policy =>
        {
            policy.WithOrigins("https://localhost:7086")
            .AllowAnyMethod()
            .AllowAnyHeader();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Apply CORS policy
app.UseCors("blazorCors");

app.UseAuthorization();

// Map controller endpoints
app.MapControllers();

// Start the application
app.Run();
