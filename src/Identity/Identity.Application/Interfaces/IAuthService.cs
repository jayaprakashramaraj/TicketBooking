using System.Threading.Tasks;
using Identity.Application.DTOs;

namespace Identity.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse?> RegisterAsync(RegisterRequest request);
        Task<AuthResponse?> LoginAsync(LoginRequest request);
    }
}
