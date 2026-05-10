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
    private readonly DatabaseHelper db;
    private string secretKey = "LemmikkihoitolaHaapasenHuvila2024!";

    public AuthController(DatabaseHelper db) {
        this.db = db;
    }

    [HttpPost("register")]
    public IActionResult Register([FromBody] User user) {
        using var conn = db.GetConnection();
        conn.Open();

        var check = new MySqlCommand(
            "SELECT COUNT(*) FROM users WHERE email = @email", conn);
        check.Parameters.AddWithValue("@email", user.Email);
        if ((long)check.ExecuteScalar() > 0)
            return BadRequest(new { message = "Sähköposti on jo käytössä!" });

        var cmd = new MySqlCommand(@"
            INSERT INTO users (username, email, password, phone)
            VALUES (@username, @email, @password, @phone); 
            SELECT LAST_INSERT_ID();", conn);
        cmd.Parameters.AddWithValue("@username", user.Username ?? "");
        cmd.Parameters.AddWithValue("@email",    user.Email);
        cmd.Parameters.AddWithValue("@password", BCrypt.Net.BCrypt.HashPassword(user.Password));
        cmd.Parameters.AddWithValue("@phone",    user.Phone ?? "");
        
        var newUserId = cmd.ExecuteScalar();
        
        return Ok(new { 
            message = "Rekisteröityminen onnistui!", 
            userId = newUserId,
            email = user.Email
        });
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request) {
        using var conn = db.GetConnection();
        conn.Open();
        var cmd = new MySqlCommand(
            "SELECT * FROM users WHERE email = @email", conn);
        cmd.Parameters.AddWithValue("@email", request.Email);
        var reader = cmd.ExecuteReader();

        if (!reader.Read())
            return Unauthorized(new { message = "Väärä sähköposti tai salasana!" });

        var user = new User {
            Id       = (int)reader["id"],
            Username = reader["username"].ToString(),
            Email    = reader["email"].ToString(),
            Password = reader["password"].ToString(),
            Phone    = reader["phone"].ToString()
        };
        reader.Close();

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            return Unauthorized(new { message = "Väärä sähköposti tai salasana!" });

        var token = LuoToken(user);

        return Ok(new LoginResponse {
            UserId   = user.Id,
            Username = user.Username,
            Email    = user.Email,
            Token    = token
        });
    }

    private string LuoToken(User user) {
        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[] {
            new Claim("userId",   user.Id.ToString()),
            new Claim("username", user.Username ?? "")
        };
        var token = new JwtSecurityToken(
            claims:            claims,
            expires:           DateTime.Now.AddDays(7),
            signingCredentials: creds
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}