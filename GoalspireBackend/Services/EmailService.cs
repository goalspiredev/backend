using FluentEmail.Core;
using GoalspireBackend.Common;
using GoalspireBackend.Dto.Requests.Email;
using GoalspireBackend.Models;
using Org.BouncyCastle.Asn1.Ocsp;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GoalspireBackend.Services;

public interface IEmailService
{
    Task<Result> SendEmail(SendEmailRequest request);
    Task<Result> SendEmailConfirmationEmail(SendEmailVerificationEmailRequest request);

    Task<Result> SendForgotPasswordEmail(ForgotPasswordEmailRequest request);
}

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly IFluentEmail _email;

    public EmailService(ILogger<EmailService> logger, IFluentEmail email)
    {
        _logger = logger;
        _email = email;
    }

    public async Task<Result> SendEmail(SendEmailRequest request)
    {
        _logger.LogInformation($"Sending an email to {request.Email} with title: {request.Title}");

        var email = await _email
            .To(request.Email)
            .Subject(request.Title)
            .Body(request.Content, request.IsHtml)
            .SendAsync();

        if (!email.Successful)
        {
            //throw new Exception(string.Join("; ", email.ErrorMessages));

            return Result.Failure(string.Join(", ", email.ErrorMessages));
        }

        return Result.Success();
    }

    public async Task<Result> SendEmailConfirmationEmail(SendEmailVerificationEmailRequest request)
    {
        string confirmEmailHtmlPath = "./EmailTemplates/VerifyEmail.html";
        string confirmEmailTxtPath = "./EmailTemplates/VerifyEmail.txt";


        Result res = await SendEmail(new SendEmailRequest
        {
            Email = request.Email,
            Title = request.Title,
            IsHtml = request.IsHtml,
            Content = File.ReadAllText(request.IsHtml ? confirmEmailHtmlPath : confirmEmailTxtPath).Replace("%%UserName%%", request.UserName).Replace("%%confirmUrl%%", request.ConfirmURL)
        });
        return res;
    }

    public async Task<Result> SendForgotPasswordEmail(ForgotPasswordEmailRequest request)
    {
        string forgotPasswordHtmlEmail = "./EmailTemplates/ForgotPassword.html";

        return await SendEmail(new SendEmailRequest
        {
            Email = request.Email,
            Title = "Reset your password",
            Content = File.ReadAllText(forgotPasswordHtmlEmail).Replace("%%ResetUrl%%", request.ResetUrl).Replace("%%UserName%%", request.UserName),
            IsHtml = true
        });
    }
}