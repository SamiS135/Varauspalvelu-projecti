namespace ShelterBookingAPI.Models;

/// <summary>
/// Price-malli edustaa palvelun hintaa varaussysteemissä
/// Sisältää tiedot eri eläimiä koskevien eri palveluiden hinnoista
/// </summary>
public class Price {
    /// <summary>
    /// Hinnan yksilöllinen tunnusnumero tietokannassa
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Eläimen laji johon hinta kohdistuu (esim. "cat", "dog")
    /// </summary>
    public string? Species { get; set; }
    
    /// <summary>
    /// Palvelutyyppi (esim. "grooming"=kylpeminen, "training"=koulutus)
    /// </summary>
    public string? ServiceType { get; set; }
    
    /// <summary>
    /// Palvelun nimi suomeksi (esim. "Koiran kylpy ja tuoksu")
    /// </summary>
    public string? ServiceName { get; set; }
    
    /// <summary>
    /// Hinnan suuruus euroissa
    /// </summary>
    public decimal Amount { get; set; }
}