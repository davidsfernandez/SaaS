using SaasAsaasApp.Models.Auth;

namespace SaasAsaasApp.Data.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterTenantAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> ForgotPasswordAsync(string email);
    Task<AuthResponse> ResetPasswordAsync(ResetPasswordRequest request);
}
