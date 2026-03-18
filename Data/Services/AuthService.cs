using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
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
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        IAsaasService asaasService,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _context = context;
        _asaasService = asaasService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<AuthResponse> RegisterTenantAsync(RegisterRequest request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // 1. Create the Tenant
            var tenant = new Tenant
            {
                Id = Guid.NewGuid(),
                Name = request.CompanyName,
                Slug = request.CompanyName.ToLower().Replace(" ", "-"),
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
