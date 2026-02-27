using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using MongoDB.Driver;
using Properties.Services;
using Properties.Settings;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Properties API",
        Version = "v1"
    });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insira apenas o token JWT"
    };

    options.AddSecurityDefinition("Bearer", securityScheme);

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

// Keycloak configuration
var keycloakSettings = builder.Configuration.GetSection("Keycloak");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = keycloakSettings["Authority"];
        options.Audience = keycloakSettings["Audience"];
        options.RequireHttpsMetadata = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuers = keycloakSettings.GetSection("ValidIssuers").Get<string[]>(),
            ValidateAudience = false,
            ValidateLifetime = true
        };
    });



builder.Services.AddAuthorization();

builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDbSettings"));

var mongoSettings = builder.Configuration.GetSection("MongoDbSettings").Get<MongoDbSettings>();

if (mongoSettings == null || string.IsNullOrEmpty(mongoSettings.ConnectionString))
{
    throw new Exception("Erro: Seção MongoDbSettings não encontrada no appsettings.json ou ConnectionString vazia.");
}

var client = new MongoClient(mongoSettings.ConnectionString);
var database = client.GetDatabase(mongoSettings.DatabaseName);
builder.Services.AddSingleton<IMongoDatabase>(database);

builder.Services.AddScoped<IPropertyService, PropertyService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();