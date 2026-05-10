using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MySql.Data.MySqlClient;
using ShelterBookingAPI.Models;

namespace ShelterBookingAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingController : ControllerBase {
    // Tietokantayhteys
    private readonly DatabaseHelper _db;

    // Konstruktori
    public BookingController(DatabaseHelper db) {
        _db = db;
    }

    // Hae kaikki varaukset
    [HttpGet]
    public async Task<IActionResult> GetBookings() {
        try {
            var bookings = new List<Booking>();
            using var conn = _db.GetConnection();
            await conn.OpenAsync();
            var cmd = new MySqlCommand("SELECT * FROM bookings", conn);
            var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync()) {
                bookings.Add(new Booking {
                    Id = (int)reader["id"],
                    FirstName = reader["first_name"]?.ToString() ?? string.Empty,
                    LastName = reader["last_name"]?.ToString() ?? string.Empty,
                    Email = reader["email"]?.ToString() ?? string.Empty,
                    Phone = reader["phone"]?.ToString() ?? string.Empty,
                    AnimalSpecies = reader["animal_species"]?.ToString() ?? string.Empty,
                    ServiceType = reader["service_type"]?.ToString() ?? string.Empty,
                    StartDate = reader["start_date"] != DBNull.Value ? (DateTime)reader["start_date"] : DateTime.MinValue,
                    EndDate = reader["end_date"] != DBNull.Value ? (DateTime)reader["end_date"] : DateTime.MinValue,
                    TotalPrice = reader["total_price"] != DBNull.Value ? (decimal)reader["total_price"] : 0,
                    Status = reader["status"]?.ToString() ?? "pending"
                });
            }
            return Ok(bookings);
        }
        catch (Exception ex) {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // Lisää uusi varaus
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> AddBooking([FromBody] Booking booking) {
        try {
            // Validointi: etunimi ja sukunimi
            if (string.IsNullOrWhiteSpace(booking.FirstName) || string.IsNullOrWhiteSpace(booking.LastName))
                return BadRequest(new { error = "Etunimi ja sukunimi vaaditaan" });

            // Tarkistetaan sähköposti
            if (string.IsNullOrWhiteSpace(booking.Email))
                return BadRequest(new { error = "Sähköposti vaaditaan" });

            // Tarkistetaan käyttäjä ID
            if (booking.UserId <= 0)
                return BadRequest(new { error = "Käyttäjä ID vaaditaan" });

            // Loppupäivä ei saa olla ennen alkamispäivää
            if (booking.EndDate < booking.StartDate)
                return BadRequest(new { error = "Päättymispäivä ei voi olla ennen alkamispäivää" });

            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            // Lisää uusi varaus (@-merkit suojaavat SQL-injektiolta)
            var cmd = new MySqlCommand(@"
                INSERT INTO bookings
                (user_id, first_name, last_name, email, phone, animal_species,
                 service_type, start_date, end_date, total_price, status)
                VALUES
                (@uid, @fn, @ln, @email, @phone, @species,
                 @type, @start, @end, @price, @status)", conn);

            // Aseta parametrit
            cmd.Parameters.AddWithValue("@uid", booking.UserId);
            cmd.Parameters.AddWithValue("@fn", booking.FirstName ?? string.Empty);
            cmd.Parameters.AddWithValue("@ln", booking.LastName ?? string.Empty);
            cmd.Parameters.AddWithValue("@email", booking.Email ?? string.Empty);
            cmd.Parameters.AddWithValue("@phone", booking.Phone ?? string.Empty);
            cmd.Parameters.AddWithValue("@species", booking.AnimalSpecies ?? string.Empty);
            cmd.Parameters.AddWithValue("@type", booking.ServiceType ?? string.Empty);
            cmd.Parameters.AddWithValue("@start", booking.StartDate);
            cmd.Parameters.AddWithValue("@end", booking.EndDate);
            cmd.Parameters.AddWithValue("@price", booking.TotalPrice);
            cmd.Parameters.AddWithValue("@status", booking.Status ?? "pending");

            // Suorita INSERT
            await cmd.ExecuteNonQueryAsync();

            return Ok(new { message = "Varaus lisätty!" });
        }
        catch (Exception ex) {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // Hae käyttäjän varaukset
    [Authorize]
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserBookings(int userId) {
        try {
            var bookings = new List<Booking>();
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            // Hae kaikki varaukset tähän käyttäjään
            var cmd = new MySqlCommand(
                "SELECT * FROM bookings WHERE user_id = @uid", conn);
            cmd.Parameters.AddWithValue("@uid", userId);

            // Lue tulokset
            var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync()) {
                bookings.Add(new Booking {
                    Id = (int)reader["id"],
                    FirstName = reader["first_name"]?.ToString() ?? string.Empty,
                    LastName = reader["last_name"]?.ToString() ?? string.Empty,
                    Email = reader["email"]?.ToString() ?? string.Empty,
                    Phone = reader["phone"]?.ToString() ?? string.Empty,
                    AnimalSpecies = reader["animal_species"]?.ToString() ?? string.Empty,
                    ServiceType = reader["service_type"]?.ToString() ?? string.Empty,
                    StartDate = reader["start_date"] != DBNull.Value ? (DateTime)reader["start_date"] : DateTime.MinValue,
                    EndDate = reader["end_date"] != DBNull.Value ? (DateTime)reader["end_date"] : DateTime.MinValue,
                    TotalPrice = reader["total_price"] != DBNull.Value ? (decimal)reader["total_price"] : 0,
                    Status = reader["status"]?.ToString() ?? "pending"
                });
            }

            return Ok(bookings);
        }
        catch (Exception ex) {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // Poista varaus
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBooking(int id) {
        try {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            // Poista varaus
            var cmd = new MySqlCommand(
                "DELETE FROM bookings WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);

            // Tarkista kuinka monta riviä poistettiin
            int rowsAffected = await cmd.ExecuteNonQueryAsync();

            // Varaus ei ollut olemassa
            if (rowsAffected == 0)
                return NotFound(new { error = "Varausta ei löydetty" });

            return Ok(new { message = "Varaus poistettu!" });
        }
        catch (Exception ex) {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}