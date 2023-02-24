using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Web;
using GoalspireBackend.Common;
using GoalspireBackend.Data;
using GoalspireBackend.Dto.Requests;
using GoalspireBackend.Dto.Requests.Auth;
using GoalspireBackend.Dto.Requests.Email;
using GoalspireBackend.Dto.Response.Auth;
using GoalspireBackend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace GoalspireBackend.Services;

public interface IAuthService
{
    Task<LoginResponse> Login(LoginRequest request);
    Task<IdentityResult> Register(RegisterRequest request);
    Task<Result> ConfirmEmail(ConfirmEmailRequest request);
    Task Logout();
    Task<Result> ForgotPassword(ForgotPasswordRequest request);
    Task<Result> ResetPassword(ResetPasswordRequest request);
    Task<List<User>> GetAllUsers();
}

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;
    private readonly SignInManager<User> _signInManager;
    private readonly DataContext _dataContext;

    public AuthService(UserManager<User> userManager, IConfiguration configuration, IEmailService emailService, SignInManager<User> signInManager, DataContext dataContext)
    {
        _userManager = userManager;
        _configuration = configuration;
        _emailService = emailService;
        _signInManager = signInManager;
        _dataContext = dataContext;
    }

    public async Task<Result> ConfirmEmail(ConfirmEmailRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return Result.Failure("User does not exist.");
        }

        var result = await _userManager.ConfirmEmailAsync(user, request.Token);
        if (result.Succeeded)
        {
            return Result.Success();
        }

        return Result.Failure(result.Errors.First().Description);
    }

    public async Task Logout()
    {
        await _signInManager.SignOutAsync();
    }

    public async Task<LoginResponse> Login(LoginRequest request)
    {
        //get the user from db
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return new LoginResponse
            {
                Succeeded = false
            };
        }

        if (!user.EmailConfirmed) // mby it would be better to check this with the option turned on in Program.cs. But I ain't exactly sure how...
        {
            return new LoginResponse
            {
                Succeeded = false,
                Note = "Email not confirmed."
            };
        }

        //sign in the user
        var result = await _signInManager.PasswordSignInAsync(user, request.Password, request.RememberMe, false);
        if (!result.Succeeded)
        {
            return new LoginResponse
            {
                Succeeded = false
            };
        }

        //setting up the JWT token
        var authClaims = new List<Claim>
        {
            new Claim("id", user.Id),
            new Claim("name", user.UserName),
            new Claim("email", user.Email),

            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var token = GetToken(authClaims, request.RememberMe ? DateTime.Now.AddDays(30) : DateTime.Now.AddHours(6));

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
            // create settings with default values for the user + add his timezone
            await _dataContext.Settings.AddAsync(new Settings()
            {
                UserId = Guid.Parse(user.Id),
                IanaTimeZone = request.IanaTimezoneId
            });
            await _dataContext.SaveChangesAsync();


            //sending the email
            var emailConfirmCode = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedCode = HttpUtility.UrlEncode(emailConfirmCode);
            var encodedEmail = HttpUtility.UrlEncode(user.Email);

            var confirmUrl = HtmlEncoder.Default.Encode($"{_configuration["App:BaseUrl"]}/auth/confirm-email?code={encodedCode}&email={encodedEmail}");

            await _emailService.SendEmailConfirmationEmail(new SendEmailVerificationEmailRequest
            {
                Email = user.Email,
                UserName = user.UserName,
                ConfirmURL = confirmUrl,
                IsHtml = true
            });
        }

        return result;
    }


    private JwtSecurityToken GetToken(IEnumerable<Claim> authClaims, DateTime expiration)
    {
        //create a signing key from the secret
        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]!));

        var token = new JwtSecurityToken(
            issuer: _configuration["JWT:ValidIssuer"],
            audience: _configuration["JWT:ValidAudience"],
            expires: expiration,
            claims: authClaims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );

        return token;
    }


    public async Task<Result> ForgotPassword(ForgotPasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return Result.Failure("User not found");
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);


        var encodedToken = HttpUtility.UrlEncode(token);
        var encodedEmail = HttpUtility.UrlEncode(request.Email);

        var resetUrl = HtmlEncoder.Default.Encode($"{_configuration["App:BaseUrl"]}/auth/reset-password?token={encodedToken}&email={encodedEmail}");

        await _emailService.SendForgotPasswordEmail(new ForgotPasswordEmailRequest
        {
            Email = request.Email,
            ResetUrl = resetUrl,
            UserName = user.UserName!,
        });

        return Result.Success();
    }


    public async Task<Result> ResetPassword(ResetPasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return Result.Failure("User not found");
        }

        IdentityResult passwordChangeResult = await _userManager.ResetPasswordAsync(user, request.Token, request.Password);

        //cause you can't create a new failed IdentityResult to use above. So I've gotta parse it into my own
        return new Result()
        {
            Succeeded = passwordChangeResult.Succeeded,
            Error = passwordChangeResult.Succeeded ? null : string.Join(", ", passwordChangeResult.Errors)
        };
    }

    public async Task<List<User>> GetAllUsers()
    {
        return await _userManager.Users.ToListAsync();
    }
}