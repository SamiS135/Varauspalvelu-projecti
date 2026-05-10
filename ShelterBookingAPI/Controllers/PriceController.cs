using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using ShelterBookingAPI.Models;

namespace ShelterBookingAPI.Controllers;

/// <summary>
/// PriceController hoitaa hintoihin liittyvät API-pyynnöt
/// Käyttäjät voivat hakea hintoja lajin ja palvelun mukaan
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PriceController : ControllerBase {
    // _db on DatabaseHelper-olio jota käytetään tietokantayhteyden luomiseen
    private readonly DatabaseHelper _db;

    /// <summary>
    /// Konstruktori - vastaanottaa DatabaseHelper-olion
    /// </summary>
    public PriceController(DatabaseHelper db) {
        _db = db;
    }

    /// <summary>
    /// GET-pyyntö: Noutaa hinnat tietyn lajin ja palvelun perusteella
    /// Osoite: http://localhost:5000/api/price/{species}/{serviceType}
    /// Esimerkki: http://localhost:5000/api/price/cat/grooming
    /// </summary>
    /// <param name="species">Eläimen laji (kissa, koira jne)</param>
    /// <param name="serviceType">Palvelutyyppi (hoito, koulutus jne)</param>
    [HttpGet("{species}/{serviceType}")]
    public async Task<IActionResult> GetPrices(string species, string serviceType) {
        try {
            // VALIDOINTI: Tarkistetaan että laji ja palvelutyyppi on annettu
            if (string.IsNullOrWhiteSpace(species) || string.IsNullOrWhiteSpace(serviceType))
                return BadRequest(new { error = "Laji ja palvelutyyppi vaaditaan" });

            var prices = new List<Price>();
            using var conn = _db.GetConnection();
            await conn.OpenAsync();
            
            // SQL-kysely: etsitään hinnat jotka vastaavat lajia ja palvelutyyppiä
            var cmd = new MySqlCommand(
                "SELECT * FROM prices WHERE species = @species AND service_type = @type", conn);
            
            // Asetetaan parametrit turvallisesti (SQL-injektio-suoja)
            cmd.Parameters.AddWithValue("@species", species);
            cmd.Parameters.AddWithValue("@type", serviceType);
            
            var reader = await cmd.ExecuteReaderAsync();

            // Käydään läpi kaikki haetut hinnat
            while (await reader.ReadAsync()) {
                prices.Add(new Price {
                    Id          = (int)reader["id"],                                 // Hinnan tunnusnumero
                    Species     = reader["species"]?.ToString() ?? string.Empty,     // Laji (kissa, koira jne)
                    ServiceType = reader["service_type"]?.ToString() ?? string.Empty, // Palvelutyyppi
                    ServiceName = reader["service_name"]?.ToString() ?? string.Empty, // Palvelun nimi
                    Amount      = reader["amount"] != DBNull.Value ? (decimal)reader["amount"] : 0 // Hinta euroin
                });
            }
            
            return Ok(prices);
        }
        catch (Exception ex) {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}