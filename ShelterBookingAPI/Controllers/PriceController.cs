using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using ShelterBookingAPI.Models;

namespace ShelterBookingAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PriceController : ControllerBase {
    // Tietokantayhteys
    private readonly DatabaseHelper _db;

    // Konstruktori
    public PriceController(DatabaseHelper db) {
        _db = db;
    }

    // Hae hinnat lajin ja palvelun mukaan
    [HttpGet("{species}/{serviceType}")]
    public async Task<IActionResult> GetPrices(string species, string serviceType) {
        try {
            // Validointi
            if (string.IsNullOrWhiteSpace(species) || string.IsNullOrWhiteSpace(serviceType))
                return BadRequest(new { error = "Laji ja palvelutyyppi vaaditaan" });

            var prices = new List<Price>();
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            // Etsii hinnat lajin ja palvelun perusteella
            var cmd = new MySqlCommand(
                "SELECT * FROM prices WHERE species = @species AND service_type = @type", conn);

            // Aseta parametrit (@-merkit suojaavat SQL-injektiolta)
            cmd.Parameters.AddWithValue("@species", species);
            cmd.Parameters.AddWithValue("@type", serviceType);

            // Lue tulokset
            var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync()) {
                prices.Add(new Price {
                    Id = (int)reader["id"],
                    Species = reader["species"]?.ToString() ?? string.Empty,
                    ServiceType = reader["service_type"]?.ToString() ?? string.Empty,
                    ServiceName = reader["service_name"]?.ToString() ?? string.Empty,
                    Amount = reader["amount"] != DBNull.Value ? (decimal)reader["amount"] : 0
                });
            }

            return Ok(prices);
        }
        catch (Exception ex) {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}