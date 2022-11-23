using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using GoalspireBackend.Dto.Requests;
using GoalspireBackend.Dto.Requests.Auth;
using GoalspireBackend.Dto.Requests.Email;
using GoalspireBackend.Dto.Response.Auth;
using GoalspireBackend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.Tokens;

namespace GoalspireBackend.Services;

public interface IAuthService
{
    Task<LoginResponse> Login(LoginRequest request);
    Task<IdentityResult> Register(RegisterRequest request);
    Task Logout();
}

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;
    private readonly SignInManager<User> _signInManager;

    public AuthService(UserManager<User> userManager, IConfiguration configuration, IEmailService emailService, SignInManager<User> signInManager)
    {
        _userManager = userManager;
        _configuration = configuration;
        _emailService = emailService;
        _signInManager = signInManager;
    }

    public async Task Logout()
    {
        await _signInManager.SignOutAsync();
    }

    public async Task<LoginResponse> Login(LoginRequest request)
    {
        var user = await _userManager.FindByNameAsync(request.Login);
        if (user == null)
        {
            return new LoginResponse
            {
                Succeeded = false
            };
        }
        
        var result = await _signInManager.PasswordSignInAsync(user, request.Password, request.RememberMe, false);
        if (!result.Succeeded)
        {
            return new LoginResponse
            {
                Succeeded = false
            };
        }
        
       
        var authClaims = new List<Claim>
        {
            new Claim("id", user.Id),
            new Claim("name", user.UserName),
            new Claim("email", user.Email),
            
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var token = GetToken(authClaims);
        return new LoginResponse
        {
            Succeeded = true,
            Token = new JwtSecurityTokenHandler().WriteToken(token)
        };
    }

    public async Task<IdentityResult> Register(RegisterRequest request)
    {
        var user = new User
        {
            UserName = request.Username,
            Email = request.Email
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (result.Succeeded)
        {
            var emailConfirmCode = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(emailConfirmCode));
            var encodedEmail = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(user.Email));

            var confirmUrl = HtmlEncoder.Default.Encode($"{_configuration["App:BaseUrl"]}/auth/confirm-email?code={encodedCode}&email=${encodedEmail}");
            await _emailService.SendEmail(new SendEmailRequest
            {
                Email = user.Email,
                Title = "Email confirmation",
                Content = $"Hi {user.UserName}, your verification link is: {confirmUrl}"
            });
        }

        return result;
    }
    
    private JwtSecurityToken GetToken(IEnumerable<Claim> authClaims)
    {
        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]!));

        var token = new JwtSecurityToken(
            issuer: _configuration["JWT:ValidIssuer"],
            audience: _configuration["JWT:ValidAudience"],
            expires: DateTime.Now.AddDays(30),
            claims: authClaims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );

        return token;
    }
}