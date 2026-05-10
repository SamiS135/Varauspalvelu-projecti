using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MySql.Data.MySqlClient;
using NSwag.AspNetCore;
using ShelterBookingAPI;
using ShelterBookingAPI.Models;
using System.Text;

// Luodaan web-sovellus ASP.NET Core:n avulla
var builder = WebApplication.CreateBuilder(args);

// Rekisteröidään DatabaseHelper palveluna, jotta sitä voidaan käyttää riippuvuuden injektoinnin kautta
// Tämä tarkoittaa, että kaikki kontrollerit voivat pyytää DatabaseHelper-oliota
builder.Services.AddScoped<DatabaseHelper>();

// Lisätään Controllers-tuki (AuthController ja muut)
builder.Services.AddControllers();

// Lisätään JWT-autentikointi
var secretKey = "LemmikkihoitolaHaapasenHuvila2024!";
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

// Lisätään OpenAPI dokumentaation tuki (Swagger)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config =>
{
    config.DocumentName = "ShelterBookingAPI";
    config.Title = "ShelterBookingAPI v1";
    config.Version = "v1";
});

// Lisätään CORS-tuki (Cross-Origin Resource Sharing) jotta WordPress-sivu voi kutsua tätä API:a
// AllowAnyOrigin = sallii pyynnöt mistä tahansa osoitteesta
builder.Services.AddCors(options => {
    options.AddPolicy("AllowWordPress", policy => {
        policy.AllowAnyOrigin()              // Sallii mistä tahansa alkuperästä
              .AllowAnyHeader()               // Sallii kaikki HTTP-otsikot
              .AllowAnyMethod();              // Sallii kaikki HTTP-metodit (GET, POST, DELETE jne)
    });
});

// Rakennetaan sovellus
var app = builder.Build();

// Jos sovellus on kehitystilassa, käytetään Swagger-dokumentaatiota
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

// Otetaan CORS-käytäntö käyttöön
app.UseCors("AllowWordPress");

// Lisätään autentikointi ja autorisointi
app.UseAuthentication();
app.UseAuthorization();

// Rekisteröidään kontrollerit (AuthController, BookingController jne)
app.MapControllers();

// Määritellään API-reitit (URL-polut joita WordPress-sivu voi kutsua)
// Note: API endpoints are provided by controllers in ShelterBookingAPI.Controllers
// The minimal API route mappings were removed to avoid duplicate route registrations
// which caused AmbiguousMatchException on /api/animal etc.

// Sovellus kuuntelee oletusporia (5000 tai 5001)
app.Run();

