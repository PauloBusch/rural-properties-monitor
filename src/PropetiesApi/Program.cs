using PropertiesService.Infrastructure.Mongo;
using PropetiesApi.Application.Services;
using PropetiesApi.Application.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Register MongoDB context and services
builder.Services.AddSingleton<MongoContext>();
builder.Services.AddScoped<IPropertyService, PropertyService>();
builder.Services.AddScoped<IPlotService, PlotService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
