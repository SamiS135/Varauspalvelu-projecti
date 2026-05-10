namespace ShelterBookingAPI.Models;

public class User {
    public int Id { get; set; }
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? Phone { get; set; }
}

public class LoginRequest {
    public string? Email { get; set; }
    public string? Password { get; set; }
}

public class LoginResponse {
    public int UserId { get; set; }
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? Token { get; set; }
}