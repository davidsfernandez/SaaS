using SaasAsaasApp.Models.Auth;

namespace SaasAsaasApp.Data.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterTenantAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
}
