namespace Api.DTOs.Account;

public class AuthResponseDto
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public string AccessToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpires { get; set; }

    public string RefreshToken { get; set; } = string.Empty;
    public DateTime RefreshTokenExpires { get; set; }

    public List<string> Roles { get; set; } = new();
}