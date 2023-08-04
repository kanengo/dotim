using System.ComponentModel.DataAnnotations;

namespace ApiGate.Models;

public class LoginRequest
{
    [Required]
    public string Username { get; }
    
    [Required]
    public string Password { get; }
    
    [Required]
    public LoginType Type { get; }

    public LoginRequest(string username, string password, LoginType type)
    {
        Username = username;
        Password = password;
        Type = type;
    }

}