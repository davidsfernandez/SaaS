using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SaasAsaasApp.Data.Entities;
using SaasAsaasApp.Data.Enums;
using SaasAsaasApp.Data.Interfaces;
using SaasAsaasApp.Models.Auth;
using SaasAsaasApp.Models.Asaas;

namespace SaasAsaasApp.Data.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly IAsaasService _asaasService;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        IAsaasService asaasService,
        IEmailService emailService,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _context = context;
        _asaasService = asaasService;
        _emailService = emailService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<AuthResponse> RegisterTenantAsync(RegisterRequest request)
    {
        var slug = request.CompanyName.ToLower().Replace(" ", "-");
        
        // Check if slug already exists to avoid DB exception
        var existingTenant = await _context.Tenants
            .IgnoreQueryFilters()
            .AnyAsync(t => t.Slug == slug);

        if (existingTenant)
        {
            return new AuthResponse { Success = false, Message = "A company with this name already exists. Please choose another one." };
        }

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // 1. Create the Tenant
            var tenant = new Tenant
            {
                Id = Guid.NewGuid(),
                Name = request.CompanyName,
                Slug = slug,
                Email = request.Email,
                TaxId = request.TaxId,
                CurrentPlanId = request.PlanId,
                Status = TenantStatus.PendingPayment
            };

            _context.Tenants.Add(tenant);
            await _context.SaveChangesAsync();

            // 2. Create the User linked to the Tenant
            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FullName = request.FullName,
                TenantId = tenant.Id
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                return new AuthResponse 
                { 
                    Success = false, 
                    Message = string.Join(", ", result.Errors.Select(e => e.Description)) 
                };
            }

            // 3. Create Customer in Asaas API
            var asaasCustomer = await _asaasService.CreateCustomerAsync(new AsaasCustomerRequest
            {
                Name = request.CompanyName,
                Email = request.Email,
                CpfCnpj = request.TaxId,
                ExternalReference = tenant.Id.ToString()
            });

            if (asaasCustomer != null)
            {
                tenant.AsaasCustomerId = asaasCustomer.Id;
                await _context.SaveChangesAsync();
            }
            else
            {
                _logger.LogWarning("Tenant {TenantId} created but Asaas Customer creation failed.", tenant.Id);
            }

            await transaction.CommitAsync();

            // 4. Trigger Welcome Email (Non-blocking)
            _ = Task.Run(async () =>
            {
                try
                {
                    var placeholders = new Dictionary<string, string>
                    {
                        { "CompanyName", "SaasAsaasApp" }, // Or get from config
                        { "UserName", user.FullName ?? user.Email! }
                    };

                    await _emailService.SendEmailAsync(user.Email!, "Welcome to SaasAsaasApp!", "welcome.html", placeholders);
                    _logger.LogInformation("Welcome email triggered for {Email}", user.Email);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending welcome email to {Email}", user.Email);
                }
            });

            return new AuthResponse
            {
                Success = true,
                Message = "Registration successful",
                Token = GenerateJwtToken(user)
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error during tenant registration");
            return new AuthResponse { Success = false, Message = "An internal error occurred during registration." };
        }
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
        {
            return new AuthResponse { Success = false, Message = "Invalid credentials" };
        }

        return new AuthResponse
        {
            Success = true,
            Token = GenerateJwtToken(user)
        };
    }

    public async Task<AuthResponse> ForgotPasswordAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            // We return success to avoid email enumeration attacks
            return new AuthResponse { Success = true, Message = "If the email exists, a reset link has been sent." };
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        
        // Trigger Email (Non-blocking)
        _ = Task.Run(async () =>
        {
            try
            {
                var resetUrl = $"{_configuration["AppUrl"]}/Account/ResetPassword?email={Uri.EscapeDataString(user.Email!)}&token={Uri.EscapeDataString(token)}";
                var placeholders = new Dictionary<string, string>
                {
                    { "ResetUrl", resetUrl }
                };

                await _emailService.SendEmailAsync(user.Email!, "Reset your Password", "password-reset.html", placeholders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending password reset email to {Email}", user.Email);
            }
        });

        return new AuthResponse { Success = true, Message = "Reset link sent successfully." };
    }

    public async Task<AuthResponse> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return new AuthResponse { Success = false, Message = "Invalid request." };
        }

        var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (result.Succeeded)
        {
            return new AuthResponse { Success = true, Message = "Password has been reset successfully." };
        }

        return new AuthResponse 
        { 
            Success = false, 
            Message = string.Join(", ", result.Errors.Select(e => e.Description)) 
        };
    }

    private string GenerateJwtToken(ApplicationUser user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var key = Encoding.ASCII.GetBytes(jwtSettings["Secret"]!);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim("TenantId", user.TenantId.ToString()),
                new Claim("FullName", user.FullName ?? "")
            }),
            Expires = DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpiryMinutes"]!)),
            Issuer = jwtSettings["Issuer"],
            Audience = jwtSettings["Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
