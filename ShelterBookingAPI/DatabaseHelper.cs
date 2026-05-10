using MySql.Data.MySqlClient;

namespace ShelterBookingAPI;

/// <summary>
/// DatabaseHelper-luokka hallinnoi tietokantayhteyksiä MySQL-kantaan
/// Tämä luokka luo MySQL-yhteyden käyttäen appsettings.json-tiedostosta 
/// luettavia yhteysparametreja
/// </summary>
public class DatabaseHelper {
    // Yhteyden merkkijono - sisältää palvelimen osoitteen, käyttäjänimen, salasanan jne
    private readonly string _connStr;

    /// <summary>
    /// Konstruktori - vastaanottaa konfiguraatiotiedot sovellukselta
    /// </summary>
    /// <param name="config">IConfiguration sisältää appsettings.json-tiedoston asetukset</param>
    public DatabaseHelper(IConfiguration config) {
        // Luetaan yhteyden merkkijono konfiguraatiosta (ConnectionStrings:DefaultConnection)
        // Jos merkkijonoa ei löydy, heitetään virhe
        _connStr = config.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string not found");
    }

    /// <summary>
    /// Luodaan uusi MySQL-yhteys käyttäen yhteysmerkkijonoa
    /// </summary>
    /// <returns>MySqlConnection-olio jota voidaan käyttää SQL-komentojen suorittamiseen</returns>
    public MySqlConnection GetConnection() {
        return new MySqlConnection(_connStr);
    }
}