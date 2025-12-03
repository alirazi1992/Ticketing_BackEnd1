using Ticketing.Api.Models;

namespace Ticketing.Api.Services
{
    public interface ITokenService
    {
        string CreateToken(User user);
    }
}
