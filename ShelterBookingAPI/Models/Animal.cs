namespace ShelterBookingAPI.Models;

/// <summary>
/// Animal-malli edustaa eläintä varaussysteemissä
/// Sisältää tiedot eläimen ominaisuuksista ja saatavuudesta
/// </summary>
public class Animal {
    /// <summary>
    /// Eläimen yksilöllinen tunnusnumero tietokannassa
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Eläimen nimi (esim. "Musti", "Napsu")
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// Eläimen laji (esim. "cat", "dog", "rabbit")
    /// </summary>
    public string? Species { get; set; }
    
    /// <summary>
    /// Eläimen rotu (esim. "Finnish Lapphund", "Siamese")
    /// </summary>
    public string? Breed { get; set; }
    
    /// <summary>
    /// Eläimen ikä vuosina
    /// </summary>
    public int Age { get; set; }
    
    /// <summary>
    /// Onko eläin saatavilla varattavaksi? true = saatavilla, false = varattu
    /// </summary>
    public bool Available { get; set; }
}