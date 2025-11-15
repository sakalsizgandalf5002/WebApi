using System.ComponentModel.DataAnnotations;

namespace Api.Options;

public sealed class JwtOptions
{
    [Required]
    public string Issuer { get; init; } = default!;
    [Required]
    public string Audience  { get; init; } = default!;
    [Required, MinLength(64)]
    public string SigningKey { get; init; } = default!;
    
}