using Sanad.Models.Data;

namespace Sanad.Services
{
    public interface ITokenService
    {
        string GenerateToken(User user);
    }
}
