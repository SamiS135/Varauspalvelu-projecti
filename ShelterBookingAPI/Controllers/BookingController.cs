using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using ShelterBookingAPI.Models;

namespace ShelterBookingAPI.Controllers;

/// <summary>
/// BookingController hoitaa varausten hallintaan liittyvät API-pyynnöt
/// Käyttäjät voivat hakea, lisätä ja poistaa varauksia
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class BookingController : ControllerBase {
    // _db on DatabaseHelper-olio jota käytetään tietokantayhteyden luomiseen
    private readonly DatabaseHelper _db;

    /// <summary>
    /// Konstruktori - vastaanottaa DatabaseHelper-olion
    /// </summary>
    public BookingController(DatabaseHelper db) {
        _db = db;
    }

    /// <summary>
    /// GET-pyyntö: Noutaa kaikki varaukset
    /// Osoite: http://localhost:5000/api/booking
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetBookings() {
        try {
            var bookings = new List<Booking>();
            using var conn = _db.GetConnection();
            await conn.OpenAsync();
            
            // SQL-kysely: valitaan kaikki varaukset
            var cmd = new MySqlCommand("SELECT * FROM bookings", conn);
            var reader = await cmd.ExecuteReaderAsync();
            
            while (await reader.ReadAsync()) {
                bookings.Add(new Booking {
                    Id          = (int)reader["id"],                             // Varauksen tunnusnumero
                    FirstName   = reader["first_name"]?.ToString() ?? string.Empty,  // Asiakkaan etunimi
                    LastName    = reader["last_name"]?.ToString() ?? string.Empty,   // Asiakkaan sukunimi
                    Email       = reader["email"]?.ToString() ?? string.Empty,       // Asiakkaan sähköposti
                    Phone       = reader["phone"]?.ToString() ?? string.Empty,       // Asiakkaan puhelinnumero
                    AnimalId    = reader["animal_id"] != DBNull.Value ? (int)reader["animal_id"] : 0, // Eläimen ID
                    ServiceType = reader["service_type"]?.ToString() ?? string.Empty, // Palvelutyyppi
                    StartDate   = reader["start_date"] != DBNull.Value ? (DateTime)reader["start_date"] : DateTime.MinValue, // Alkupäivä
                    EndDate     = reader["end_date"] != DBNull.Value ? (DateTime)reader["end_date"] : DateTime.MinValue,     // Loppupäivä
                    TotalPrice  = reader["total_price"] != DBNull.Value ? (decimal)reader["total_price"] : 0, // Kokonais hinta
                    Status      = reader["status"]?.ToString() ?? "pending"      // Tila (odottava/vahvistettu jne)
                });
            }
            return Ok(bookings);
        }
        catch (Exception ex) {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// POST-pyyntö: Lisää uuden varauksen tietokantaan
    /// Osoite: http://localhost:5000/api/booking
    /// Vaatii JSON-muotoisen Booking-olion request-rungossa
    /// </summary>
    /// <param name="booking">Varauksen tiedot JSON-muodossa</param>
    [HttpPost]
    public async Task<IActionResult> AddBooking([FromBody] Booking booking) {
        try {
            // VALIDOINTI: Tarkistetaan että pakollinen tieto on annettu
            
            // Tarkistetaan etunimi ja sukunimi
            if (string.IsNullOrWhiteSpace(booking.FirstName) || string.IsNullOrWhiteSpace(booking.LastName))
                return BadRequest(new { error = "Etunimi ja sukunimi vaaditaan" });
            
            // Tarkistetaan sähköposti
            if (string.IsNullOrWhiteSpace(booking.Email))
                return BadRequest(new { error = "Sähköposti vaaditaan" });
            
            // Sallitaan saman päivän varaukset, mutta loppupäivä ei saa olla ennen alkamispäivää
            if (booking.EndDate < booking.StartDate)
                return BadRequest(new { error = "Päättymispäivä ei voi olla ennen alkamispäivää" });

            using var conn = _db.GetConnection();
            await conn.OpenAsync();
            
            // SQL-komento: lisätään uusi rivi bookings-tauluun
            // @-merkit ovat paikan pitäjiä, jotka suojaavat SQL-injektio hyökkäyksiltä
            var cmd = new MySqlCommand(@"
                INSERT INTO bookings
                (first_name, last_name, email, phone, animal_id,
                 service_type, start_date, end_date, total_price, status)
                VALUES
                (@fn, @ln, @email, @phone, @aid,
                 @type, @start, @end, @price, @status)", conn);

            // Asetetaan parametrien arvot
            cmd.Parameters.AddWithValue("@fn",    booking.FirstName ?? string.Empty);
            cmd.Parameters.AddWithValue("@ln",    booking.LastName ?? string.Empty);
            cmd.Parameters.AddWithValue("@email", booking.Email ?? string.Empty);
            cmd.Parameters.AddWithValue("@phone", booking.Phone ?? string.Empty);
            cmd.Parameters.AddWithValue("@aid",   booking.AnimalId);
            cmd.Parameters.AddWithValue("@type",  booking.ServiceType ?? string.Empty);
            cmd.Parameters.AddWithValue("@start", booking.StartDate);
            cmd.Parameters.AddWithValue("@end",   booking.EndDate);
            cmd.Parameters.AddWithValue("@price", booking.TotalPrice);
            cmd.Parameters.AddWithValue("@status", booking.Status ?? "pending");

            // Suoritetaan INSERT-komento
            await cmd.ExecuteNonQueryAsync();

            return Ok(new { message = "Varaus lisätty!" });
        }
        catch (Exception ex) {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// DELETE-pyyntö: Poistaa varauksen tietokannasta
    /// Osoite: http://localhost:5000/api/booking/{id}
    /// Esimerkki: http://localhost:5000/api/booking/5
    /// </summary>
    /// <param name="id">Poistettavan varauksen tunnus</param>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBooking(int id) {
        try {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();
            
            // SQL-komento: poistetaan varaus jonka ID vastaa parametria
            var cmd = new MySqlCommand(
                "DELETE FROM bookings WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            
            // Suoritetaan DELETE-komento ja tarkistetaan montako riviä poistettiin
            int rowsAffected = await cmd.ExecuteNonQueryAsync();
            
            // Jos mitään ei poistettu (0 riviä), varaus ei ollut olemassa
            if (rowsAffected == 0)
                return NotFound(new { error = "Varausta ei löydetty" });

            return Ok(new { message = "Varaus poistettu!" });
        }
        catch (Exception ex) {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}