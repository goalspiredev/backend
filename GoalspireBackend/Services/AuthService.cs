using System.Text;
using System.Text.Encodings.Web;
using GoalspireBackend.Dto.Requests;
using GoalspireBackend.Dto.Requests.Auth;
using GoalspireBackend.Dto.Requests.Email;
using GoalspireBackend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;

namespace GoalspireBackend.Services;

public interface IAuthService
{
    Task<IdentityResult> Login(LoginRequest request);
    Task<IdentityResult> Register(RegisterRequest request);
}

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;

    public AuthService(UserManager<User> userManager, IConfiguration configuration, IEmailService emailService)
    {
        _userManager = userManager;
        _configuration = configuration;
        _emailService = emailService;
    }

    public Task<IdentityResult> Login(LoginRequest request)
    {
        throw new NotImplementedException();
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
}