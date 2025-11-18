using Api.Models;

namespace Api.Interfaces.IService
{
    public interface ITokenService
    {
        string CreateToken(AppUser user);
    }
}