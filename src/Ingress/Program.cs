using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
    
builder.Services.AddControllers();
builder.Services.AddOpenApi();

//Keycloak configuration
var keycloakSettings = builder.Configuration.GetSection("Keycloak");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Keycloak:Authority"];
        options.Audience = builder.Configuration["Keycloak:Audience"];
        options.RequireHttpsMetadata = builder.Configuration.GetValue<bool>("Keycloak:RequireHttpsMetadata");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuers = keycloakSettings.GetSection("ValidIssuers").Get<string[]>(),
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// Bind KafkaConfig from configuration and register as singleton
builder.Services.Configure<IngressApi.Models.KafkaConfig>(
    builder.Configuration.GetSection("Kafka"));
builder.Services.AddSingleton(resolver =>
    resolver.GetRequiredService<Microsoft.Extensions.Options.IOptions<IngressApi.Models.KafkaConfig>>().Value);

// Bind InfluxDbConfig from configuration and register as singleton
builder.Services.Configure<IngressApi.Models.InfluxDbConfig>(
    builder.Configuration.GetSection("InfluxDbConfig"));
builder.Services.AddSingleton(resolver =>
    resolver.GetRequiredService<Microsoft.Extensions.Options.IOptions<IngressApi.Models.InfluxDbConfig>>().Value);

// Register repository
builder.Services.AddSingleton<IngressApi.Repositories.ISensorDataRepository, IngressApi.Repositories.InfluxSensorDataRepository>();

// Register services
builder.Services.AddScoped<IngressApi.Services.ISensorDataService, IngressApi.Services.SensorDataService>();

// Register Kafka consumer background service
builder.Services.AddHostedService<IngressApi.Services.KafkaConsumerService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

//Security - Keycloak
app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllers();

app.Run();
