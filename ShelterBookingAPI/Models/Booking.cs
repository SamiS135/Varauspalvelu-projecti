namespace ShelterBookingAPI.Models;

/// <summary>
/// Booking-malli edustaa varaustoimitusta varaussysteemissä
/// Sisältää tiedot asiakkaasta, varattavasta eläimestä ja varauksen yksityiskohdista
/// </summary>
public class Booking {
    /// <summary>
    /// Varauksen yksilöllinen tunnusnumero tietokannassa
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Asiakkaan etunimi
    /// </summary>
    public string? FirstName { get; set; }
    
    /// <summary>
    /// Asiakkaan sukunimi
    /// </summary>
    public string? LastName { get; set; }
    
    /// <summary>
    /// Asiakkaan sähköpostiosoite (yhteystieto)
    /// </summary>
    public string? Email { get; set; }
    
    /// <summary>
    /// Asiakkaan puhelinnumero (yhteystieto)
    /// </summary>
    public string? Phone { get; set; }
    
    /// <summary>
    /// Viittaus varattavaan eläimeen (Animal.Id)
    /// </summary>
    public int AnimalId { get; set; }
    
    /// <summary>
    /// Palvelutyyppi (esim. "grooming"=kylpeminen, "training"=koulutus, "daycare"=päivähoito)
    /// </summary>
    public string? ServiceType { get; set; }
    
    /// <summary>
    /// Varauksen alkamispäivä ja aika
    /// </summary>
    public DateTime StartDate { get; set; }
    
    /// <summary>
    /// Varauksen päättymispäivä ja aika
    /// </summary>
    public DateTime EndDate { get; set; }
    
    /// <summary>
    /// Varauksen kokonaishinta euroissa
    /// </summary>
    public decimal TotalPrice { get; set; }
    
    /// <summary>
    /// Varauksen tila oletuksena "pending" (odottava)
    /// Mahdolliset arvot: "pending", "confirmed", "completed", "cancelled"
    /// </summary>
    public string? Status { get; set; } = "pending";
}