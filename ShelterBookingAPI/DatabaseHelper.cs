using MySql.Data.MySqlClient;

namespace ShelterBookingAPI;

public class DatabaseHelper {
    // Yhteyden merkkijono appsettings.json:sta
    private readonly string _connStr;

    // Konstruktori - lue yhteyden merkkijono konfiguraatiosta
    public DatabaseHelper(IConfiguration config) {
        _connStr = config.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string not found");
    }

    // Luo uusi tietokantayhteys
    public MySqlConnection GetConnection() {
        return new MySqlConnection(_connStr);
    }
}