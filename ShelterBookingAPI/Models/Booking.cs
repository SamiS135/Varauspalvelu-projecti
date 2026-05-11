namespace ShelterBookingAPI.Models;

public class Booking {
    public int Id { get; set; } // Varauksen ID
    public string? FirstName { get; set; } // Asiakkaan etunimi
    public string? LastName { get; set; } // Asiakkaan sukunimi
    public string? Email { get; set; } // Asiakkaan sähköposti
    public string? Phone { get; set; } // Asiakkaan puhelinnumero
    public int UserId { get; set; } // Käyttäjän ID
    public string? AnimalSpecies { get; set; } // Eläimen laji (cat/dog)
    public string? ServiceType { get; set; } // Palvelutyyppi (hoitola/hotelli)
    public DateTime StartDate { get; set; } // Varauksen alkamispäivä
    public DateTime EndDate { get; set; } // Varauksen loppupäivä
    public decimal TotalPrice { get; set; } // Kokonaishinta
    public string? Status { get; set; } = "pending"; // Tila: pending/confirmed/completed/cancelled
}