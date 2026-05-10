using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using NSwag.AspNetCore;
using ShelterBookingAPI;
using ShelterBookingAPI.Models;

// Luodaan web-sovellus ASP.NET Core:n avulla
var builder = WebApplication.CreateBuilder(args);

// Rekisteröidään DatabaseHelper palveluna, jotta sitä voidaan käyttää riippuvuuden injektoinnin kautta
// Tämä tarkoittaa, että kaikki kontrollerit voivat pyytää DatabaseHelper-oliota
builder.Services.AddScoped<DatabaseHelper>();

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

// Määritellään API-reitit (URL-polut joita WordPress-sivu voi kutsua)
// /api/animal - eläimen tiedot
RouteGroupBuilder animalGroup = app.MapGroup("/api/animal");
animalGroup.MapGet("/", GetAnimals);

// /api/booking - varaukset (haku, lisäys, poisto)
RouteGroupBuilder bookingGroup = app.MapGroup("/api/booking");
bookingGroup.MapGet("/", GetBookings);                    // GET = nouda kaikki varaukset
bookingGroup.MapPost("/", AddBooking);                   // POST = lisää uusi varaus
bookingGroup.MapDelete("/{id}", DeleteBooking);          // DELETE = poista varaus ID:n perusteella

// /api/price - hinnoittelu
RouteGroupBuilder priceGroup = app.MapGroup("/api/price");
priceGroup.MapGet("/{species}/{serviceType}", GetPrices); // GET = nouda hinnat lajin ja palvelun mukaan

// Sovellus kuuntelee oletusporia (5000 tai 5001)
app.Run();

/// <summary>
/// Noutaa kaikki saatavilla olevat eläimet tietokannasta
/// </summary>
static async Task<IResult> GetAnimals(DatabaseHelper db, ILogger<Program> logger)
{
    try
    {
        var animals = new List<Animal>();

        // Avataan yhteys MySQL-tietokantaan
        using var conn = db.GetConnection();
        await conn.OpenAsync();

        // SQL-kysely: valitaan kaikki eläimet joiden available-sarake on TRUE (saatavilla)
        var cmd = new MySqlCommand("SELECT * FROM animals WHERE available = TRUE", conn);
        using var reader = await cmd.ExecuteReaderAsync();

        // Luetaan tulosjoukosta rivi kerrallaan
        while (await reader.ReadAsync())
        {
            animals.Add(new Animal
            {
                Id = (int)reader["id"],                              // Eläimen tunnusnumero
                Name = reader["name"]?.ToString() ?? string.Empty,   // Eläimen nimi
                Species = reader["species"]?.ToString() ?? string.Empty, // Laji (kissa, koira jne)
                Breed = reader["breed"]?.ToString() ?? string.Empty, // Rotu
                Age = reader["age"] != DBNull.Value ? (int)reader["age"] : 0, // Ikä vuosina
                Available = (bool)reader["available"]                // Saatavilla? true/false
            });
        }

        // Palautetaan eläinten lista JSON-muodossa
        return TypedResults.Ok(animals);
    }
    catch (Exception ex)
    {
        // Jos tapahtuu virhe, kirjoitetaan se logiikkaan ja palautetaan virhekoodi 500
        logger.LogError(ex, "Error fetching animals");
        return TypedResults.StatusCode(500);
    }
}

/// <summary>
/// Noutaa kaikki varaukset tietokannasta
/// </summary>
static async Task<IResult> GetBookings(DatabaseHelper db, ILogger<Program> logger)
{
    try
    {
        var bookings = new List<Booking>();

        using var conn = db.GetConnection();
        await conn.OpenAsync();

        // SQL-kysely: valitaan kaikki varaukset
        var cmd = new MySqlCommand("SELECT * FROM bookings", conn);
        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            bookings.Add(new Booking
            {
                Id = (int)reader["id"],                             // Varauksen tunnusnumero
                FirstName = reader["first_name"]?.ToString() ?? string.Empty, // Asiakas: etunimi
                LastName = reader["last_name"]?.ToString() ?? string.Empty,   // Asiakas: sukunimi
                Email = reader["email"]?.ToString() ?? string.Empty,          // Asiakkaan sähköposti
                Phone = reader["phone"]?.ToString() ?? string.Empty,          // Asiakkaan puhelinnumero
                AnimalId = reader["animal_id"] != DBNull.Value ? (int)reader["animal_id"] : 0, // Minkä eläimen varauksesta on kyse
                ServiceType = reader["service_type"]?.ToString() ?? string.Empty, // Palvelutyyppi (hoito, koulutus jne)
                StartDate = reader["start_date"] != DBNull.Value ? (DateTime)reader["start_date"] : DateTime.MinValue, // Varauksen alkamispäivä
                EndDate = reader["end_date"] != DBNull.Value ? (DateTime)reader["end_date"] : DateTime.MinValue,       // Varauksen päättymispäivä
                TotalPrice = reader["total_price"] != DBNull.Value ? (decimal)reader["total_price"] : 0, // Kokonais hinta
                Status = reader["status"]?.ToString() ?? "pending"   // Tila (pending=odottava, confirmed=vahvistettu jne)
            });
        }

        return TypedResults.Ok(bookings);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error fetching bookings");
        return TypedResults.StatusCode(500);
    }
}

