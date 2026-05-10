using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using ShelterBookingAPI.Models;

namespace ShelterBookingAPI.Controllers;

/// <summary>
/// AnimalController hoitaa kaikki eläimiin liittyvät API-pyynnöt
/// Käyttäjät voivat hakea saatavilla olevia eläimiä
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AnimalController : ControllerBase {
    // _db on DatabaseHelper-olio jota käytetään tietokantayhteyden luomiseen
    // readonly tarkoittaa että sitä ei voi muuttaa konstruktorin jälkeen
    private readonly DatabaseHelper _db;

    /// <summary>
    /// Konstruktori - vastaanottaa DatabaseHelper-olion riippuvuuden injektoinnin kautta
    /// </summary>
    /// <param name="db">DatabaseHelper-olio tietokantayhteyksille</param>
    public AnimalController(DatabaseHelper db) {
        _db = db;
    }

    /// <summary>
    /// GET-pyyntö: Noutaa kaikki saatavilla olevat eläimet
    /// Osoite: http://localhost:5000/api/animal
    /// </summary>
    /// <returns>JSON-lista eläimistä</returns>
    [HttpGet]
    public async Task<IActionResult> GetAnimals() {
        try {
            var animals = new List<Animal>();
            
            // Luodaan yhteys tietokantaan
            using var conn = _db.GetConnection();
            await conn.OpenAsync();
            
            // SQL-kysely: haetaan kaikki eläimet joiden available = TRUE
            var cmd = new MySqlCommand(
                "SELECT * FROM animals WHERE available = TRUE", conn);
            
            // Suoritetaan kysely ja luetaan tulokset
            var reader = await cmd.ExecuteReaderAsync();
            
            // Käydään läpi kaikki haetut rivit
            while (await reader.ReadAsync()) {
                animals.Add(new Animal {
                    // reader["sarakkeen_nimi"] lukee tietyn sarakkeen arvon nykyiseltä riviltä
                    Id        = (int)reader["id"],                               // Eläimen tunnusnumero
                    Name      = reader["name"]?.ToString() ?? string.Empty,      // Eläimen nimi
                    Species   = reader["species"]?.ToString() ?? string.Empty,   // Laji (kissa, koira jne)
                    Breed     = reader["breed"]?.ToString() ?? string.Empty,     // Rotu
                    Age       = reader["age"] != DBNull.Value ? (int)reader["age"] : 0, // Ikä
                    Available = (bool)reader["available"]                        // Onko saatavilla?
                });
            }
            
            // Palautetaan eläinten lista HTTP 200 OK -vasteella
            return Ok(animals);
        }
        catch (Exception ex) {
            // Jos tapahtuu virhe, palautetaan HTTP 500 virhekoodi
            return StatusCode(500, new { error = ex.Message });
        }
    }
}