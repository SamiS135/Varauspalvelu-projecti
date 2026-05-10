namespace ShelterBookingAPI.Models;

public class Price {
    public int Id { get; set; } // Hinnan ID
    public string? Species { get; set; } // Eläimen laji (cat/dog)
    public string? ServiceType { get; set; } // Palvelutyyppi (hoitola/hotelli)
    public string? ServiceName { get; set; } // Palvelun nimi suomeksi
    public decimal Amount { get; set; } // Hinta euroissa
}