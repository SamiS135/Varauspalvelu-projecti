namespace ShelterBookingAPI.Models;

public class User {
    public int Id { get; set; } // Käyttäjän ID
    public string? Username { get; set; } // Käyttäjätunnus
    public string? Email { get; set; } // Sähköposti
    public string? Password { get; set; } // Salasana (salattu)
    public string? Phone { get; set; } // Puhelinnumero
}

public class LoginRequest {
    public string? Email { get; set; } // Kirjautumiseen käytetty sähköposti
    public string? Password { get; set; } // Kirjautumiseen käytetty salasana
}

public class LoginResponse {
    public int UserId { get; set; } // Käyttäjän ID
    public string? Username { get; set; } // Käyttäjätunnus
    public string? Email { get; set; } // Käyttäjän sähköposti
    public string? Token { get; set; } // JWT-token
}