/// <summary>
/// Lisää uuden varauksen tietokantaan
/// </summary>
static async Task<IResult> AddBooking(Booking booking, DatabaseHelper db, ILogger<Program> logger)
{
    try
    {
        // Tarkistetaan että pakollinen tieto on annettu
        if (string.IsNullOrWhiteSpace(booking.FirstName) || string.IsNullOrWhiteSpace(booking.LastName))
            return TypedResults.BadRequest(new { error = "Etunimi ja sukunimi vaaditaan" });
        
        if (string.IsNullOrWhiteSpace(booking.Email))
            return TypedResults.BadRequest(new { error = "Sähköposti vaaditaan" });
        
        // Sallitaan saman päivän varaukset, mutta päättymispäivä ei saa olla ennen alkamispäivää
        if (booking.EndDate < booking.StartDate)
            return TypedResults.BadRequest(new { error = "Päättymispäivä ei voi olla ennen alkamispäivää" });

        using var conn = db.GetConnection();
        await conn.OpenAsync();

        // SQL-komento: lisätään uusi rivi bookings-tauluun
        var cmd = new MySqlCommand(@"
            INSERT INTO bookings
            (first_name, last_name, email, phone, animal_id,
             service_type, start_date, end_date, total_price, status)
            VALUES
            (@fn, @ln, @email, @phone, @aid,
             @type, @start, @end, @price, @status)", conn);

        // @-merkit ovat paikan pitäjiä joihin korvataan todellinen data (sql-injektio-suojaus)
        cmd.Parameters.AddWithValue("@fn", booking.FirstName ?? string.Empty);
        cmd.Parameters.AddWithValue("@ln", booking.LastName ?? string.Empty);
        cmd.Parameters.AddWithValue("@email", booking.Email ?? string.Empty);
        cmd.Parameters.AddWithValue("@phone", booking.Phone ?? string.Empty);
        cmd.Parameters.AddWithValue("@aid", booking.AnimalId);
        cmd.Parameters.AddWithValue("@type", booking.ServiceType ?? string.Empty);
        cmd.Parameters.AddWithValue("@start", booking.StartDate);
        cmd.Parameters.AddWithValue("@end", booking.EndDate);
        cmd.Parameters.AddWithValue("@price", booking.TotalPrice);
        cmd.Parameters.AddWithValue("@status", booking.Status ?? "pending");

        // Suoritetaan komento (INSERT)
        await cmd.ExecuteNonQueryAsync();

        return TypedResults.Ok(new { message = "Varaus lisätty!" });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error adding booking");
        return TypedResults.StatusCode(500);
    }
}

/// <summary>
/// Poistaa varauksen tietokannasta ID:n perusteella
/// </summary>
static async Task<IResult> DeleteBooking(int id, DatabaseHelper db, ILogger<Program> logger)
{
    try
    {
        using var conn = db.GetConnection();
        await conn.OpenAsync();

        // SQL-komento: poistetaan varaus jonka ID vastaa parametria
        var cmd = new MySqlCommand("DELETE FROM bookings WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("@id", id);

        // Suoritetaan komento ja tarkistetaan montako riviä poistettiin
        int rowsAffected = await cmd.ExecuteNonQueryAsync();
        
        // Jos mitään ei poistettu (0 riviä), varaus ei ollut olemassa
        if (rowsAffected == 0)
            return TypedResults.NotFound(new { error = "Varausta ei löydetty" });

        return TypedResults.Ok(new { message = "Varaus poistettu!" });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error deleting booking");
        return TypedResults.StatusCode(500);
    }
}

/// <summary>
/// Noutaa hinnat tietyn lajin ja palvelun perusteella
/// </summary>
static async Task<IResult> GetPrices(string species, string serviceType, DatabaseHelper db, ILogger<Program> logger)
{
    try
    {
        // Tarkistetaan että laji ja palvelutyyppi on annettu
        if (string.IsNullOrWhiteSpace(species) || string.IsNullOrWhiteSpace(serviceType))
            return TypedResults.BadRequest(new { error = "Laji ja palvelutyyppi vaaditaan" });

        var prices = new List<Price>();

        using var conn = db.GetConnection();
        await conn.OpenAsync();

        // SQL-kysely: etsitään hinnat jotka vastaavat lajia ja palvelutyyppiä
        var cmd = new MySqlCommand(
            "SELECT * FROM prices WHERE species = @species AND service_type = @type", conn);
        cmd.Parameters.AddWithValue("@species", species);
        cmd.Parameters.AddWithValue("@type", serviceType);

        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            prices.Add(new Price
            {
                Id = (int)reader["id"],                                 // Hinnan tunnusnumero
                Species = reader["species"]?.ToString() ?? string.Empty, // Laji (kissa, koira jne)
                ServiceType = reader["service_type"]?.ToString() ?? string.Empty, // Palvelutyyppi
                ServiceName = reader["service_name"]?.ToString() ?? string.Empty, // Palvelun nimi
                Amount = reader["amount"] != DBNull.Value ? (decimal)reader["amount"] : 0 // Hinta euroin
            });
        }

        return TypedResults.Ok(prices);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error fetching prices");
        return TypedResults.StatusCode(500);
    }
}
