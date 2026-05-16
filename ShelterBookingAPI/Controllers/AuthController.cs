using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using ShelterBookingAPI.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ShelterBookingAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase {
    // Salasalaisanalle
    private readonly DatabaseHelper db;
    private readonly string secretKey;

    // Konstruktori
    public AuthController(DatabaseHelper db, IConfiguration configuration) {
        this.db = db;
        secretKey = configuration["Jwt:SecretKey"]
            ?? throw new InvalidOperationException("Jwt:SecretKey puuttuu asetuksista.");
    }

    // Rekisteröi uusi käyttäjä
    [HttpPost("register")]
    public IActionResult Register([FromBody] User user) {
        using var conn = db.GetConnection();
        conn.Open();

        // Tarkista onko sähköposti jo käytössä
        var check = new MySqlCommand(
            "SELECT COUNT(*) FROM users WHERE email = @email", conn);
        check.Parameters.AddWithValue("@email", user.Email);
        if ((long)check.ExecuteScalar() > 0)
            return BadRequest(new { message = "Sähköposti on jo käytössä!" });

        // Lisää uusi käyttäjä tietokantaan ja salaa salasana
        var cmd = new MySqlCommand(@"
            INSERT INTO users (username, email, password, phone)
            VALUES (@username, @email, @password, @phone); 
            SELECT LAST_INSERT_ID();", conn);
        cmd.Parameters.AddWithValue("@username", user.Username ?? "");
        cmd.Parameters.AddWithValue("@email",    user.Email);
        cmd.Parameters.AddWithValue("@password", BCrypt.Net.BCrypt.HashPassword(user.Password));
        cmd.Parameters.AddWithValue("@phone",    user.Phone ?? "");

        var newUserId = cmd.ExecuteScalar();

        // Palauta uuden käyttäjän ID
        return Ok(new { 
            message = "Rekisteröityminen onnistui!", 
            userId = newUserId,
            email = user.Email
        });
    }

    // Kirjaudu sisään
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request) {
        using var conn = db.GetConnection();
        conn.Open();

        // Hae käyttäjä sähköpostin perusteella
        var cmd = new MySqlCommand(
            "SELECT * FROM users WHERE email = @email", conn);
        cmd.Parameters.AddWithValue("@email", request.Email);
        var reader = cmd.ExecuteReader();

        // Käyttäjä ei löytynyt
        if (!reader.Read())
            return Unauthorized(new { message = "Väärä sähköposti tai salasana!" });

        // Luo User-objekti
        var user = new User {
            Id       = (int)reader["id"],
            Username = reader["username"].ToString(),
            Email    = reader["email"].ToString(),
            Password = reader["password"].ToString(),
            Phone    = reader["phone"].ToString()
        };
        reader.Close();

        // Tarkista salasana
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            return Unauthorized(new { message = "Väärä sähköposti tai salasana!" });

        // Luo JWT-token
        var token = LuoToken(user);

        return Ok(new LoginResponse {
            UserId   = user.Id,
            Username = user.Username,
            Email    = user.Email,
            Token    = token
        });
    }

    // Luo JWT-token käyttäjälle
    private string LuoToken(User user) {
        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        // Tokeniin tallennetaan userId ja username
        var claims = new[] {
            new Claim("userId",   user.Id.ToString()),
            new Claim("username", user.Username ?? "")
        };
        // Token voimassa 7 päivää
        var token = new JwtSecurityToken(
            claims:            claims,
            expires:           DateTime.Now.AddDays(7),
            signingCredentials: creds
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}