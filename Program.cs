using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MySql.Data.MySqlClient;
using NSwag.AspNetCore;
using ShelterBookingAPI;
using ShelterBookingAPI.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Sovelluksen entry-point (top-level statements).
// Tämä tiedosto konfiguroi palvelut (DI), autentikoinnin, Swaggerin ja CORS-politiikat.
// Tärkeimmät symbolit:
// - `builder` : sovelluksen rakennin, sisältää Configuration-objektin
// - `secretKey` : JWT:n allekirjoitusavain (luetaan asetuksista)
// - `key` : UTF8-bytit allekirjoitusavaimesta, käytetään TokenValidationParameters

// Rekisteröi DatabaseHelper palveluna
builder.Services.AddScoped<DatabaseHelper>();

var secretKey = builder.Configuration["Jwt:SecretKey"]
    ?? throw new InvalidOperationException("Jwt:SecretKey puuttuu asetuksista.");

// Lisää Controllers-tuki
builder.Services.AddControllers();

// Aseta JWT-autentikointi
var key = Encoding.UTF8.GetBytes(secretKey);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true
        };
    });

builder.Services.AddAuthorization();

// Lisää Swagger API dokumentaatio
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config =>
{
    config.DocumentName = "ShelterBookingAPI";
    config.Title = "ShelterBookingAPI v1";
    config.Version = "v1";
});

// Lisää CORS (sallii pyynnöt eri alkuperistä)
builder.Services.AddCors(options => {
    options.AddPolicy("AllowWordPress", policy => {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

// Rakenna sovellus
var app = builder.Build();

// Kehitystilassa: käytä Swaggeria
if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi(config =>
    {
        config.DocumentTitle = "ShelterBookingAPI";
        config.Path = "/swagger";
        config.DocumentPath = "/swagger/{documentName}/swagger.json";
        config.DocExpansion = "list";
    });
}

// Aktivoi CORS
app.UseCors("AllowWordPress");

// Aktivoi autentikointi ja autorisointi
app.UseAuthentication();
app.UseAuthorization();

// Rekisteröi kontrollerit
app.MapControllers();

// API-päätepisteet tulevat kontrollereista
app.Run();
app.MapControllers();

