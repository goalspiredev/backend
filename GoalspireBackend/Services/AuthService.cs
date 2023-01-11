﻿using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using GoalspireBackend.Common;
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
    Task<Result> ConfirmEmail(ConfirmEmailRequest request);
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
            return Result.Success;
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
        var user = await _userManager.FindByNameAsync(request.Login);
        if (user == null)
        {
            return new LoginResponse
            {
                Succeeded = false
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
            //sending the email
            var emailConfirmCode = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(emailConfirmCode));
            var encodedEmail = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(user.Email));

            var confirmUrl = HtmlEncoder.Default.Encode($"{_configuration["App:BaseUrl"]}/auth/confirm-email?code={encodedCode}&email={encodedEmail}");
            string confirmEmailHtmlPath = "./EmailTemplates/VerifyEmail.html";

            await _emailService.SendEmail(new SendEmailRequest
            {
                Email = user.Email,
                Title = "Email confirmation",
                Content = File.ReadAllText(confirmEmailHtmlPath).Replace("%%UserName%%", user.UserName).Replace("%%confirmUrl%%", confirmUrl),
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
}